using System;
using System.Collections.Generic;
using System.Linq;

public class Construction
{
    public ModelRef<BuildingModel> Model { get; private set; }
    public PolyTriPosition Pos { get; private set; }
    public float Progress { get; private set; }

    public Construction(ModelRef<BuildingModel> model, PolyTriPosition pos, float progress)
    {
        Model = model;
        Pos = pos;
        Progress = progress;
    }

    public bool ProgressConstruction(float progress, ProcedureWriteKey key)
    {
        Progress += progress;
        return Progress >= Model.Model().BuildCost;
    }
}
