using Godot;
using System;

public interface ISession
{
    RefFulfiller RefFulfiller { get; }
    IClient Client { get; }
    void QueueFree();
    Guid PlayerGuid { get; }
}
