using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteLogic : ILogic
{
    private ServerWriteKey _sKey;
    private ProcedureWriteKey _pKey;
    private bool _inited;
    private List<Update> _syncingUpdates;
    public RemoteLogic(Data data, GameSession session)
    {
        _sKey = new ServerWriteKey(data, session);
        _pKey = new ProcedureWriteKey(data, session);
        _inited = false;
        _syncingUpdates = new List<Update>();
    }

    public void Process(float delta)
    {
        
    }
    public void ProcessUpdate(Update u)
    {
        if(_inited)
        {
            u.Enact(_sKey);
            return;
        }
        if (u is FinishedStateSyncUpdate su)
        {
            GD.Print("Got finished state sync update");
            _inited = true;
            var creations = _syncingUpdates.SelectWhereOfType<Update, EntityCreationUpdate>();
            EntitiesCreationUpdate.Create(creations, _sKey).Enact(_sKey);
            su.Enact(_sKey);
            return;
        }
        _syncingUpdates.Add(u);
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