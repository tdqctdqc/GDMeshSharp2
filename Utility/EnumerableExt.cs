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

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
    {
        return new HashSet<T>(enumerable);
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

    public static float Product(this IEnumerable<float> floats)
    {
        var res = 1f;
        foreach (var f in floats)
        {
            res *= f;
        }

        return res;
    }

    public static HashSet<T> GetSingles<T>(this IEnumerable<T> ts)
    {
        var singles = new HashSet<T>();
        foreach (var t in ts)
        {
            _ = singles.Contains(t) ? singles.Remove(t) : singles.Add(t);
        }
        return singles;
    }

    public static void DoForGridAround(this Func<int, int, bool> action, int x, int y, 
        bool skipCenter = true)
    {
        bool cont = true;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (skipCenter && i == 0 && j == 0) continue;
                cont = action(i, j);
                if (cont == false)
                {
                    break;
                }
            }
            if (cont == false)
            {
                break;
            }
        }
    }

    public static Dictionary<TKey, TValue> BindDictionary<TKey, TValue>(this List<TKey> keys,
        List<TValue> values)
    {
        if (keys.Count != values.Count) throw new Exception("different number of keys and values");
        var res = new Dictionary<TKey, TValue>();
        for (var i = 0; i < keys.Count; i++)
        {
            res.Add(keys[i], values[i]);
        }
        return res;
    }

    public static T Next<T>(this List<T> list, int i)
    {
        return list[(i + 1) % list.Count];
    }
    public static T Prev<T>(this List<T> list, int i)
    {
        return list[(i - 1 + list.Count) % list.Count];
    }
}
