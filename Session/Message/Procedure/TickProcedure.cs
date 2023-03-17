
using Godot;

public class TickProcedure : Procedure
{
    public override bool Valid(Data data)
    {
        return true;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var gc = key.Data.BaseDomain.GameClock.Value;
        gc.Set(nameof(GameClock.Tick), gc.Tick + 1, key);
    }
}
