using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class CurrentConstructionManager
{
    public Dictionary<PolyTriPosition, Construction> Constructions { get; private set; }

    public static CurrentConstructionManager Construct()
    {
        return new CurrentConstructionManager(new Dictionary<PolyTriPosition, Construction>());
    }
    [SerializationConstructor] private CurrentConstructionManager(Dictionary<PolyTriPosition, Construction> constructions)
    {
        Constructions = constructions;
    }
}
