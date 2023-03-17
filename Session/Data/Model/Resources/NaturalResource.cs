
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class NaturalResource : Resource
{
    protected NaturalResource(string name, Color color) : base(name, color)
    {
    }
    public Dictionary<MapPolygon, float> GenerateDeposits(Data data)
    {
        var polys = data.Planet.Polygons.Entities;
        var deps = new Dictionary<MapPolygon, float>();
        var scores = new Dictionary<MapPolygon, float>();
        foreach (var p in polys)
        {
            var score = scores.GetOrAdd(p, GetDepositScore);
            var chance = _depositChanceMetric.GetMetric(score);
            if (Game.I.Random.Randf() > chance) continue;
            var size = GenerateDepositSize(p) + (deps.ContainsKey(p) ? deps[p] : 0f);
            if (size < _minDepositSize) continue;
            deps.AddOrSum(p, size);
            if (deps[p] > _overflowSize) deps[p] = _overflowSize;
            var rem = size - _overflowSize;
            if (rem <= 0) continue;

            var neighbors = p.Neighbors.Refs();
            var portions = Apportioner.ApportionLinear(rem, neighbors, n => scores.GetOrAdd(n, GetDepositScore));
            for (var i = 0; i < portions.Count; i++)
            {
                var n = neighbors.ElementAt(i);
                deps.AddOrSum(n, portions[i]);
                deps[n] = Mathf.Min(_overflowSize, deps[n]);
                if (deps[n] < _minDepositSize) deps.Remove(n);
            }
        }

        return deps;
    }
    protected abstract IMetric<float> _depositChanceMetric { get; }
    protected abstract float GetDepositScore(MapPolygon p);
    protected abstract float GenerateDepositSize(MapPolygon p);
    protected abstract float _overflowSize { get; }
    protected abstract float _minDepositSize { get; }
}
