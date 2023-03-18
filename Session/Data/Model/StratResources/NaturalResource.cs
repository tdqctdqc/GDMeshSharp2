
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class NaturalResource : Item
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
            var chance = DepositChanceFunction.Calc(score);
            if (Game.I.Random.Randf() > chance) continue;
            var size = GenerateDepositSize(p) + (deps.ContainsKey(p) ? deps[p] : 0f);
            if (size < _minDepositSize) continue;
            deps.AddOrSum(p, size);
            if (deps[p] > _overflowSize) deps[p] = _overflowSize;
            var rem = size - _overflowSize;
            if (rem <= 0) continue;

            if(_overflow == OverFlowType.Multiple) OverflowMult(p, deps, scores, rem);
            else if(_overflow == OverFlowType.Single) OverflowSingle(p, deps, rem);
        }

        return deps;
    }

    private void OverflowMult(MapPolygon p, Dictionary<MapPolygon, float> deps, 
        Dictionary<MapPolygon, float> scores, float rem)
    {
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
    private void OverflowSingle(MapPolygon p, Dictionary<MapPolygon, float> deps, float rem)
    {
        var overflowPoly = p.Neighbors.Refs()
            .OrderBy(GetDepositScore)
            .Where(n => deps.ContainsKey(n) == false)
            .FirstOrDefault();
        if (overflowPoly != null)
        {
            deps.AddOrSum(overflowPoly, rem);
        }
    }
    protected abstract IFunction<float, float> DepositChanceFunction { get; }
    protected abstract float GetDepositScore(MapPolygon p);
    protected abstract float GenerateDepositSize(MapPolygon p);
    protected abstract float _overflowSize { get; }
    protected abstract float _minDepositSize { get; }
    protected abstract OverFlowType _overflow { get; }

    protected enum OverFlowType
    {
        None, Single, Multiple
    }
}
