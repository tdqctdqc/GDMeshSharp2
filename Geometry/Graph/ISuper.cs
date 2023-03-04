using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ISuper<TSuper, TSub>
{
    IReadOnlyCollection<TSuper> Neighbors { get; }
    IReadOnlyCollection<TSub> Subs { get; }
    IReadOnlyCollection<TSub> GetSubNeighbors(TSub sub);
    TSuper GetSubSuper(TSub sub);
}

public static class ISuperExt
{
    public static IEnumerable<TSub> GetBorderElements<TSuper, TSub>(this ISuper<TSuper, TSub> super)
    {
        return super.Subs.Where(s => super.GetSubNeighbors(s).Any(n => super.GetSubSuper(n).Equals(super) == false));
    }
}
