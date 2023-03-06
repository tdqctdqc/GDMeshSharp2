
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MessageManager
{
    private Dictionary<byte, IMessageTypeManager> _managersByByte;
    private Dictionary<Type, IMessageTypeManager> _managersByType;
    private Dictionary<Type, byte> _markers;
    private PacketProtocol _protocol;
    public MessageManager(Action<Update> handleUpdate, 
        Action<Procedure> handleProcedure, 
        Action<Command> handleCommand,
        Action<Decision> handleDecision)
    {
        _managersByByte = new Dictionary<byte, IMessageTypeManager>();
        _managersByType = new Dictionary<Type, IMessageTypeManager>();
        _markers = new Dictionary<Type, byte>();
        AddType<Update>(handleUpdate);
        AddType<Procedure>(handleProcedure);
        AddType<Command>(handleCommand);
        AddType<Decision>(handleDecision);
    }
    public void HandleIncoming(byte[] packet, Guid fromGuid)
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

    public byte[] WrapDecision(Decision d)
    {
        return WrapMessage<Decision>(d);
    }
    private byte[] WrapMessage<T>(T t)
    {
        var typeManager = (MessageTypeManager<T>)_managersByType[typeof(T)];
        var wrapper = typeManager.WrapAsMessage(t, _markers[typeof(T)]);
        var bytes = Game.I.Serializer.MP.Serialize(wrapper);
        if (bytes.Length > short.MaxValue - 1)
        {
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
