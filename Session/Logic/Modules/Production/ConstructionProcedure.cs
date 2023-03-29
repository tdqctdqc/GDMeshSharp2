using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionProcedure : Procedure
{
    public EntityRef<Regime> Regime { get; private set; }
    public Dictionary<PolyTriPosition, Construction> Constructions { get; private set; }
    public override bool Valid(Data data)
    {
        throw new NotImplementedException();
    }

    public override void Enact(ProcedureWriteKey key)
    {
        throw new NotImplementedException();
    }
}
