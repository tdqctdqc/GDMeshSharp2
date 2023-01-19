using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GenPolygon : Polygon
{
    public GenCell Cell { get; private set; }
    public List<GenPolygon> GeoNeighbors { get; private set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public float SettlementSize { get; private set; }
    public RegimeGen RegimeGen { get; private set; }
    
    
    
    
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public GeoPolygonBorder GetGeoPolyBorder(Polygon neighbor) 
        => (GeoPolygonBorder)_borderDic[neighbor];
    public GenPolygon(int id, Vector2 center, float mapWidth) : base(id, center, mapWidth)
    {
        GeoNeighbors = new List<GenPolygon>();
    }

    
    public void SetCell(GenCell cell)
    {
        if(Cell != null) throw new Exception();
        Cell = cell;
    }

    public override void AddNeighbor(Polygon poly, PolygonBorder border)
    {
        base.AddNeighbor(poly, border);
        GeoNeighbors = Neighbors.Select(n => (GenPolygon)n).ToList();
    }

    public override void RemoveNeighbor(Polygon poly)
    {
        base.RemoveNeighbor(poly);
        GeoNeighbors.Remove((GenPolygon) poly);
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

    public void SetRegime(RegimeGen r)
    {
        RegimeGen = r;
    }
}
