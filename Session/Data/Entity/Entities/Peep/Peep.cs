using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Peep : Entity
{
    public int Size { get; private set; }
    public EntityRef<MapPolygon> Home { get; private set; }
    public ModelRef<PeepJob> Job { get; private set; }

    public Peep(int id, int size, EntityRef<MapPolygon> home, 
        ModelRef<PeepJob> job, CreateWriteKey key) : base(id, key)
    {
        Size = size;
        Home = home;
        Job = job;
    }

    [SerializationConstructor] public Peep(int id, int size, EntityRef<MapPolygon> home, 
        ModelRef<PeepJob> job) : base(id)
    {
        Size = size;
        Home = home;
        Job = job;
    }
}