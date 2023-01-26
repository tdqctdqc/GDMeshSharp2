using Godot;
using System;

public interface IServer
{
    int NetworkId { get; }
    void PushCommand(Command command);
}
