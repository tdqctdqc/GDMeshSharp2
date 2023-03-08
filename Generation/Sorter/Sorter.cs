using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Sorter
{
    
}

public static class SorterExt
{
    public static Dictionary<TKey, List<TValue>> Sort<TKey, TValue>(this IEnumerable<TValue> vals,
        Func<TValue, TKey> getKey)
    {
        var dic = new Dictionary<TKey, List<TValue>>();
        foreach (var val in vals)
        {
            var key = getKey(val);
            dic.AddOrUpdate(key, val);
        }
        return dic;
    }
    

    
    
}
