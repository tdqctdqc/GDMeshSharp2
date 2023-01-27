using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Location : Entity
{
    protected Location(int id, CreateWriteKey key) : base(id, key)
    {
    }
    
    protected Location(object[] args, ServerWriteKey key) : base(args, key)
    {
    }
    
}