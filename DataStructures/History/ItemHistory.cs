using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemHistory : History<string, float>
{
    public static ItemHistory Construct()
    {
        return new ItemHistory(new Dictionary<int, Snapshot<string, float>>());
    }
    [SerializationConstructor] private ItemHistory(Dictionary<int, Snapshot<string, float>> snapshots) 
        : base(snapshots)
    {
    }
}
