using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteLogic : ILogic
{
    private ServerWriteKey _sKey;
    private ProcedureWriteKey _pKey;
    public RemoteLogic(Data data)
    {
        _sKey = new ServerWriteKey(data);
        _pKey = new ProcedureWriteKey(data);
    }

    public void Process(float delta)
    {
        
    }

    public void ProcessUpdate(Update u)
    {
        u.Enact(_sKey);
    }

    public void ProcessProcedure(Procedure p)
    {
        p.Enact(_pKey);
    }

    public void ProcessDecision(Decision d)
    {
        if (d.Decided) return;
        if (d.IsPlayerDecision(_sKey.Data) == false
            || d.Decider.Entity().GetPlayer(_sKey.Data).PlayerGuid != Game.I.PlayerGuid) return;
        _sKey.Data.Notices.NeedDecision?.Invoke(d);
    }
}