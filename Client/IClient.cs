using Godot;
using System;

public interface IClient
{
    void HandleInput(InputEvent e, float delta);
    CameraController Cam { get; }
}
