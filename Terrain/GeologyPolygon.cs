using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class GeologyPolygon : Polygon
{
    public GeologyCell Cell { get; private set; }
    public List<GeologyPolygon> GeoNeighbors { get; private set; }
    public List<GeoPolygonBorder> GeoBorders => Neighbors.Select(n => (GeoPolygonBorder)GetPolyBorder(n)).ToList();
    public GeoPolygonBorder GetGeoPolyBorder(Polygon neighbor) 
        => (GeoPolygonBorder)_borderDic[neighbor];
    public bool IsLand => Altitude > .5f;
    public bool IsWater => IsLand == false;
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    

    public GeologyPolygon(int id, Vector2 center, float mapWidth) : base(id, center, mapWidth)
    {
        GeoNeighbors = new List<GeologyPolygon>();
    }

    public void SetCell(GeologyCell cell)
    {
        if(Cell != null) throw new Exception();
        Cell = cell;
    }

    public override void AddNeighbor(Polygon poly, PolygonBorder border)
    {
        base.AddNeighbor(poly, border);
        GeoNeighbors = Neighbors.Select(n => (GeologyPolygon)n).ToList();
    }

    public override void RemoveNeighbor(Polygon poly)
    {
        base.RemoveNeighbor(poly);
        GeoNeighbors.Remove((GeologyPolygon) poly);
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
}
