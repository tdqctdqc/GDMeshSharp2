using Godot;
using System;

public interface IUpdate
{
    string UpdateType { get; }
    string Serialize();
}
