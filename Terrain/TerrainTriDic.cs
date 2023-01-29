using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TerrainTriDic 
{
    public Dictionary<int, List<Triangle>> Value { get; private set; }
    public TerrainTriDic(Dictionary<int, List<Triangle>> value)
    {
        Value = value;
    }
}