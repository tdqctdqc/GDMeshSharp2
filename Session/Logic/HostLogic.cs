using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class HostLogic : ILogic
{
    public ConcurrentQueue<Command> CommandQueue { get; }
    public EntityValueCache<Regime, RegimeAi> AIs { get; }
    private ConcurrentDictionary<Player, TurnOrders> _playerTurnOrders;
    private ConcurrentDictionary<Regime, Task<TurnOrders>> _aiTurnOrders;
    private HostServer _server;
    private HostWriteKey _hKey;
    private ProcedureWriteKey _pKey;
    private Data _data;
    private Task<LogicResults> _calculatingLogicResult;
    private Task<bool> _calculatingAiOrders;
    private LogicModule[] _majorModules, _minorModules;
    public HostLogic(Data data)
    {
        _playerTurnOrders = new ConcurrentDictionary<Player, TurnOrders>();
        _aiTurnOrders = new ConcurrentDictionary<Regime, Task<TurnOrders>>();
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

    public void Start()
    {
        DoCommands();
        CalcAiTurnOrders();
    }
    public bool Process(float delta)
    {
        if (_calculatingLogicResult == null)
        {
            DoCommands();
            if(CheckReadyForFrame())
            {
                //make submitting 'final' on players part
                _calculatingLogicResult = Task.Run(CalculateFrameResults);
                return false;
            }
        }
        else if (_calculatingLogicResult != null && _calculatingLogicResult.IsCompleted)
        {
            DoCommands();
            DoFrame();
            CalcAiTurnOrders();
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

    public void SubmitTurn(Player player, TurnOrders orders)
    {
        if (orders.Tick != _data.BaseDomain.GameClock.Tick) throw new Exception();
        var added = _playerTurnOrders.TryAdd(player, orders);
        if (added == false) throw new Exception();
    }

    private bool CheckReadyForFrame()
    {
        var players = _data.BaseDomain.Players.Entities;
        var aiRegimes = _data.Society.Regimes.Entities.Where(r => r.IsPlayerRegime(_data) == false);
        foreach (var kvp in _aiTurnOrders)
        {
            if (kvp.Value.IsFaulted)
            {
                throw kvp.Value.Exception;
            }
        }

        var allPlayersHaveRegime = players.All(p => p.Regime.Empty() == false);
        // if(allPlayersHaveRegime == false) GD.Print("players dont have regime");
        
        var allPlayersSubmitted = players.All(p => _playerTurnOrders.ContainsKey(p));
        // if(allPlayersSubmitted == false) GD.Print("players havent submitted");

        var allAisHaveEntry = aiRegimes.All(p => _aiTurnOrders.ContainsKey(p));
        // if(allAisHaveEntry == false) GD.Print("ais not entered");

        var allAisCompleted = _aiTurnOrders.All(kvp => kvp.Value.IsCompleted);
        // if(allAisHaveEntry == false) GD.Print("ais not finished");

        return allPlayersHaveRegime && allPlayersSubmitted && allAisHaveEntry && allAisCompleted;
    }
    private void DoFrame()
    {
        if( _calculatingLogicResult.Exception != null)
        {
            throw _calculatingLogicResult.Exception;
        }
            
        var result = _calculatingLogicResult.Result;
        EnactFrame(result);
        _playerTurnOrders.Clear();
        _aiTurnOrders.Clear();
        _calculatingLogicResult = null;
    }
    private void CalcAiTurnOrders()
    {
        if (_data.BaseDomain.GameClock.MajorTurn(_data))
        {
            inner(r => AIs[r].GetMajorTurnOrders(_data));
        }
        else
        {
            inner(r => AIs[r].GetMinorTurnOrders(_data));
        }

        void inner(Func<Regime, TurnOrders> getOrders)
        {
            var aiRegimes = _data.Society.Regimes.Entities
                .Where(r => r.IsPlayerRegime(_data) == false);
            foreach (var aiRegime in aiRegimes)
            {
                if (_aiTurnOrders.ContainsKey(aiRegime) == false)
                {
                    var task = Task.Run(() =>
                    {
                        return (TurnOrders) getOrders(aiRegime);
                    });
                    _aiTurnOrders.TryAdd(aiRegime, task);
                }
            }
        }
    }
    private LogicResults CalculateFrameResults()
    {
        if (_data.BaseDomain.GameClock.MajorTurn(_data))
        {
            return inner<MajorTurnOrders>((o, m, e) =>
                    ProcessMajorTurnOrders(o, m, e),
                _majorModules);
        }
        else
        {
            return inner<MinorTurnOrders>((o, m, e) =>
                    ProcessMinorTurnOrders(o, m, e),
                _minorModules);
        }

        LogicResults inner<T>(Action<T, Action<Message>, Action<Func<HostWriteKey, Entity>>> handleTurnOrders, LogicModule[] modules) where T : TurnOrders
        {
            var msgs = new ConcurrentBag<Message>();
            var entityCreateFuncs = new ConcurrentBag<Func<HostWriteKey, Entity>>();
        
            foreach (var kvp in _playerTurnOrders)
            {
                if (kvp.Key.Regime.Entity().IsPlayerRegime(_data) == false) continue;
                var orders = (T) kvp.Value;
                handleTurnOrders(orders, msgs.Add, entityCreateFuncs.Add);
            }
            foreach (var kvp in _aiTurnOrders)
            {
                if (kvp.Key.IsPlayerRegime(_data)) continue;
                var orders = (T) kvp.Value.Result;
                handleTurnOrders(orders, msgs.Add, entityCreateFuncs.Add);
            }
            foreach (var m in modules)
            {
                m.Calculate(_data, msgs.Add, entityCreateFuncs.Add);
            }

            return new LogicResults(msgs, entityCreateFuncs);
        }
    }

    

    private void ProcessMajorTurnOrders(MajorTurnOrders orders, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        orders.StartConstructions.Enact(_data, queueMessage, 
            queueEntityCreation);
    }
    private void ProcessMinorTurnOrders(MinorTurnOrders orders, Action<Message> queueMessage,
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
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
