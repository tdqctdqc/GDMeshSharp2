
using System;
using System.Collections.Generic;
using System.Linq;

public class Picker
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
}
