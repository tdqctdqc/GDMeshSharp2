using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolyTerrainSample
{
    public MapPolygon Poly { get; private set; }
    public float FertilityMod { get; private set; }
    public MapPolyTerrainSample(MapPolygon poly, Data data)
    {
        Poly = poly;
        var tris = poly.GetTrisRel(data);
        var polyTri = poly.GetTerrainTris(data);
        var polyTris = poly.GetTerrainTris(data).Tris;
        FertilityMod = polyTris.Count() > 0f 
            ? polyTris.Select(i => i.Landform.FertilityMod * i.Vegetation.FertilityMod).Average()
            : 0f;
    }
}