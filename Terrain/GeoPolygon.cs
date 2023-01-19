using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeoPolygon : Polygon
{
    public GeoCell Cell { get; private set; }
    public List<GeoPolygon> GeoNeighbors { get; private set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public float SettlementSize { get; private set; }
    public Regime Regime { get; private set; }
    
    
    
    
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public GeoPolygonBorder GetGeoPolyBorder(Polygon neighbor) 
        => (GeoPolygonBorder)_borderDic[neighbor];
    public GeoPolygon(int id, Vector2 center, float mapWidth) : base(id, center, mapWidth)
    {
        GeoNeighbors = new List<GeoPolygon>();
    }

    
    public void SetCell(GeoCell cell)
    {
        if(Cell != null) throw new Exception();
        Cell = cell;
    }

    public override void AddNeighbor(Polygon poly, PolygonBorder border)
    {
        base.AddNeighbor(poly, border);
        GeoNeighbors = Neighbors.Select(n => (GeoPolygon)n).ToList();
    }

    public override void RemoveNeighbor(Polygon poly)
    {
        base.RemoveNeighbor(poly);
        GeoNeighbors.Remove((GeoPolygon) poly);
    }

    public void SetAltitude(float altitude)
    {
        Altitude = altitude;
    }

    public void SetRoughness(float roughness)
    {
        Roughness = roughness;
    }

    public void SetMoisture(float moisture)
    {
        Moisture = moisture;
    }
    
    public void SetSettlementSize(float size)
    {
        SettlementSize = size;
    }

    public void SetRegime(Regime r)
    {
        Regime = r;
    }
}
