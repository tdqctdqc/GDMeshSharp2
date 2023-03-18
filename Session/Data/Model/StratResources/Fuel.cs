
using System.Collections.Generic;
using Godot;

public class Fuel : NaturalResource
{
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    protected override float _overflowSize { get; } = 100f;
    protected override float _minDepositSize { get; } = 10f;

    public Fuel() 
        : base(nameof(Fuel), Colors.Black)
    {
    }


    protected override float GetDepositScore(MapPolygon p)
    {
        var score = 0f;
        score += 15f * (1f - p.Roughness);
        return score;
    }

    protected override float GenerateDepositSize(MapPolygon p)
    {
        return 100f * Game.I.Random.RandfRange(.5f, 2f);
    }

}
