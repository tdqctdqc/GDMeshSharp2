using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HostServer : Node, IServer
{
    public int NetworkId => 1;
    private HostWriteKey _key;
    private HostLogic _logic;
    private List<int> _clients;
    private List<HostSyncer> _peers;
    private List<StreamPeerTCP> _connections;
    private Queue<byte[]> _wrappedPacketsToSend;
    public Queue<Command> QueuedCommands { get; private set; }
    private TCP_Server _tcp;
    private MessageManager _msg;
    private string _ip = "127.0.0.1";
    private int _port = 3306;
    private int _maxPlayers = 100;
    public override void _Ready()
    {
        _connections = new List<StreamPeerTCP>();
        _clients = new List<int>();
        _peers = new List<HostSyncer>();
        _tcp = new TCP_Server();
        _tcp.Listen((ushort)_port);
    }

    public override void _Process(float delta)
    {
        if (_tcp.IsConnectionAvailable())
        {
            GD.Print("connection available");
            var peer = _tcp.TakeConnection();
            HandleNewPeer(peer);
        }
    }

    private void HandleNewPeer(StreamPeerTCP peer)
    {

        // peer.ConnectToHost(_ip, _port);
        var packet = new PacketPeerStream();
        packet.StreamPeer = peer;

        var syncer = new HostSyncer(packet, _msg);
        GD.Print("started syncing");
        syncer.Sync(_key);
        GD.Print("Done syncing");

        _peers.Add(syncer);
    }
    public void SetDependencies(HostLogic logic, Data data)
    {
        _logic = logic;
        _key = new HostWriteKey(this, data);
        _msg = new MessageManager(u => { }, p => { }, logic.CommandQueue.Enqueue);
    }

    public void QueueUpdate(Update u)
    {
        var bytes = _msg.WrapUpdate(u);
        for (var i = 0; i < _peers.Count; i++)
        {
            _peers[i].QueuePacket(bytes);
        }
    }

    public void ReceiveLogicMessages(List<Procedure> procs, List<Update> updates, HostWriteKey key)
    {
        for (var i = 0; i < procs.Count; i++)
        {
            var bytes = _msg.WrapProcedure(procs[i]);
            for (var j = 0; j < _peers.Count; j++)
            {
                _peers[j].QueuePacket(bytes);
            }
        }

        for (var i = 0; i < updates.Count; i++)
        {
            var bytes = _msg.WrapUpdate(updates[i]);
            for (var j = 0; j < _peers.Count; j++)
            {
                _peers[j].QueuePacket(bytes);
            }
        }
    }
}
