using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HostServer : Node, IServer
{
    public int NetworkId => 1;
    

    private HostWriteKey _key;
    private HostLogic _logic;
    private List<object[]> _queuedUpdates;
    private List<Command> _queuedCommands;
    private List<int> _clients;
    private List<StreamPeerTCP> _connections;
    private NetworkedMultiplayerENet _network; 
    private TCP_Server _tcp;
    // private StreamPeerTCP _peer;
    // private PacketPeerStream _packet;

    private string _ip = "127.0.0.1";
    private int _port = 3306;
    private int _maxPlayers = 100;

    private float _tickTime = .25f;
    private float _ticker = 0f;
    public override void _Ready()
    {
        _connections = new List<StreamPeerTCP>();
        _clients = new List<int>();
        _queuedUpdates = new List<object[]>();
        _queuedCommands = new List<Command>();
        _network = new NetworkedMultiplayerENet();
        _network.CreateServer(_port, _maxPlayers);
        GetTree().NetworkPeer = _network;
        _network.Connect("peer_connected", this, nameof(PeerConnected));
        _network.Connect("peer_disconnected", this, nameof(PeerDisconnected));
        
        _tcp = new TCP_Server();
        _tcp.Listen((ushort)_port);
        // _peer = new StreamPeerTCP();
        // _peer.ConnectToHost(_ip, _port);
        // _packet = new PacketPeerStream();
        // _packet.StreamPeer = _peer;
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
        if (_tcp.IsConnectionAvailable())
        {
            GD.Print("connection available");
            var connection = _tcp.TakeConnection();
            var hostPacket = new PacketPeerStream();
            hostPacket.StreamPeer = connection;
            StateTransferUpdate.Send(_key, connection, hostPacket);
            // _connections.Add(connection);
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
        _queuedUpdates.ForEach(u =>
        {
            _connections.ForEach(c =>
            {
                c.PutVar(u);
            });
        });
        _queuedUpdates.Clear();
    }

    private void TransferState()
    {
        
    }
    private void PeerConnected(int id)
    {
        _clients.Add(id);
        GD.Print("peer " + id + " connected");
        RpcId(id, nameof(RemoteServer.OnConnectionSucceeded));
    }
    private void PeerDisconnected(int id)
    {
        _clients.Remove(id);
    }
    [Remote] public void ReceiveCommand(object[] commandArgs)
    {
        var commandTypeName = (string)commandArgs[0];
        var commandMeta = Game.I.Serializer.GetCommandMeta(commandTypeName);
        //todo revert
        // var command = commandMeta.Deserialize(commandArgs);
        // _queuedCommands.Add(command);
    }
    public void PushCommand(Command command)
    {
        _queuedCommands.Add(command);
    }
    private void ProcessCommands()
    {
        _queuedCommands.ForEach(c =>
        {
            c.Enact(_key);
        });
    }
}
