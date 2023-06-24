using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class LogicFrame
{
    private LogicModule[] _modules;
    public LogicFrame(params LogicModule[] modules)
    {
        _modules = modules;
    }

    public LogicResults Calculate(Data data)
    {
        var results = new ConcurrentBag<Message>();
        var entityCreateFuncs = new ConcurrentBag<Func<HostWriteKey, Entity>>();
        Parallel.ForEach(_modules, m =>
        {
            Game.I.Logger.RunAndLogTime(
                () => m.Calculate(data, results.Add, entityCreateFuncs.Add),
                m.GetType().Name, LogType.Logic);
        });
        
        return new LogicResults(results, entityCreateFuncs);
    }
}