using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class TriListHolder
{
    public Dictionary<string, List<Triangle>> Landforms { get; private set; }
    public Dictionary<string, List<Triangle>> Vegetations { get; private set; }
    public List<Triangle> this[TerrainAspect ta] => GetTris(ta);

    public static TriListHolder Construct()
    {
        return new TriListHolder(new Dictionary<string, List<Triangle>>(),
            new Dictionary<string, List<Triangle>>());
    }
    [SerializationConstructor] public TriListHolder(Dictionary<string, List<Triangle>> landforms, 
        Dictionary<string, List<Triangle>> vegetations)
    {
        Landforms = landforms;
        Vegetations = vegetations;
    }
    public void Add(TerrainAspect ta, List<Triangle> tris)
    {
        if (ta is Landform lf)
        {
            Landforms[lf.Name] = tris;
        } 
        else if (ta is Vegetation v)
        {
            Vegetations[v.Name] = tris;
        }
    }

    private List<Triangle> GetTris(TerrainAspect ta)
    {
        if (ta is Landform lf)
        {
            if (Landforms.TryGetValue(ta.Name, out var val))
            {
                return val;
            }

            return null;
        }
        if(ta is Vegetation v)
        {
            if (Vegetations.TryGetValue(ta.Name, out var val))
            {
                return val;
            }
            return null;
        }
        throw new Exception();
    }
    public Landform GetLandformAtPoint(MapPolygon poly, Data data, Vector2 offset)
    {
        foreach (var keyValuePair in Landforms)
        {
            var ts = keyValuePair.Value;
            if (ts.Any(t => t.ContainsPoint(offset))) return (Landform)data.Models[keyValuePair.Key];
        }

        return poly.IsLand ? LandformManager.LandDefault : LandformManager.WaterDefault;
    }
    public Vegetation GetVegetationAtPoint(MapPolygon poly, Data data, Vector2 offset)
    {
        foreach (var keyValuePair in Vegetations)
        {
            var ts = keyValuePair.Value;
            if (ts.Any(t => t.ContainsPoint(offset))) return (Vegetation)data.Models[keyValuePair.Key];
        }

        return poly.IsLand ? VegetationManager.LandDefault : VegetationManager.WaterDefault;
    }
    
}