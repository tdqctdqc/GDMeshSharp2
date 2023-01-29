using Godot;
using System;

public abstract class Update
{
    protected Update(HostWriteKey key)
    {
        
    }
    public byte[] GetPacketBytes()
    {
        var uBytes = Game.I.Serializer.SerializeToUtf8(this);
        var wrapper = new UpdateWrapper(this.GetType().Name, uBytes);
        var wrapperBytes = Game.I.Serializer.SerializeToUtf8(wrapper);
        return wrapperBytes;
    }

    public static Update DecodePacket(byte[] packet)
    {
        var wrapper = Game.I.Serializer.Deserialize<UpdateWrapper>(packet);
        var updateType = Game.I.Serializer.Types[wrapper.UpdateName];
        return (Update)Game.I.Serializer.Deserialize(wrapper.UpdateBytes, updateType);
    }

    public abstract void Enact(ServerWriteKey key);
}
