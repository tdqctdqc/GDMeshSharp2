using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[Convertible] public class TerrainTriDic : EntityConvertibleVar<Dictionary<int, List<Vector2>>, Dictionary<int, List<Triangle>>>
{
    public TerrainTriDic(Dictionary<int, List<Triangle>> c, CreateWriteKey key) : base(c, key)
    {
    }

    public TerrainTriDic(Dictionary<int, List<Vector2>> b) : base(b)
    {
    }

    public override Dictionary<int, List<Triangle>> ConvertFromBase(Dictionary<int, List<Vector2>> b)
    {
        var res = new Dictionary<int, List<Triangle>>();
        foreach (var keyValuePair in b)
        {
            var polyId = keyValuePair.Key;
            var ps = keyValuePair.Value;
            var tris = new List<Triangle>();
            res.Add(polyId, tris);
            for (var i = 0; i < ps.Count; i += 3)
            {
                tris.Add(new Triangle(ps[i], ps[i + 1], ps[i + 2]));
            }
        }

        return res;
    }

    public override Dictionary<int, List<Vector2>> ConvertToBase(Dictionary<int, List<Triangle>> c)
    {
        var res = new Dictionary<int, List<Vector2>>();
        foreach (var keyValuePair in c)
        {
            var polyId = keyValuePair.Key;
            var tris = keyValuePair.Value;
            var points = new List<Vector2>();
            res.Add(polyId, points);
            for (var i = 0; i < tris.Count; i += 3)
            {
                points.Add(tris[i].A);
                points.Add(tris[i].B);
                points.Add(tris[i].C);
            }
        }

        return res;
    }
}