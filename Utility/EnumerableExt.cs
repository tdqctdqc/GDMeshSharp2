using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumerableExt
{
    private static RandomNumberGenerator _rand = new RandomNumberGenerator();

    public static List<T> GetBetween<T>(this IReadOnlyList<T> list, T from, T to)
    {
        var start = list.IndexOf(from);
        if (start == -1) throw new Exception();
        var res = new List<T>();
        for (var i = 1; i < list.Count; i++)
        {
            var val = list.Modulo(start + i);
            if (val.Equals(from))
            {
                throw new Exception();
            }
            if (val.Equals(to))
            {
                break;
            }
            res.Add(val);
        }

        return res;
    }
    public static T GetRandomElement<T>(this IEnumerable<T> enumerable)
    {
        var index = _rand.RandiRange(0, enumerable.Count() - 1);
        return enumerable.ElementAt(index);
    }

    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
    {
        return new HashSet<T>(enumerable);
    }
    public static List<T> GetDistinctRandomElements<T>(this IEnumerable<T> enumerable, int n)
    {
        var indices = new HashSet<int>();
        var choices = new List<T>(enumerable);
        var count = choices.Count();
        while (indices.Count < n)
        {
            indices.Add(Game.I.Random.RandiRange(0, count - 1));
        }

        return indices.Select(i => choices[i]).ToList();
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


    public static void DoForGridNeighbors<T>(this T[][] array, Func<T, T, bool> action,
        bool skipCenter = true)
    {
        bool cont = true;
        var width = array[0].Length;
        var height = array.Length;

        for (int h = 0; h < height; h++)
        {
            for (int g = 0; g < width; g++)
            {
                act(g, h);
            }
        }
        
        void act(int g, int h)
        {
            var el = array[h][g];
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (g + i < 0 || h + j < 0 || g + i >= width || h + j >= height) continue;
                    if (skipCenter && i == 0 && j == 0) continue;
                    var el2 = array[h + j][g + i];
                    cont = action(el, el2);
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

    public static T FindNext<T>(this List<T> list, Func<T, bool> pred, int from)
    {
        for (var i = 1; i < list.Count; i++)
        {
            var t = list.Modulo(i + from);
            if (pred(t)) return t;
        }

        throw new Exception();
    }
    public static T Next<T>(this List<T> list, int i)
    {
        return list[(i + 1) % list.Count];
    }
    public static T Prev<T>(this List<T> list, int i)
    {
        return list[(i - 1 + list.Count) % list.Count];
    }

    public static T FromEnd<T>(this IReadOnlyList<T> list, int i)
    {
        return list[(list.Count * 2 - 1 - i) % list.Count];
    }

    public static T Modulo<T>(this IReadOnlyList<T> list, int i)
    {
        return list[i % list.Count];
    }

    public static void AddRange<T>(this ICollection<T> hash, IEnumerable<T> en)
    {
        foreach (var t in en)
        {
            hash.Add(t);
        }
    }

    public static int IndexOf<T>(this IReadOnlyList<T> list, T el)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].Equals(el)) return i;
        }

        return -1;
    }

    public static bool AddOrUpdateRange<TKey, TValue, TCol>(this Dictionary<TKey, TCol> dic,
        TKey key, params TValue[] vals) where TCol : ICollection<TValue>, new()
    {
        if (dic.ContainsKey(key))
        {
            dic[key].AddRange(vals);
            return true;
        }
        else
        {
            var col = new TCol();
            col.AddRange(vals);
            dic.Add(key, col);
            return false;
        }
    }
    public static bool AddOrUpdate<TKey, TValue, TCol>(this Dictionary<TKey, TCol> dic,
        TKey key, TValue val) where TCol : ICollection<TValue>, new()
    {
        if (dic.ContainsKey(key))
        {
            dic[key].Add(val);
            return true;
        }
        else
        {
            var col = new TCol();
            col.Add(val);
            dic.Add(key, col);
            return false;
        }
    }
    
    public static bool AddOrSum<TKey>(this Dictionary<TKey, float> dic,
        TKey key, float val, float min = float.MinValue, float max = float.MaxValue)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = Mathf.Clamp(val + dic[key], min, max);
            return true;
        }
        else
        {
            dic[key] = Mathf.Clamp(val, min, max);
            return false;
        }
    }

    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key, Func<TKey, TValue> generate)
    {
        if (dic.TryGetValue(key, out var val)) return val;
        dic.Add(key, generate(key));
        return dic[key];
    }
}
