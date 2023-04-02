using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class JobAssignment
{
    public ModelRef<PeepJob> Job { get; private set; }
    public EntityRef<Peep> Peep { get; private set; }
    public int Count { get; private set; }
    public int Proficiency { get; private set; } //out of 100
    public JobAssignment(EntityRef<Peep> peep, ModelRef<PeepJob> job, 
        int count, int proficiency)
    {
        Peep = peep;
        Job = job;
        Count = count;
        Proficiency = proficiency;
    }

    public void ChangeCount(int delta, ProcedureWriteKey key)
    {
        Count += delta;
        if (Count < 0) throw new Exception();
    }
}
