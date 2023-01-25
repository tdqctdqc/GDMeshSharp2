using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HostServer : Node, IServer
{
    public int NetworkId => 1;

    private HostWriteKey _key;
    public static HostServer ForTest;
    private HostLogic _logic;
    private List<object[]> _queuedUpdates;
    private List<Tuple<string, string>> _queuedCommands;
    private List<int> _clients;
    private NetworkedMultiplayerENet _network; 
    private string _ip = "127.0.0.1";
    private int _port = 3306;
    private int _maxPlayers = 100;

    private float _tickTime = .25f;
    private float _ticker = 0f;
    public override void _Ready()
    {
        ForTest = this;
        _clients = new List<int>();
        _queuedUpdates = new List<object[]>();
        _queuedCommands = new List<Tuple<string, string>>();
        _network = new NetworkedMultiplayerENet();
        _network.CreateServer(_port, _maxPlayers);
        GetTree().NetworkPeer = _network;
        _network.Connect("peer_connected", this, nameof(PeerConnected));
        _network.Connect("peer_disconnected", this, nameof(PeerDisconnected));
    }

    public override void _Process(float delta)
    {
        _ticker += delta;
        if (_ticker >= _tickTime)
        {
            _ticker = 0;
            BroadcastUpdates();
            ProcessCommands();
        }
    }

    public void SetDependencies(HostLogic logic, Data data)
    {
        _logic = logic;
        _key = new HostWriteKey(this, data);
    }

    public void QueueUpdate(Update u)
    {
        _queuedUpdates.Add(u.GetMeta().GetArgs(u));
    }

    private void BroadcastUpdates()
    {
        // Rpc(nameof(RemoteServer.ReceiveUpdates), updatesJson, updateTypesJson);
        _queuedUpdates.Clear();
    }
    private void PeerConnected(int id)
    {
        _clients.Add(id);
        GD.Print("peer " + id + " connected");
        RpcId(id, nameof(RemoteServer.OnConnectionSucceeded));
        StateTransferUpdate.Send(_key, id);
    }
    private void PeerDisconnected(int id)
    {
        _clients.Remove(id);
    }
    [Remote] public void ReceiveCommand(string commandType, string commandJson)
    {
        _queuedCommands.Add(new Tuple<string, string>(commandType, commandJson));
    }

    private void ProcessCommands()
    {
        for (var i = 0; i < _queuedCommands.Count; i++)
        {
            var commandType = _queuedCommands[i].Item1;
            var commandJson = _queuedCommands[i].Item2;
            CommandMeta.Enact(_key, commandType, commandJson);
        }
        _queuedCommands.Clear();
    }
}
