using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class History<TElement, TData>
{
    public Dictionary<int, Snapshot<TElement, TData>> Snapshots { get; private set; }
    [SerializationConstructor] protected History(Dictionary<int, Snapshot<TElement, TData>> snapshots)
    {
        Snapshots = snapshots;
    }
    public void AddSnapshot(int tick, Snapshot<TElement, TData> snap, ProcedureWriteKey key)
    {
        Snapshots.Add(tick, snap);
    }
}