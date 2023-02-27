using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class HostLogic : ILogic
{
    public Queue<Command> CommandQueue { get; private set; }
    private HostServer _server;
    private HostWriteKey _hKey;
    private ProcedureWriteKey _pKey;
    private Data _data;
    private readonly LogicFrame[] _frames;
    private int _frameIter = 0;
    private float _framePeriod = .1f;
    private float _frameTimer = .1f;
    private bool _calculating;
    
    public HostLogic()
    {
        CommandQueue = new Queue<Command>();
        _frames = new LogicFrame[]
        {
            new LogicFrame()
        };
    }

    public void Process(float delta)
    {
        _frameTimer += delta;
        if (_frameTimer >= _framePeriod 
            && _calculating == false)
        {
            _frameTimer = 0f;
            _calculating = true;
            DoFrame();
        }
    }
    public void SetDependencies(HostServer server, Data data)
    {
        _data = data;
        _server = server;
        _hKey = new HostWriteKey(server, data);
        _pKey = new ProcedureWriteKey(data);
    }

    public async void DoFrame()
    {
        var procs = await Task.Run(() => _frames[_frameIter].Calculate(_data));
        _frameIter = (_frameIter + 1) % _frames.Length;

        for (var i = 0; i < procs.Count; i++)
        {
            procs[i].Enact(_pKey);
        }
        
        _calculating = false;
    }
}
