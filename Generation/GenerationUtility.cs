using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class GenerationUtility
{
    public static List<TSeed>[] PickSeeds<TSeed>(IEnumerable<TSeed> available, int[] seedNums)
    {
        var taken = new HashSet<TSeed>();
        var result = new List<TSeed>[seedNums.Length];
        for (var i = 0; i < seedNums.Length; i++)
        {
            var seeds = available.Except(taken).GetNRandomElements(seedNums[i]);
            seeds.ForEach(s => taken.Add(s));
            result[i] = seeds;
        }
        return result;
    }
    public static HashSet<TPicked> PickInTurn<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, IEnumerable<TPicker> openPickersSource,
        Func<TPicker, IEnumerable<TPicked>> getAdjacent, Action<TPicker, TPicked> pick)
    {
        var notTaken = new HashSet<TPicked>(notTakenSource);
        var openPickers = new HashSet<TPicker>(openPickersSource);
        while (openPickers.Count > 0)
        {
            var cell = openPickers.GetRandomElement();
            var available = getAdjacent(cell).Intersect(notTaken);
            if (available.Count() == 0)
            {
                openPickers.Remove(cell);
                continue;
            }
            var take = available.GetRandomElement();
            notTaken.Remove(take);
            pick(cell, take);
        }

        return notTaken;
    }
    public static HashSet<TPicked> PickInTurnHeuristic<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, 
        IEnumerable<TPicker> openPickersSource,
        Func<TPicker, IEnumerable<TPicked>> getAdjacent, Action<TPicker, TPicked> pick,
        Func<TPicked, TPicker, float> heuristic)
    {
        var notTaken = new HashSet<TPicked>(notTakenSource);
        var openPickers = new HashSet<TPicker>(openPickersSource);
        while (openPickers.Count > 0)
        {
            var picker = openPickers.GetRandomElement();
            var available = getAdjacent(picker).Intersect(notTaken).OrderByDescending(p => heuristic(p, picker));
            if (available.Count() == 0)
            {
                openPickers.Remove(picker);
                continue;
            }
            var take = available.ElementAt(0);
            notTaken.Remove(take);
            pick(picker, take);
        }

        return notTaken;
    }

    public static void OrderByClockwise<T>(this List<T> elements, 
        Vector2 center, 
        Func<T, Vector2> elPos)
    {
        var first = elPos(elements.First()) - center;
        
        elements.Sort((i,j) => 
            (elPos(i) - center).GetClockwiseAngleTo(first)
            .CompareTo(
                (elPos(j) - center).GetClockwiseAngleTo(first)
            )
        );
    }

    

    public static IEnumerable<TSub> GetBorderElements<TSub>(IEnumerable<TSub> els, 
        Func<TSub, IEnumerable<TSub>> getNeighbors,
        Func<TSub, bool> checkForeign)
    {
        //todo move these functions
        return els.Where(e => getNeighbors(e).Any(n => checkForeign(n)));
    }

    public static List<TSub> GetOrderedBorder<TSub>(
        IEnumerable<TSub> borderSubs, Func<TSub, IEnumerable<TSub>> getSubNeighbors,
        Func<TSub, bool> checkForeign) where TSub : class
    {
        return GetOrderedBorderPairs(borderSubs, getSubNeighbors, checkForeign).Select(e => e.Native).ToList();
    }
    public static List<TSub> GetOrderedOuterBorder<TSub>(
        IEnumerable<TSub> borderSubs, Func<TSub, IEnumerable<TSub>> getSubNeighbors,
        Func<TSub, bool> checkForeign) where TSub : class
    {
        return GetOrderedBorderPairs(borderSubs, getSubNeighbors, checkForeign).Select(e => e.Foreign).ToList();
    }
    public static List<BorderEdge<TSub>> GetOrderedBorderPairs<TSub>( 
        IEnumerable<TSub> borderSubs, Func<TSub, IEnumerable<TSub>> getSubNeighbors, 
        Func<TSub, bool> checkForeign) where TSub : class
    {
        var nativeEdgeDic = new Dictionary<TSub, List<BorderEdge<TSub>>>();
        var foreignEdgeDic = new Dictionary<TSub, List<BorderEdge<TSub>>>();
        var edges = new HashSet<BorderEdge<TSub>>();
        foreach (var sub in borderSubs)
        {
            if (nativeEdgeDic.ContainsKey(sub))
            {
                continue;
            }

            var thisSubEdges = getSubNeighbors(sub).Where(checkForeign)
                .Select(n => new BorderEdge<TSub>(sub, n)).ToList();
            nativeEdgeDic.Add(sub, thisSubEdges);
            foreach (var e in thisSubEdges)
            {
                edges.Add(e);
                if(foreignEdgeDic.ContainsKey(e.Foreign) == false) foreignEdgeDic.Add(e.Foreign, new List<BorderEdge<TSub>>());
                foreignEdgeDic[e.Foreign].Add(e);
            }
        }
        //todo optimize by just making edges as you traverse? 
        bool adjacentSub(BorderEdge<TSub> e, TSub s)
        {
            var ns = getSubNeighbors(s);
            return ns.Contains(e.Foreign) && ns.Contains(e.Native);
        }
        bool adjacentEdge(BorderEdge<TSub> e1, BorderEdge<TSub> e2)
        {
            return adjacentSub(e1, e2.Native) || adjacentSub(e1, e2.Foreign);
        }

        IEnumerable<BorderEdge<TSub>> getAdjEdges(BorderEdge<TSub> edge)
        {
            return nativeEdgeDic[edge.Native].Union(foreignEdgeDic[edge.Foreign])
                .Where(e => e.Equals(edge) == false && adjacentEdge(e, edge))
                .Distinct();
        }
        var firstSub = borderSubs.First();
        var firstEdges = nativeEdgeDic[firstSub];
        var firstEdge = firstEdges[0];
        var firstEdgeNeighbors = getAdjEdges(firstEdge);
        
        
        var left = new List<BorderEdge<TSub>>();
        var right = new List<BorderEdge<TSub>>();
        var count = firstEdgeNeighbors.Count();
        var covered = new HashSet<BorderEdge<TSub>> {firstEdge};
        if (count > 0)
        {
            traverse(firstEdgeNeighbors.ElementAt(0), left);
        }
        if (count > 1)
        {
            traverse(firstEdgeNeighbors.ElementAt(1), right);
        }

        void traverse(BorderEdge<TSub> e, List<BorderEdge<TSub>> list)
        {
            list.Add(e);
            covered.Add(e);
            var adj = getAdjEdges(e).Where(a => covered.Contains(a) == false);
            if (adj.Count() > 1) throw new Exception();
            if (adj.Count() > 0)
            {
                traverse(adj.First(), list);
            }
        }

        var result = new List<BorderEdge<TSub>>();
        for (int i = left.Count - 1; i >= 0; i--)
        {
            result.Add(left[i]);
        }
        result.Add(firstEdge);
        for (var i = 0; i < right.Count; i++)
        {
            result.Add(right[i]);
        }
        return result;
    }
}

