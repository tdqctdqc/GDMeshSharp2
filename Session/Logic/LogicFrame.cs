using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class LogicFrame
{
    private LogicModule[] _modules;
    private ConcurrentBag<IResult> _results;

    public LogicFrame(params LogicModule[] modules)
    {
        _modules = modules;
    }

    public LogicResults Calculate(Data data)
    {
        _results = new ConcurrentBag<IResult>();
        var modCount = _modules.Length;
        Parallel.ForEach(_modules, m =>
        {
            var result = m.Calculate(data);
            _results.Add(result);
        });

        return new LogicResults(_results);
    }
}