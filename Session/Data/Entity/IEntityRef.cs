using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IEntityRef
{
    void SyncRef(ServerWriteKey key);
}