using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MapPolygon : Entity
{
    public Vector2 Center { get; protected set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public List<Vector2> NoNeighborBorders { get; protected set; }
    public Color Color { get; protected set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public float SettlementSize { get; private set; }
    public int PlateId { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public MapPolygonBorder GetBorder(MapPolygon neighbor, Data data) 
        => data.Planet.PolyBorders.GetBorder(this, neighbor);
    
    public MapPolygon(int id, Vector2 center, float mapWidth, GenWriteKey key) : base(id, key)
    {
        Center = center;
        if (Center.x > mapWidth) Center = new Vector2(Center.x - mapWidth, center.y);
        if (Center.x < 0f) Center = new Vector2(Center.x + mapWidth, center.y);
        Neighbors = EntityRefCollection<MapPolygon>.Construct(new int[0], key);
        NoNeighborBorders = new List<Vector2>();
        Color = ColorsExt.GetRandomColor();
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
        for (int i = 0; i < Neighbors.Count; i++)
        {
            if (startN.Neighbors.Any(n => Neighbors.Contains(n)))
            {
                startN = Neighbors.Refs().ElementAt((i + 1) % Neighbors.Count);
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

    public void SetPlateId(int plateId, GenWriteKey key)
    {
        PlateId = plateId;
    }
    public void RemoveNeighbor(MapPolygon poly, GenWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.RemoveRef(poly, key.Data);
    }

    public void SetRegime(Regime r, GenWriteKey key)
    {
        Regime = EntityRef<Regime>.Construct(r, key);
    }
    public void AddNoNeighborBorder(Vector2 from, Vector2 to)
    {
        NoNeighborBorders.Add(from);
        NoNeighborBorders.Add(to);
    }
    private static MapPolygon DeserializeConstructor(string json)
    {
        return new MapPolygon(json);
    }
    private MapPolygon(string json) : base(json) { }
}
public static class MapPolygonExt
{
    public static List<Vector2> GetTrisAbs(this MapPolygon p, Data data)
    {
        var tris = new List<Vector2>();
        for (var i = 0; i < p.Neighbors.Count; i++)
        {
            var edge = p.GetBorder(p.Neighbors.Refs().ElementAt(i), data);
            var segs = edge.GetSegsRel(p);
            for (var j = 0; j < segs.Count; j++)
            {
                tris.Add(p.Center);
                tris.Add(segs[j].From + p.Center);
                tris.Add(segs[j].To + p.Center);
            }
        }

        return tris;
    }
    public static List<Vector2> GetTrisRel(this MapPolygon p, Data data)
    {
        var tris = new List<Vector2>();
        for (var i = 0; i < p.Neighbors.Count; i++)
        {
            var edge = p.GetBorder(p.Neighbors.Refs().ElementAt(i), data);
            var segs = edge.GetSegsRel(p);
            for (var j = 0; j < segs.Count; j++)
            {
                tris.Add(Vector2.Zero);
                tris.Add(segs[j].From);
                tris.Add(segs[j].To);
            }
        }

        return tris;
    }

    public static Vector2 GetOffsetTo(this MapPolygon poly, MapPolygon p, float mapWidth)
    {
        var off1 = p.Center - poly.Center;
        var off2 = (off1 + Vector2.Right * mapWidth);
        var off3 = (off1 + Vector2.Left * mapWidth);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    
    public static Vector2 GetOffsetTo(this MapPolygon poly, Vector2 p, float mapWidth)
    {
        var off1 = p - poly.Center;
        var off2 = (off1 + Vector2.Right * mapWidth);
        var off3 = (off1 + Vector2.Left * mapWidth);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    
}