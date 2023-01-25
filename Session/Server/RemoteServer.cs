using Godot;
using System;
using System.Collections.Generic;

public class RemoteServer : Node, IServer
{
    public int NetworkId { get; private set; }
    public Action ReceivedStateTransfer { get; set; }

    private ServerWriteKey _key;
    private NetworkedMultiplayerENet _network;
    private string _ip = "127.0.0.1";
    private int _port = 3306;
    public override void _Ready()
    {
        _network = new NetworkedMultiplayerENet();
        _network.CreateClient(_ip, _port);
        GetTree().NetworkPeer = _network;
        _network.Connect("connection_failed", this, nameof(OnConnectionFailed));
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
    [Remote] public void ReceiveUpdates(object[][] updates)
    {
        for (int i = 0; i < updates.Length; i++)
        {
            var updateArgs = updates[i];
            var updateTypeName = (string)updateArgs[0];
            var updateMeta = Game.I.Serializer.GetUpdateMeta(updateTypeName);
            var update = updateMeta.Deserialize(updateArgs);
            update.Enact(_key);
        }
    }

    [Remote] public void ReceiveStateTransfer(object[] stateTransferArgs)
    {
        var meta = Game.I.Serializer.GetUpdateMeta<StateTransferUpdate>();
        var update = meta.Deserialize(stateTransferArgs);
        update.Enact(_key);
    }
    public void ReceiveCommand(string commandType, string commandJson)
    {
        RpcId(0, nameof(ReceiveCommand), commandType, commandJson);
    }
}
