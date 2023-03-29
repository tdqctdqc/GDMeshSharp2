
using Godot;

public class TickProcedure : Procedure
{
    public override bool Valid(Data data)
    {
        return true;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var gc = key.Data.BaseDomain.GameClock;
        gc.Set<int>(nameof(GameClock.Tick), gc.Tick + 1, key);
        key.Data.Notices.Ticked.Invoke(gc.Tick);
    }
}
