using Godot;
using System;
using System.Collections.Generic;

public class ClientServer : Node, IServer
{
    public int NetworkId { get; private set; }
    

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
    [Remote] public void ReceiveUpdates(string updatesJson, string updateTypesJson)
    {
        var updatesJsonsList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(updatesJson);
        var updateTypes = System.Text.Json.JsonSerializer.Deserialize<List<string>>(updateTypesJson);

        for (int i = 0; i < updatesJsonsList.Count; i++)
        {
            var updateType = updateTypes[i];
            if (updateType == EntityVarUpdate.UpdateType)
            {
                EntityVarUpdate.DeserializeAndEnact(updatesJsonsList[i], _key);
            }
            else if (updateType == EntityCreationUpdate.UpdateType)
            {
                EntityCreationUpdate.DeserializeAndEnact(updatesJsonsList[i], _key);
            }
            else if (updateType == StateTransferUpdate.UpdateType)
            {
                StateTransferUpdate.DeserializeAndEnact(updatesJsonsList[i], _key);
            }
            else if (updateType == ProcedureUpdate.UpdateType)
            {
                ProcedureUpdate.DeserializeAndEnact(updatesJsonsList[i], _key);
            }
        }
    }
    public void ReceiveCommand(string commandType, string commandJson)
    {
        RpcId(0, nameof(ReceiveCommand), commandType, commandJson);
    }
}
