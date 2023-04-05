using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class HostServer : Node, IServer
{
    private HostWriteKey _key;
    private HostLogic _logic;
    private List<HostSyncer> _peers;
    private Dictionary<Guid, HostSyncer> _peersByGuid;
    private TCP_Server _tcp;
    private int _port = 3306;
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
        var newPlayerGuid = Guid.NewGuid();
        var syncer = new HostSyncer(packet, _logic, newPlayerGuid);
        GD.Print("started syncing");
        syncer.Sync(newPlayerGuid, _key);
        GD.Print("Done syncing");
        _peers.Add(syncer);
        _peersByGuid.Add(newPlayerGuid, syncer);
    }
    public void SetDependencies(HostLogic logic, Data data, GameSession session)
    {
        _logic = logic;
        _key = new HostWriteKey(this, logic, data, session);
    }

    public void QueueUpdate(Update u)
    {
        var bytes = u.Wrap();
        for (var i = 0; i < _peers.Count; i++)
        {
            _peers[i].QueuePacket(bytes);
        }
    }
    public void ReceiveLogicResult(LogicResults results, HostWriteKey key)
    {
        for (var i = 0; i < results.Procedures.Count; i++)
        {
            var bytes = results.Procedures[i].Wrap();
            for (var j = 0; j < _peers.Count; j++)
            {
                _peers[j].QueuePacket(bytes);
            }
        }

        for (var i = 0; i < results.Updates.Count; i++)
        {
            var bytes = results.Updates[i].Wrap();
            for (var j = 0; j < _peers.Count; j++)
            {
                _peers[j].QueuePacket(bytes);
            }
        }
        
        for (var i = 0; i < results.Decisions.Count; i++)
        {
            var d = results.Decisions[i];
            if (d.Decided) continue;
            if (d.IsPlayerDecision(_key.Data))
            {
                var p = d.Decider.Entity().GetPlayer(_key.Data);
                if (p.PlayerGuid != Game.I.PlayerGuid)
                {
                    var bytes = d.Wrap();
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
    public void QueueCommandLocal(Command c)
    {
        GD.Print("queueing command in server");
        c.SetGuid(Game.I.PlayerGuid);
        _logic.CommandQueue.Enqueue(c);
    }
}
