using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteSyncer : Syncer
{
    public RemoteSyncer(PacketPeerStream packetStream, RemoteLogic logic) 
        : base(packetStream, 
            new MessageManager(
                logic.ProcessUpdate, 
                logic.ProcessProcedure, 
                c => {},
                logic.ProcessDecision), 
            new Guid())
    {
    }

    public void SendCommand(Command command)
    {
        var bytes = _msg.WrapCommand(command);
        PushPacket(bytes);
    }
    
}