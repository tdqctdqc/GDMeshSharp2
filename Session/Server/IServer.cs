using Godot;
using System;

public interface IServer
{
    void QueueCommand(Command c, WriteKey key);
}
