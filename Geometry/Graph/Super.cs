using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Super<TSuper, TSub> : ISuper<TSuper, TSub> where TSuper : class
{
    public HashSet<TSuper> Neighbors { get; private set; }
    public HashSet<TSub> Subs { get; set; }
    public HashSet<TSub> NeighboringSubs { get; private set; }
    protected abstract IReadOnlyCollection<TSub> GetSubNeighbors(TSub sub);
    protected abstract TSuper GetSubSuper(TSub sub);
    protected abstract void SetSubSuper(TSub sub, TSuper super);

    public Super()
    {
        Subs = new HashSet<TSub>();
        NeighboringSubs = new HashSet<TSub>();
    }
    public void AddSub(TSub sub)
    {
        Subs.Add(sub);
        if(GetSubSuper(sub) is Super<TSuper, TSub> sup) sup.RemoveSub(sub);
        SetSubSuper(sub, this as TSuper);

        NeighboringSubs.Remove(sub);
        var border = GetSubNeighbors(sub).Except(Subs);
        foreach (var cell in border)
        {
            NeighboringSubs.Add(cell);
        }
    }
    
    
    protected void RemoveSub(TSub sub)
    {
        Subs.Remove(sub);
        var ns = GetSubNeighbors(sub);
        foreach (var n in ns)
        {
            if (GetSubNeighbors(n).Any(nn => Subs.Contains(nn)) == false)
            {
                NeighboringSubs.Remove(n);
            }
        }
    }
    public void SetNeighbors()
    {
        Neighbors = NeighboringSubs.Select(t => GetSubSuper(t)).ToHashSet();
    }
    
    TSuper ISuper<TSuper, TSub>.GetSubSuper(TSub sub) => GetSubSuper(sub);
    IReadOnlyCollection<TSub> ISuper<TSuper, TSub>.GetSubNeighbors(TSub sub) => GetSubNeighbors(sub);
    IReadOnlyCollection<TSuper> ISuper<TSuper, TSub>.Neighbors => Neighbors;
    IReadOnlyCollection<TSub> ISuper<TSuper, TSub>.Subs => Subs;
}