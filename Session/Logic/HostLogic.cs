using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class HostLogic : ILogic
{
    public ConcurrentQueue<Command> CommandQueue { get; }
    public EntityValueCache<Regime, RegimeAi> AIs { get; }
    private HostServer _server;
    private HostWriteKey _hKey;
    private ProcedureWriteKey _pKey;
    private Data _data;
    private readonly LogicFrame[] _frames;
    private int _tick = 0;
    private int _frameIter = 0;
    private float _framePeriod = 1f;
    private float _frameTimer = 1f;
    private Task _calculating;
    
    public HostLogic(Data data)
    {
        AIs = EntityValueCache<Regime, RegimeAi>
            .ConstructConstant(data, r => new RegimeAi(r, data));
        CommandQueue = new ConcurrentQueue<Command>();
        _frames = new LogicFrame[]
        {
            new LogicFrame(new WorkProdConsumeModule()),
            new LogicFrame(new ConstructBuildingsModule()),
            new LogicFrame(new PeepGrowthModule()),
            new LogicFrame(),
            new LogicFrame(new AiProdConstructModule(AIs, data)),
        };
    }

    public void Process(float delta)
    {
        _frameTimer += delta;
        if (_calculating != null && _calculating.IsCompleted)
        {
            if( _calculating.Exception != null)
            {
                throw _calculating.Exception;
            }
            _calculating = null;
        }
        
        if (_frameTimer >= _framePeriod 
            && _calculating == null)
        {
            _frameTimer = 0f;
            _calculating = Task.Run(() => {
                DoCommands();
                DoFrame();
                _calculating = null;
            });
        }
        else if (_calculating == null)
        {
            _calculating = Task.Run(() =>
            {
                DoCommands();
                _calculating = null;
            });
        }
    }
    public void SetDependencies(HostServer server, GameSession session, Data data)
    {
        _data = data;
        _server = server;
        _hKey = new HostWriteKey(server, this, data, session);
        _pKey = new ProcedureWriteKey(data, session);
    }

    private void DoFrame()
    {
        //todo ticking for remote as well?
        new TickProcedure().Enact(_pKey);
        var logicResult = _frames[_frameIter].Calculate(_data);
        _frameIter = (_frameIter + 1) % _frames.Length;
        
        
        for (var i = 0; i < logicResult.Procedures.Count; i++)
        {
            logicResult.Procedures[i].Enact(_pKey);
        }

        for (int i = 0; i < logicResult.CreateEntities.Count; i++)
        {
            logicResult.CreateEntities[i].Invoke(_hKey);
        }
        
        for (var i = 0; i < logicResult.Decisions.Count; i++)
        {
            var d = logicResult.Decisions[i];
            if(d.IsPlayerDecision(_data) == false)
            {
                logicResult.Decisions[i].AIDecide(_hKey);
            }
            else 
            {
                var p = d.Decider.Entity().GetPlayer(_pKey.Data);
                if (p.PlayerGuid == Game.I.PlayerGuid)
                {
                    _pKey.Data.Notices.NeedDecision?.Invoke(d);
                }
            }
        }
        
        _server.ReceiveLogicResult(logicResult, _hKey);
        _server.PushPackets(_hKey);
    }

    private void DoCommands()
    {
        var queuedProcs = new List<Procedure>();
        var logicResult = new LogicResults(new List<Message>(), new List<Func<HostWriteKey, Entity>>());
        var commandCount = CommandQueue.Count;
        for (var i = 0; i < commandCount; i++)
        {
            if (CommandQueue.TryDequeue(out var command))
            {
                if(command.Valid(_data)) command.Enact(_hKey, logicResult.Procedures.Add);
            }
        }
        for (var i = 0; i < logicResult.Procedures.Count; i++)
        {
            logicResult.Procedures[i].Enact(_pKey);
        }
        _server.ReceiveLogicResult(logicResult, _hKey);
        _server.PushPackets(_hKey);
    }
}
