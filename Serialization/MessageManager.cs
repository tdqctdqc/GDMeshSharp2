
using System;
using System.Collections.Generic;
using Godot;

public class MessageManager
{
    private Dictionary<byte, IMessageTypeManager> _managersByByte;
    private Dictionary<Type, IMessageTypeManager> _managersByType;
    private Dictionary<Type, byte> _markers;
    private PacketProtocol _protocol;
    public MessageManager(Action<Update> handleUpdate, Action<Procedure> handleProcedure, Action<Command> handleCommand)
    {
        _managersByByte = new Dictionary<byte, IMessageTypeManager>();
        _managersByType = new Dictionary<Type, IMessageTypeManager>();
        _markers = new Dictionary<Type, byte>();
        AddType<Update>(handleUpdate);
        AddType<Procedure>(handleProcedure);
        AddType<Command>(handleCommand);
    }
    public void HandleIncoming(byte[] packet)
    {
        var msg = Game.I.Serializer.MP.Deserialize<MessageWrapper>(packet);
        _managersByByte[msg.Marker].HandleIncoming(msg.SubMarker, msg.Data);
    }

    public byte[] WrapUpdate(Update t)
    {
        return WrapMessage<Update>(t);
    }
    public byte[] WrapProcedure(Procedure t)
    {
        return WrapMessage<Procedure>(t);
    }
    public byte[] WrapCommand(Command t)
    {
        return WrapMessage<Command>(t);
    }
    private byte[] WrapMessage<T>(T t)
    {
        var typeManager = (MessageTypeManager<T>)_managersByType[typeof(T)];
        var wrapper = typeManager.WrapAsMessage(t, _markers[typeof(T)]);
        var bytes = Game.I.Serializer.MP.Serialize(wrapper);
        var wrapper2 = Game.I.Serializer.MP.Deserialize<MessageWrapper>(bytes);
        if (bytes.Length > short.MaxValue - 1)
        {
            if (t is EntityCreationUpdate u)
            {
                GD.Print(u.EntityTypeName);
            }
            throw new Exception($"Message size of {bytes.Length} for {t.GetType()} is too big");
        }
        return bytes;
    }
    private void AddType<T>(Action<T> handler)
    {
        var manager = new MessageTypeManager<T>(handler);
        var marker = Convert.ToByte(_managersByByte.Count);
        _managersByByte.Add(marker, manager);
        _managersByType.Add(typeof(T), manager);
        _markers.Add(typeof(T), marker);
    }
}
