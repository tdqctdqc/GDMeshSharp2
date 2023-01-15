using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Godot;

public abstract class TerrainAspect
{
    public abstract Func<GeologyPolygon, HashSet<GeologyPolygon>, List<Triangle>> BuildTrisForPoly { get;
        protected set;
    }
    
    
    
}