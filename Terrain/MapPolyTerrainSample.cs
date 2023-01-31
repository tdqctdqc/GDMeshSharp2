using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MapPolyTerrainSample
{
    public MapPolygon Poly { get; private set; }
    public Vegetation[] Plants { get; private set; }
    public Landform[] Landforms { get; private set; }
    public float FertilityMod { get; private set; }
    public MapPolyTerrainSample(MapPolygon poly, Data data)
    {
        Poly = poly;
        var tris = poly.GetTrisRel(data);
        Plants = new Vegetation[tris.Count];
        Landforms = new Landform[tris.Count];
        for (int i = 0; i < tris.Count; i++)
        {
            var tri = tris[i];
            Plants[i] = data.Models.Vegetation.GetAspectAtPoint(poly, tri.GetCentroid(), data);
            Landforms[i] = data.Models.Landforms.GetAspectAtPoint(poly, tri.GetCentroid(), data);
        }

        FertilityMod = Enumerable.Range(0, Plants.Length)
            .Select(i => Landforms[i].FertilityMod * Plants[i].FertilityMod).Average();
    }
}