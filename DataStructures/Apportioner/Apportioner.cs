
using System;
using System.Collections.Generic;
using System.Linq;

public static class Apportioner
{
    public static List<float> ApportionLinear<T>(float toApportion, IEnumerable<T> cands, Func<T, float> getScore)
    {
        var res = cands.Select(getScore).ToList();
        var totalScore = res.Sum();
        for (var i = 0; i < res.Count; i++)
        {
            res[i] = toApportion * res[i] / totalScore;
        }

        return res;
    }
}
