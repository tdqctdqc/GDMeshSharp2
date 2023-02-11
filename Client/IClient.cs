using Godot;
using System;

public interface IClient
{
    void HandleInput(InputEvent e, float delta);
    void ProcessPoly(float delta);
    CameraController Cam { get; }
}
