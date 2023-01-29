using Godot;
using System;

public interface ISession
{
    Data Data { get; }
    IClient Client { get; }
    void StartClient(IServer server, ServerWriteKey key);
}
