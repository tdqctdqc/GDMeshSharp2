using Godot;
using System;

public interface IClient
{
    ClientRequests Requests { get; }
    void HandleInput(InputEvent e, float delta);
    void Process(float delta);
    ICameraController Cam { get; }
    ClientSettings Settings { get; }
    ClientWriteKey Key { get; }
}
