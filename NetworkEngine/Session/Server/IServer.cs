using Godot;
using System;

public interface IServer
{
    int NetworkId { get; }
    void ReceiveCommand(string commandType, string commandJson);
}
