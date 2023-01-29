using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RequestMoreBytesCommand : Command
{
    public int AvailableBytes { get; private set; }

    public RequestMoreBytesCommand(int availableBytes, WriteKey key) : base(key)
    {
        AvailableBytes = availableBytes;
    }



    public override void Enact(HostWriteKey key)
    {
        throw new NotImplementedException();
    }
}