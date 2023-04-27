using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RuleVars : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(BaseDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public int FoodConsumptionPerPeepPoint { get; protected set; }
    public float MinSurplusRatioToGetGrowth { get; private set; }
    public float MaxEffectiveSurplusRatio { get; private set; }
    public float GrowthRateCeiling { get; private set; }
    public float GathererFoodCeiling { get; private set; }
    public float GathererFoodFloor { get; private set; }
    public float MinDeficitRatioToGetDecline { get; private set; }
    public float MaxEffectiveDeficitRatio { get; private set; }
    public float DeclineRateCeiling { get; private set; }
    public static RuleVars CreateDefault(GenWriteKey key)
    {
        var v = new RuleVars(
            1,
            .01f,
            1f,
            .02f,
            .1f,
            1f,
            .2f,
            500f,
            100f,
            key.IdDispenser.GetID());
        key.Create(v);
        return v;
    }
    [SerializationConstructor] private RuleVars(
        int foodConsumptionPerPeepPoint, 
        float minSurplusRatioToGetGrowth,
        float maxEffectiveSurplusRatio,
        float growthRateCeiling,
        float minDeficitRatioToGetDecline,
        float maxEffectiveDeficitRatio,
        float declineRateCeiling,
        float gathererFoodCeiling,
        float gathererFoodFloor,
        int id) : base(id)
    {
        FoodConsumptionPerPeepPoint = foodConsumptionPerPeepPoint;
        MinSurplusRatioToGetGrowth = minSurplusRatioToGetGrowth;
        MaxEffectiveSurplusRatio = maxEffectiveSurplusRatio;
        GrowthRateCeiling = growthRateCeiling;
        MinDeficitRatioToGetDecline = minDeficitRatioToGetDecline;
        MaxEffectiveDeficitRatio = maxEffectiveDeficitRatio;
        DeclineRateCeiling = declineRateCeiling;
        GathererFoodCeiling = gathererFoodCeiling;
        GathererFoodFloor = gathererFoodFloor;
    }
    
}
