using Godot;
using System;

public class ServerWriteKey : StrongWriteKey
{
    public ISession Session { get; private set; }
    public IServer Server { get; private set; }
    public ServerWriteKey(IServer server, ISession session, Data data) : base(data)
    {
        Session = session;
    }
}
