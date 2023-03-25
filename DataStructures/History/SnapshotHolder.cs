using System;
using System.Collections.Generic;
using System.Linq;

public class SnapshotHolder<TElement, TData>
{
    private int _latest = 0;

    private Dictionary<int, Snapshot<TElement, TData>> _snapshots;

    public static SnapshotHolder<TElement, TData> Construct()
    {
        return new SnapshotHolder<TElement, TData>(new Dictionary<int, Snapshot<TElement, TData>>());
    }
    public SnapshotHolder(Dictionary<int, Snapshot<TElement, TData>> snapshots)
    {
        _snapshots = snapshots;
    }

    public void Add(int tick, Snapshot<TElement, TData> snap)
    {
        if (_latest < tick) _latest = tick;
        _snapshots.Add(tick, snap);
    }
    public TData GetLatest(TElement el)
    {
        if (_snapshots[_latest].Entries.ContainsKey(el) == false) return default;
        return _snapshots[_latest].Entries[el];
    }
}
