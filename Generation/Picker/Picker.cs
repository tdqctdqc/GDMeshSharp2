
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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
        Func<TPicker, HashSet<TPicked>> getAdjacent, Action<TPicker, TPicked> pick)
    {
        var notTaken = new HashSet<TPicked>(notTakenSource);
        var openPickers = new LinkedList<TPicker>(openPickersSource);
        while (openPickers.Count > 0)
        {
            var cell = openPickers.First;
            openPickers.RemoveFirst();

            TPicked take = default;
            bool found = false;
            var adj = getAdjacent(cell.Value);
            for (var i = 0; i < adj.Count; i++)
            {
                var el = adj.ElementAt(i);
                if (notTaken.Contains(el))
                {
                    take = el;
                    notTaken.Remove(take);
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                continue;
            }

            openPickers.AddLast(cell);
            pick(cell.Value, take);
        }

        return notTaken;
    }
    public static HashSet<TPicked> PickInTurnHeuristic<TPicker, TPicked>(IEnumerable<TPicked> notTakenSource, 
        IEnumerable<TPicker> openPickersSource,
        Func<TPicker, IEnumerable<TPicked>> getAdjacent, Action<TPicker, TPicked> pick,
        Func<TPicked, TPicker, float> heuristic)
    {
        var notTaken = new HashSet<TPicked>(notTakenSource);
        var openPickers = new LinkedList<TPicker>(openPickersSource);
        while (openPickers.Count > 0)
        {
            var picker = openPickers.First;
            openPickers.RemoveFirst();
            TPicked take = default;
            bool found = false;
            var adj = getAdjacent(picker.Value);
            float takeHeur = Mathf.Inf;
            foreach (var p in adj)
            {
                if (notTaken.Contains(p) == false) continue;
                var heur = heuristic(p, picker.Value);
                if (heur < takeHeur)
                {
                    found = true;
                    takeHeur = heur;
                    take = p;
                }
            }
            
            if (found == false)
            {
                continue;
            }
            openPickers.AddLast(picker);
            notTaken.Remove(take);
            pick(picker.Value, take);
        }

        return notTaken;
    }
}
