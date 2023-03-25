using System;
using System.Collections.Generic;
using System.Linq;

public class GhostPeep : Peep
{
    public string Boo { get; protected set; }

    public static GhostPeep Create(MapPolygon home, PeepJob job, string boo,
        CreateWriteKey key)
    {
        var g = new GhostPeep(key.IdDispenser.GetID(), home.MakeRef(), job.MakeRef(), boo);
        key.Create(g);
        return g;
    }
    protected GhostPeep(int id, EntityRef<MapPolygon> home, ModelRef<PeepJob> job, string boo) : base(id, home, job)
    {
        Boo = boo;
    }
}
