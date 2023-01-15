using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ISuper<TSub>
{
    IReadOnlyCollection<ISuper<TSub>> Neighbors { get; }
    IReadOnlyCollection<TSub> Subs { get; }
    IReadOnlyCollection<TSub> GetSubNeighbors(TSub sub);
    ISuper<TSub> GetSubSuper(TSub sub);
}

public static class ISuperExt
{
    public static IEnumerable<TSub> GetBorderElements<TSub>(this ISuper<TSub> super)
    {
        return super.Subs.Where(s => super.GetSubNeighbors(s).Any(n => super.GetSubSuper(n).Equals(super) == false));
    }
    public static List<BorderEdge<TSub>> GetOrderedBorderPairs<TSub>(this ISuper<TSub> super) where TSub : class
    {
        return GenerationUtility.GetOrderedBorderPairs(GetBorderElements(super), 
            super.GetSubNeighbors, s => super.GetSubSuper(s).Equals(super) == false);
    }
    public static List<TSub> GetOrderedBorder<TSub>(this ISuper<TSub> super) where TSub : class
    {
        return GenerationUtility.GetOrderedBorder(GetBorderElements(super), 
            super.GetSubNeighbors, s => super.GetSubSuper(s).Equals(super) == false);
    }
    public static List<TSub> GetOrderedOuterBorder<TSub>(this ISuper<TSub> super) where TSub : class
    {
        return GenerationUtility.GetOrderedOuterBorder(GetBorderElements(super), 
            super.GetSubNeighbors, s => super.GetSubSuper(s).Equals(super) == false);
    }
}
