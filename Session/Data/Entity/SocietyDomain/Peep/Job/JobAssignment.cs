using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class JobAssignment
{
    public byte Marker { get; private set; }
    public ModelRef<PeepJob> Job { get; private set; }
    public EntityRef<Building> Building { get; private set; }
    public EntityRef<Peep> Peep { get; private set; }
    public int Count { get; private set; }
    public int Proficiency { get; private set; } //out of 100
    public bool Unemployed() => Building.Empty();
    public JobAssignment(EntityRef<Peep> peep, byte marker, ModelRef<PeepJob> job, EntityRef<Building> building, 
        int count, int proficiency)
    {
        Peep = peep;
        Marker = marker;
        Job = job;
        Building = building;
        Count = count;
        Proficiency = proficiency;
    }

    public void ChangeCount(int delta, ProcedureWriteKey key)
    {
        Count += delta;
        if (Count < 0) throw new Exception();
    }

    public void SetMarker(byte marker, ProcedureWriteKey key)
    {
        if (marker == 0) throw new Exception();
        Marker = marker;
    }
}
