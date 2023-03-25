
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Iron : NaturalResource
{
    protected override float _overflowSize { get; } = 100f;
    protected override float _minDepositSize { get; } = 10f;
    protected override OverFlowType _overflow { get; } = OverFlowType.None;
    
    
    public Iron() 
        : base(nameof(Iron), Colors.Black.Lightened(.3f),
            new MineableAttribute())
    {
    }
    protected override IFunction<float, float> DepositChanceFunction { get; }  = new ArctanFunction(100f);
    protected override float GetDepositScore(MapPolygon p)
    {
        var score = 15f;
        score += p.Roughness * 50f;
        if (p.IsWater()) score *= .1f;
        if(p.IsLand && p.Moisture >= VegetationManager.Swamp.MinMoisture * .75f) score += 20f;
        return score;
    }

    protected override float GenerateDepositSize(MapPolygon p)
    {
        return 150f * Game.I.Random.RandfRange(.5f, 2f);
    }
}
