using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class GenWriteKey : CreateWriteKey
{
    public WorldData WorldData => (WorldData) Data;
    public GenWriteKey(WorldData data) : base(data)
    {
    }
}