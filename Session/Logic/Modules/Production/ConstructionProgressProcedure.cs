using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionProgressProcedure : Procedure
{
    public Dictionary<PolyTriPosition, float> ConstructionProgresses { get; private set; }
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var kvp in ConstructionProgresses)
        {
            var pos = kvp.Key;
            var regime = pos.Poly(key.Data).Regime.Entity();
            var con = regime.CurrentConstruction.Constructions[pos];
            con.ProgressConstruction(kvp.Value, key);
        }
    }
}
