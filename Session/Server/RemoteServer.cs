using Godot;
using System;
using System.Collections.Generic;

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
        var availPackets = _packetStream.GetAvailablePacketCount();
        if(availPackets > 0) GD.Print("avail packets " + availPackets);
        for (int i = 0; i < availPackets; i++)
        {
            ReceiveUpdates((object[])_packetStream.GetVar(true));
        }
    }
    public void ReceiveDependencies(Data data)
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
    public void ReceiveUpdates(object[] updateArgs)
    {
        var updateTypeName = (string)updateArgs[0];
        var updateMeta = Game.I.Serializer.GetUpdateMeta(updateTypeName);
        var update = updateMeta.Deserialize(updateArgs);
        update.Enact(_key);
    }

    [Remote] public void ReceiveStateTransfer(object[] stateTransferArgs)
    {
        var meta = Game.I.Serializer.GetUpdateMeta<StateTransferUpdate>();
        var update = meta.Deserialize(stateTransferArgs);
        update.Enact(_key);
    }
    public void PushCommand(Command command)
    {
        //todo send to hostserver
    }
}
