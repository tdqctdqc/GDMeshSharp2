using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainTriRepo : Repository<TerrainTriHolder>
{
    public Dictionary<string, TerrainTriHolder> ByName { get; private set; }
    public TerrainTriHolder GetTris(TerrainAspect aspect) => ByName[aspect.Name];

    public TerrainTriRepo(Domain domain, Data data) : base(domain, data)
    {
        ByName = new Dictionary<string, TerrainTriHolder>();
        AddedEntity += (holder, key) =>
        {
            ByName.Add(holder.TerrainAspect.ModelName, holder);
        };
        RemovingEntity += (holder, key) =>
        {
            ByName.Remove(holder.TerrainAspect.ModelName);
        };
    }
}