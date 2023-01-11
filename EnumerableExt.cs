using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExt
{
    private static RandomNumberGenerator _rand = new RandomNumberGenerator();
    public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
    {
        var index = _rand.RandiRange(0, enumerable.Count() - 1);
        return enumerable.ElementAt(index);
    }

    public static List<T> GetNRandomElements<T>(this IEnumerable<T> enumerable, int n)
    {
        var choices = new HashSet<T>(enumerable);
        var result = new List<T>();
        for (int i = 0; i < n; i++)
        {
            var sample = choices.GetRandomElement();
            choices.Remove(sample);
            result.Add(sample);
        }

        return result;
    }
}
