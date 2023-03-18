
using System.Collections.Generic;
using Godot;

public class Iron : NaturalResource
{
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    protected override float _overflowSize { get; } = 100f;
    protected override float _minDepositSize { get; } = 10f;

    public Iron() 
        : base(nameof(Iron), Colors.Black.Lightened(.3f))
    {
    }
    protected override float GetDepositScore(MapPolygon p)
    {
        var score = 0f;
        score += p.Roughness * 75f;
        if (p.IsWater()) score *= .25f;
        return score;
    }

    protected override float GenerateDepositSize(MapPolygon p)
    {
        return 150f * Game.I.Random.RandfRange(.5f, 2f);
    }
}
