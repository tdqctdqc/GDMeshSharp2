using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class CreateSettlementBuildingProcedure : Procedure
{
    public int SettlementId { get; private set; }
    public ModelRef<BuildingModel> BuildingModel { get; private set; }

    public CreateSettlementBuildingProcedure(int settlementId, ModelRef<BuildingModel> buildingModel)
    {
        SettlementId = settlementId;
        BuildingModel = buildingModel;
    }

    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var s = key.Data.Society.Settlements[SettlementId];
        s.AddBuilding(BuildingModel.Model(), key);
    }
}
