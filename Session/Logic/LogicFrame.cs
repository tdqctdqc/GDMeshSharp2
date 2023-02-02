using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public class LogicFrame
{
    private LogicModule[] _modules;
    private List<Procedure> _queuedProcedures;
    private ConcurrentDictionary<int, List<Procedure>> _conc;

    public LogicFrame(params LogicModule[] modules)
    {
        _conc = new ConcurrentDictionary<int, List<Procedure>>();
        _modules = modules;
        _queuedProcedures = new List<Procedure>();
    }

    public List<Procedure> Calculate()
    {
        _conc.Clear();
        _queuedProcedures.Clear();
        var modCount = _modules.Length;
        var tasks = new Task[_modules.Length];
        for (var i = 0; i < _modules.Length; i++)
        {
            GD.Print("handing module " + i);
            int iter = i;
            var t = new Task(() =>
            {
                GD.Print("starting module " + iter);
                var processProcs = _modules[iter].Calculate();
                _conc.AddOrUpdate(iter, processProcs, (i1, list) => list);
                GD.Print("finished module " + iter);
            });
            tasks[i] = t;
            t.Start();
        }
        Task.WaitAll(tasks);
        for (var i = 0; i < _modules.Length; i++)
        {
            _queuedProcedures.AddRange(_conc[i]);
        }

        return _queuedProcedures;
    }
}