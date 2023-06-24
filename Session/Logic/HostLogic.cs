using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

public class HostLogic : ILogic
{
    public ConcurrentQueue<Command> CommandQueue { get; }
    public EntityValueCache<Regime, RegimeAi> AIs { get; }
    private ConcurrentDictionary<Regime, TurnOrders> _submittedTurn;
    private HostServer _server;
    private HostWriteKey _hKey;
    private ProcedureWriteKey _pKey;
    private Data _data;
    private Task<LogicResults> _calculatingLogicResult;
    private Task<bool> _calculatingAiOrders;
    private LogicModule[] _majorModules, _minorModules;
    public HostLogic(Data data)
    {
        _submittedTurn = new ConcurrentDictionary<Regime, TurnOrders>();
        AIs = EntityValueCache<Regime, RegimeAi>
            .ConstructConstant(data, r => new RegimeAi(r, data));
        CommandQueue = new ConcurrentQueue<Command>();
        _majorModules = new LogicModule[]
        {
            new WorkProdConsumeModule(),
            new ConstructBuildingsModule(),
            new PeepGrowthModule()
        };
        _minorModules = new LogicModule[] { };
    }

    public bool Process(float delta)
    {
        DoCommands();
        if (_data.Society.Regimes.Entities.All(p => _submittedTurn.ContainsKey(p)))
        {
            _submittedTurn.Clear();
            _calculatingLogicResult = Task.Run(CalculateFrameResults);
            return false;
        }
        else if (_calculatingLogicResult != null && _calculatingLogicResult.IsCompleted)
        {
            if( _calculatingLogicResult.Exception != null)
            {
                throw _calculatingLogicResult.Exception;
            }
            
            var result = _calculatingLogicResult.Result;
            EnactFrame(result);
            DoCommands();
            _calculatingLogicResult = null;
            
            _calculatingAiOrders = Task.Run(() =>
            {
                if (_data.BaseDomain.GameClock.MajorTurn(_data))
                {
                    CalcAiMajorTurnOrders();
                    return true;
                }
                else
                {
                    CalcAiMinorTurnOrders();
                    return true;
                }
            });
            return true;
        }
        return false;

    }
    public void SetDependencies(HostServer server, GameSession session, Data data)
    {
        _data = data;
        _server = server;
        _hKey = new HostWriteKey(server, this, data, session);
        _pKey = new ProcedureWriteKey(data, session);
    }

    public void SubmitTurn(TurnOrders orders)
    {
        _submittedTurn.TryAdd(orders.Regime.Entity(), orders);
    }

    private void CalcAiMajorTurnOrders()
    {
        var aiRegimes = _data.Society.Regimes.Entities
            .Where(r => r.IsPlayerRegime(_data) == false);
        Parallel.ForEach(aiRegimes, r =>
        {
            var orders = AIs[r].GetMajorTurnOrders(_data);
            SubmitTurn(orders);
        });
    }
    private void CalcAiMinorTurnOrders()
    {
        var aiRegimes = _data.Society.Regimes.Entities
            .Where(r => r.IsPlayerRegime(_data) == false);
        Parallel.ForEach(aiRegimes, r =>
        {
            var orders = AIs[r].GetMinorTurnOrders(_data);
            SubmitTurn(orders);
        });
    }
    private LogicResults CalculateFrameResults()
    {
        if (_data.BaseDomain.GameClock.MajorTurn(_data))
        {
            return DoMajorTurn();
        }
        else
        {
            return DoMinorTurn();
        }
    }

    private LogicResults DoMajorTurn()
    {
        var msgs = new ConcurrentBag<Message>();
        var entityCreateFuncs = new ConcurrentBag<Func<HostWriteKey, Entity>>();
        
        foreach (var m in _majorModules)
        {
            m.Calculate(_data, msgs.Add, entityCreateFuncs.Add);
        }
        
        foreach (var kvp in _submittedTurn)
        {
            var orders = (MajorTurnOrders) kvp.Value;
            orders.StartConstructions.Enact(_data, msgs.Add, 
                entityCreateFuncs.Add);
        }
        return new LogicResults(msgs, entityCreateFuncs);
    }
    private LogicResults DoMinorTurn()
    {
        var msgs = new ConcurrentBag<Message>();
        var entityCreateFuncs = new ConcurrentBag<Func<HostWriteKey, Entity>>();
        
        foreach (var m in _minorModules)
        {
            m.Calculate(_data, msgs.Add, entityCreateFuncs.Add);
        }
        
        foreach (var kvp in _submittedTurn)
        {
            var orders = (MinorTurnOrders) kvp.Value;
            
        }
        return new LogicResults(msgs, entityCreateFuncs);
    }
    private void EnactFrame(LogicResults logicResult)
    {
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
        //todo ticking for remote as well?
        new TickProcedure().Enact(_pKey);
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
