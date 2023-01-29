using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepJob : IModel
{
    public string Name { get; private set; }

    public PeepJob(string name)
    {
        Name = name;
    }
}