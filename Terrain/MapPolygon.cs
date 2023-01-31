using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public partial class MapPolygon : Entity
{
    public Vector2 Center { get; protected set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public List<Vector2> NoNeighborBorders { get; protected set; }
    public Color Color { get; protected set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public float SettlementSize { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public MapPolygonBorder GetBorder(MapPolygon neighbor, Data data) 
        => data.Planet.PolyBorders.GetBorder(this, neighbor);

    public MapPolygon(int id, Vector2 center, EntityRefCollection<MapPolygon> neighbors, 
        List<Vector2> noNeighborBorders, Color color, float altitude, float roughness, 
        float moisture, float settlementSize, EntityRef<Regime> regime) : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        NoNeighborBorders = noNeighborBorders;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        SettlementSize = settlementSize;
        Regime = regime;
    }

    public MapPolygon(int id, Vector2 center, float mapWidth, GenWriteKey key) : base(id, key)
    {
        Center = center;
        if (Center.x > mapWidth) Center = new Vector2(Center.x - mapWidth, center.y);
        if (Center.x < 0f) Center = new Vector2(Center.x + mapWidth, center.y);
        Neighbors = EntityRefCollection<MapPolygon>.Construct(new List<int>(), key);
        NoNeighborBorders = new List<Vector2>();
        Color = ColorsExt.GetRandomColor();
        Regime = new EntityRef<Regime>(-1);
    }
    public bool HasNeighbor(MapPolygon p)
    {
        return Neighbors.Refs().Contains(p);
    }
    
    public IEnumerable<MapPolygonBorder> GetNeighborBorders(Data data) => Neighbors.Refs().Select(n => GetBorder(n, data));
    public void AddNeighbor(MapPolygon poly, MapPolygonBorder border, GenWriteKey key)
    {
        if (Neighbors.Contains(poly)) return;
        Neighbors.AddRef(poly, key.Data);
        var startN = Neighbors.Refs().ElementAt(0);
        for (int i = 0; i < Neighbors.Count(); i++)
        {
            if (startN.Neighbors.Refs().Any(n => Neighbors.Contains(n)))
            {
                startN = Neighbors.Refs().ElementAt((i + 1) % Neighbors.Count());
            }
            else break;
        }
        //todo make it ordered
        var ns = Neighbors.Refs()
            .OrderByClockwise(Vector2.Zero, 
                n => GetBorder(n, key.Data).GetOffsetToOtherPoly(this),
                startN).Select(p => p.Id)
            .ToList();
        Neighbors = EntityRefCollection<MapPolygon>.Construct(ns, key);
    }
    public void RemoveNeighbor(MapPolygon poly, GenWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.RemoveRef(poly, key.Data);
    }
    public void SetRegime(Regime r, GenWriteKey key)
    {
        GetMeta().UpdateEntityVar<EntityRef<Regime>>(nameof(Regime), this, key, new EntityRef<Regime>(r.Id));
    }
    public void AddNoNeighborBorder(Vector2 from, Vector2 to)
    {
        NoNeighborBorders.Add(from);
        NoNeighborBorders.Add(to);
    }
}
public static class MapPolygonExt
{
    
    public static List<Triangle> GetTrisRel(this MapPolygon poly, Data data)
    {
        return data.Cache.PolyRelTris[poly];
    }
    public static bool PointInPoly(this MapPolygon poly, Vector2 posAbs, Data data)
    {
        return data.Cache.PolyRelTris[poly].Any(t => t.PointInsideTriangle(poly.GetOffsetTo(posAbs, data)));
    }
    
    
    public static Vector2 GetOffsetTo(this MapPolygon poly, MapPolygon p, Data data)
    {
        var off1 = p.Center - poly.Center;
        var off2 = (off1 + Vector2.Right * data.Planet.Width);
        var off3 = (off1 + Vector2.Left * data.Planet.Width);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    public static Vector2 GetOffsetTo(this MapPolygon poly, Vector2 p, Data data)
    {
        var off1 = p - poly.Center;
        var off2 = (off1 + Vector2.Right * data.Planet.Width);
        var off3 = (off1 + Vector2.Left * data.Planet.Width);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }

    public static int GetNumPeeps(this MapPolygon poly, Data data)
    {
        return data.Society.Peeps.Homes.GetNumPeepsInPoly(poly);
    }

    public static float GetArea(this MapPolygon poly, Data data)
    {
        return poly.GetTrisRel(data).Sum(t => t.GetArea());
    }
}