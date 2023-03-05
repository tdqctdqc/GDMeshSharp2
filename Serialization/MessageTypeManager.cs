
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;


public class MessageTypeManager<T> : IMessageTypeManager
{
    private Dictionary<byte, Func<byte[], T>> _unwrappers;
    private Dictionary<Type, byte> _subTypeMarkers;
    private Action<T> _handle;

    
    public MessageTypeManager(Action<T> handle)
    {
        _handle = handle;
        _unwrappers = new Dictionary<byte, Func<byte[], T>>();
        _subTypeMarkers = new Dictionary<Type, byte>();
        var types = Assembly.GetExecutingAssembly().GetConcreteTypesOfType<T>().OrderBy(t => t.Name).ToList();
        if (types.Count > 254) throw new Exception();
        for (var i = 0; i < types.Count; i++)
        {
            var subType = types[i];
            var marker = Convert.ToByte(i);
            Func<byte[], T> unwrapper = bytes => (T) Game.I.Serializer.MP.Deserialize(bytes, subType);
            _subTypeMarkers.Add(subType, marker);
            _unwrappers.Add(marker, unwrapper);
        }
    }
    
    public void HandleIncoming(byte marker, byte[] data)
    {
        var t = _unwrappers[marker](data);
        _handle(t);
    }
    
    public MessageWrapper WrapAsMessage(T t, byte marker)
    {
        return new MessageWrapper(marker, _subTypeMarkers[t.GetType()], Game.I.Serializer.MP.Serialize(t, t.GetType()));
    }
}
