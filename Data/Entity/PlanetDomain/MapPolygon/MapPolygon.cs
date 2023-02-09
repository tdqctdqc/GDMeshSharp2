using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public partial class MapPolygon : Entity
{
    public override Type GetDomainType() => typeof(PlanetDomain);
    public Vector2 Center { get; protected set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public Color Color { get; protected set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public float SettlementSize { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public TriListHolder TerrainTris { get; private set; }
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public MapPolygonBorder GetBorder(MapPolygon neighbor, Data data) 
        => data.Planet.PolyBorders.GetBorder(this, neighbor);
    
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, EntityRefCollection<MapPolygon> neighbors, 
        Color color, float altitude, float roughness, 
        float moisture, float settlementSize, EntityRef<Regime> regime,
        TriListHolder terrainTris) : base(id)
    {
        TerrainTris = terrainTris;
        Center = center;
        Neighbors = neighbors;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        SettlementSize = settlementSize;
        Regime = regime;
    }

    public static MapPolygon Create(int id, Vector2 center, float mapWidth, GenWriteKey key)
    {
        var mapCenter = center;
        if (mapCenter.x > mapWidth) mapCenter = new Vector2(mapCenter.x - mapWidth, center.y);
        if (mapCenter.x < 0f) mapCenter = new Vector2(mapCenter.x + mapWidth, center.y);
        var p = new MapPolygon(id, mapCenter,
            new EntityRefCollection<MapPolygon>(new HashSet<int>()),
            ColorsExt.GetRandomColor(),
            0f,
            0f,
            0f,
            0f,
            new EntityRef<Regime>(-1),
            TriListHolder.Construct()
        );
        key.Create(p);
        return p;
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
    }
    public void RemoveNeighbor(MapPolygon poly, GenWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.RemoveRef(poly, key.Data);
    }
    public void SetRegime(Regime r, CreateWriteKey key)
    {
        GetMeta().UpdateEntityVar<EntityRef<Regime>>(nameof(Regime), this, key, new EntityRef<Regime>(r.Id));
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
    
    public static Landform GetLandformAtPoint(this MapPolygon poly, Data data, Vector2 offset)
    {
        return poly.TerrainTris.GetLandformAtPoint(poly, data, offset);
    }
    public static Vegetation GetVegetationAtPoint(this MapPolygon poly, Data data, Vector2 offset)
    {
        return poly.TerrainTris.GetVegetationAtPoint(poly, data, offset);
    }
    public static void BuildTrisForAspects<TAspect>
        (this MapPolygon poly, TerrainAspectManager<TAspect> man, GenWriteKey key) where TAspect : TerrainAspect
    {
        for (var i = 0; i < man.ByPriority.Count; i++)
        {
            var ta = man.ByPriority[i];
            if (ta.Allowed(poly, key.GenData) == false) continue;
            poly.TerrainTris.Add(ta, ta.TriBuilder.BuildTrisForPoly(poly, key.GenData));
        }
    }
    public static void BuildTrisForAspect(this MapPolygon poly, TerrainAspect ta, GenWriteKey key)
    {
        if (ta.Allowed(poly, key.GenData) == false) return;
        poly.TerrainTris.Add(ta, ta.TriBuilder.BuildTrisForPoly(poly, key.GenData));
    }
}