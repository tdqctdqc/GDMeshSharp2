
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public class Syncer
{
    protected PacketPeerStream _packetStream;
    protected PacketProtocol _protocol;
    protected MessageManager _msg;
    protected bool _listening;
    private Stopwatch _sw;

    public Syncer(PacketPeerStream packetStream, Action<Update> handleUpdate,
        Action<Procedure> handleProcedure, Action<Command> handleCommand)
    {
        _sw = new Stopwatch();
        _packetStream = packetStream;
        _msg = new MessageManager(handleUpdate, handleProcedure, handleCommand);
        _protocol = new PacketProtocol(0);
        _protocol.MessageArrived += _msg.HandleIncoming;
        Task.Run(Listen);
    }
    public Syncer(PacketPeerStream packetStream, MessageManager msg)
    {
        _sw = new Stopwatch();
        _packetStream = packetStream;
        _msg = msg;
        _protocol = new PacketProtocol(0);
        _protocol.MessageArrived += _msg.HandleIncoming;
        Task.Run(Listen);
    }

    private int _packetsReceived = 0;
    private async void Listen()
    {
        _listening = true;
        while (true)
        {
            if (_listening)
            {
                var count = _packetStream.GetAvailablePacketCount();
                if (count > 0)
                {
                    for (var i = 0; i < count; i++)
                    {
                        _protocol.DataReceived(_packetStream.GetPacket());
                        _packetsReceived++;
                    }
                }
            }
        }
    }
    protected void PushPacket(byte[] bytes)
    {
        if (bytes.Length > short.MaxValue - 1) throw new Exception($"packet too big at {bytes.Length} bytes");

        var lengthBytes = BitConverter.GetBytes(Convert.ToInt16(bytes.Length));
        
        var e1 = _packetStream.PutPacket(lengthBytes);
        if (e1 != Error.Ok)
        {
            GD.Print(e1);
            throw new Exception();
        }
        var e2 = _packetStream.PutPacket(bytes);
        if (e2 != Error.Ok)
        {
            GD.Print(e2);
            throw new Exception();
        }
    }
}