using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class TerrainAspectManager<TAspect> : IModelManager<TAspect>
    where TAspect : TerrainAspect
{
    public Dictionary<string, TAspect> ByName { get; private set; }
    public List<TAspect> ByPriority { get; private set; }
    public static TAspect LandDefault { get; protected set; } 
    public static TAspect WaterDefault { get; protected set; }
    Dictionary<string, TAspect> IModelManager<TAspect>.Models => ByName;
    public TerrainAspectManager(TAspect waterDefault, 
        TAspect landDefault, List<TAspect> byPriority)
    {
        if (byPriority.Count + 2 > byte.MaxValue - 1) throw new Exception();
        WaterDefault = waterDefault;
        LandDefault = landDefault;
        ByPriority = byPriority;
        ByName = new Dictionary<string, TAspect>();
        // ByName.Add(waterDefault.Name, waterDefault);
        // if(landDefault != waterDefault) ByName.Add(landDefault.Name, landDefault);
        ByPriority.ForEach(ta => ByName.Add(ta.Name, ta));
    }

    

}