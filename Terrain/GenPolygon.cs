using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class GenPolygon : Polygon
{
    public EntityRefCollection<GenPolygon> GeoNeighbors { get; private set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public float SettlementSize { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public GeoPolygonBorder GetGeoPolyBorder(Polygon neighbor) 
        => (GeoPolygonBorder)_borderDic[neighbor];
    public GenPolygon(int id, Vector2 center, float mapWidth, GenWriteKey key) : base(id, center, mapWidth, key)
    {
        GeoNeighbors = new EntityRefCollection<GenPolygon>(new int[0], key.Data);
    }


    public override void AddNeighbor(Polygon poly, PolygonBorder border, GenWriteKey key)
    {
        base.AddNeighbor(poly, border, key);
        GeoNeighbors.Set(Neighbors.Select(n => n.Id), key.Data);
    }

    public override void RemoveNeighbor(Polygon poly, GenWriteKey key)
    {
        base.RemoveNeighbor(poly, key);
        GeoNeighbors.Remove(poly.Id, key.Data);
    }
    
    private static GenPolygon DeserializeConstructor(string json)
    {
        return new GenPolygon(json);
    }
    private GenPolygon(string json) : base(json) { }
}
