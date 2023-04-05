using System;
using System.Collections.Generic;
using System.Linq;

public class CapacityConstructionQueue
{
    public float CapacityInConstruction { get; private set; }
    public HashSet<Construction> Constructions { get; private set; }

    public CapacityConstructionQueue()
    {
        Constructions = new HashSet<Construction>();
    }
    public void AddConstruction(Construction c, Data data)
    {
        CapacityInConstruction += GetCapacity(c, data);
    }

    private float GetCapacity(Construction c, Data data)
    {
        var b = c.Model.Model();
        return c.Model.Model().Capacity * b.GetTriEfficiencyScore(c.Pos, data);
    }
    public void CheckForFinished(Data data)
    {
        var toRemove = new HashSet<Construction>();
        foreach (var c in Constructions)
        {
            if (c.TicksLeft <= 0)
            {
                toRemove.Add(c);
                CapacityInConstruction -= GetCapacity(c, data);
            }
        }
        foreach (var c in toRemove)
        {
            Constructions.Remove(c);
        }
    }
}
