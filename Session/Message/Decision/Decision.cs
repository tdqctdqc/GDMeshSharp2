
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class Decision : Message
{
    public EntityRef<Regime> Decider { get; private set; }
    public bool Decided { get; private set; }
    protected Decision(EntityRef<Regime> decider, bool decided)
    {
        Decider = decider;
        Decided = decided;
    }
    
    public abstract bool Valid(Data data);
    public abstract string GetDescription();
    public void AIDecide(HostWriteKey key)
    {
        Decided = true;
        if (Valid(key.Data) == false) return;
        var ai = key.Logic.AIs[Decider.Entity()];
        GetOptions()
            .OrderByDescending(o => o.GetAiScore(ai))
            .First()
            .Enact(key);
    }

    public bool IsPlayerDecision(Data data) => Decider.Entity().IsPlayerRegime(data);
    public void PlayerEnact(string option, HostWriteKey key)
    {
        Decided = true;
        if (Valid(key.Data) == false) return;
        GetOptions().First(o => o.Name == option).Enact(key);
    }
    public abstract List<DecisionOption> GetOptions();
    public override void HandleHost(HostLogic logic)
    {
        return;
    }
    public override void HandleRemote(RemoteLogic logic)
    {
        logic.ProcessDecision(this);
    }
    protected override byte GetSubMarker()
    {
        return _typeManagers[typeof(Decision)].GetMarkerFromMessageType(GetType());
    }
    protected override byte GetMarker()
    {
        return _markers[typeof(Decision)];
    }
}
