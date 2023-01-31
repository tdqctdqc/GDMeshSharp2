using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

public class RemoteServer : Node, IServer
{
    public int NetworkId { get; private set; }
    public Action ReceivedStateTransfer { get; set; }

    private ServerWriteKey _key;
    private NetworkedMultiplayerENet _network;
    private StreamPeerTCP _streamPeer;
    private PacketPeerStream _packetStream;
    private string _ip = "127.0.0.1";
    private int _port = 3306;

    public void Setup(ISession session, Data data)
    {
        _key = new ServerWriteKey(data);
    }
    public override void _Ready()
    {
        _network = new NetworkedMultiplayerENet();
        _network.CreateClient(_ip, _port);
        _streamPeer = new StreamPeerTCP();
        if (_streamPeer.IsConnectedToHost() == false)
        {
            _streamPeer.ConnectToHost(_ip, _port);
            _packetStream = new PacketPeerStream();
            _packetStream.StreamPeer = _streamPeer;
            SetProcess(true);
        }
        GetTree().NetworkPeer = _network;
        _network.Connect("connection_failed", this, nameof(OnConnectionFailed));
    }
    public override void _Process(float delta)
    {
        // var availPackets = _packetStream.GetAvailablePacketCount();
        //
        // // if(availPackets > 0) GD.Print("avail packets " + availPackets);
        // for (int i = 0; i < availPackets; i++)
        // {
        //     var packet = _packetStream.GetPacket();
        //     var argsBytes = Game.I.Serializer.Deserialize<UpdateWrapper>(packet);
        //     ReceiveUpdate(argsBytes);
        // }
    }
    
    public void ReceiveDependencies(ISession session, Data data)
    {
        _key = new ServerWriteKey(data);
    }
    [Remote] public void OnConnectionSucceeded()
    {
        NetworkId = _network.GetUniqueId();
        GD.Print("connection succeeded, id is " + NetworkId);
    }

    [Remote] public void OnConnectionFailed()
    {
        GD.Print("connection failed");
    }
    public void ReceiveUpdate(UpdateWrapper w)
    {
        // var updateType = Game.I.Serializer.Types[w.UpdateName];
        // // var updateMeta = Game.I.Serializer.GetUpdateMeta(updateTypeName);
        // var update = (Update)Game.I.Serializer.Deserialize(w.UpdateBytes, updateType);
        // update.Enact(_key);
    }

    public void PushCommand(Command command)
    {
        //todo send to hostserver sadf
    }
}
