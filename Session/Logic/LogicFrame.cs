using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class LogicFrame
{
    private LogicModule[] _modules;
    private List<LogicResult> _results;
    private ConcurrentDictionary<int, LogicResult> _conc;

    public LogicFrame(params LogicModule[] modules)
    {
        _conc = new ConcurrentDictionary<int, LogicResult>();
        _modules = modules;
        _results = new List<LogicResult>();
    }

    public LogicResult Calculate(Data data)
    {
        _conc.Clear();
        _results.Clear();
        var modCount = _modules.Length;
        var tasks = new Task[_modules.Length];
        for (var i = 0; i < _modules.Length; i++)
        {
            int iter = i;
            var t = new Task(() =>
            {
                var result = _modules[iter].Calculate(data);
                _conc.AddOrUpdate(iter, result, (i1, list) => list);
            });
            tasks[i] = t;
            t.Start();
        }
        Task.WaitAll(tasks);
        for (var i = 0; i < _modules.Length; i++)
        {
            _results.Add(_conc[i]);
        }

        return new LogicResult(_results);
    }
}