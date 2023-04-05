using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        var modCount = _modules.Length;
        Parallel.ForEach(_modules, m =>
        {
            m.Calculate(data, results.Add, entityCreateFuncs.Add);
            // GD.Print("finished module " + m.GetType().Name);
        });
        
        return new LogicResults(results, entityCreateFuncs);
    }
}