using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemHistory : History<string, int>
{
    public static ItemHistory Construct()
    {
        return new ItemHistory(SnapshotHolder<string, int>.Construct());
    }
    [SerializationConstructor] private ItemHistory(SnapshotHolder<string, int> snapshots) 
        : base(snapshots)
    {
    }
}
