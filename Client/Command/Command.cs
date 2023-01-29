using Godot;
using System;

public abstract class Command
{
    protected Command(WriteKey key)
    {
        
    }
    public byte[] GetPacketBytes()
    {
        var uBytes = Game.I.Serializer.SerializeToUtf8(this);
        var wrapper = new CommandWrapper(GetType().Name, uBytes);
        var wrapperBytes = Game.I.Serializer.SerializeToUtf8(wrapper);
        return wrapperBytes;
    }
    
    public static Command DecodePacket(byte[] packet)
    {
        var wrapper = Game.I.Serializer.Deserialize<CommandWrapper>(packet);
        var updateType = Game.I.Serializer.Types[wrapper.CommandName];
        return (Command)Game.I.Serializer.Deserialize(wrapper.CommandBytes, updateType);
    }
    public abstract void Enact(HostWriteKey key);
}
