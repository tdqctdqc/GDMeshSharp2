using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RemoteSyncer : Syncer
{
    public RemoteSyncer(PacketPeerStream packetStream, RemoteLogic logic) 
        : base(packetStream, 
        logic.ProcessUpdate, logic.ProcessProcedure, c => {})
    {
    }


    public void SendCommand(Command command)
    {
        var bytes = _msg.WrapCommand(command);
        PushPacket(bytes);
    }

    
}