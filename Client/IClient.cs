using Godot;
using System;

public interface IClient
{
    Data Data { get; }
    void HandleInput(InputEvent e, float delta);
    void Process(float delta);
    CameraController Cam { get; }
}
