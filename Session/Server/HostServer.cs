using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HostServer : Node, IServer
{
    public int NetworkId => 1;
    private HostWriteKey _key;
    private HostLogic _logic;
    private List<HostSyncer> _peers;
    private Dictionary<Guid, HostSyncer> _peersByGuid;
    public Queue<Command> QueuedCommands { get; private set; }
    private TCP_Server _tcp;
    private MessageManager _msg;
    private string _ip = "127.0.0.1";
    private int _port = 3306;
    private int _maxPlayers = 100;
    public override void _Ready()
    {
        _peers = new List<HostSyncer>();
        _peersByGuid = new Dictionary<Guid, HostSyncer>();
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
        var packet = new PacketPeerStream();
        packet.StreamPeer = peer;

        var syncer = new HostSyncer(packet, _msg);
        GD.Print("started syncing");
        syncer.Sync(_key);
        GD.Print("Done syncing");

        _peers.Add(syncer);
    }
    public void SetDependencies(HostLogic logic, Data data, GameSession session)
    {
        _logic = logic;
        _key = new HostWriteKey(this, logic, data, session);
        _msg = new MessageManager(u => { }, 
            p => { }, 
            logic.CommandQueue.Enqueue,
            d => { });
    }

    public void QueueUpdate(Update u)
    {
        var bytes = _msg.WrapUpdate(u);
        for (var i = 0; i < _peers.Count; i++)
        {
            _peers[i].QueuePacket(bytes);
        }
    }
    public void ReceiveLogicResult(LogicResult result, HostWriteKey key)
    {
        for (var i = 0; i < result.Procedures.Count; i++)
        {
            var bytes = _msg.WrapProcedure(result.Procedures[i]);
            for (var j = 0; j < _peers.Count; j++)
            {
                _peers[j].QueuePacket(bytes);
            }
        }

        for (var i = 0; i < result.Updates.Count; i++)
        {
            var bytes = _msg.WrapUpdate(result.Updates[i]);
            for (var j = 0; j < _peers.Count; j++)
            {
                _peers[j].QueuePacket(bytes);
            }
        }
        
        for (var i = 0; i < result.Decisions.Count; i++)
        {
            var d = result.Decisions[i];
            if (d.Decided) continue;
            if (d.IsPlayerDecision(_key.Data))
            {
                var p = d.Decider.Entity().GetPlayer(_key.Data);
                if (p.PlayerGuid == Game.I.PlayerGuid)
                {
                    _key.Data.Notices.NeedDecision?.Invoke(d);
                }
                else
                {
                    var bytes = _msg.WrapDecision(d);
                    var peer = _peersByGuid[p.PlayerGuid];
                    peer.QueuePacket(bytes);
                }
            }
        }
    }

    public void PushPackets(HostWriteKey key)
    {
        _peers.ForEach(p => p.PushPackets(key));
    }
    public void QueueCommand(Command c, WriteKey key)
    {
        _logic.CommandQueue.Enqueue(c);
    }
}
