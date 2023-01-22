using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IEntityRefCollection
{
    void SyncRefs(ServerWriteKey key);
}