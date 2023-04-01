using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;


public class Peep : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public EntityRef<MapPolygon> Home { get; protected set; }
    public int Size { get; private set; }
    public Dictionary<byte, JobAssignment> Jobs { get; private set; }
    public static Peep Create(MapPolygon home, int size, CreateWriteKey key)
    {
        var id = key.IdDispenser.GetID();
        var unemployedAssignment = new JobAssignment(
            new EntityRef<Peep>(id),
            0, 
            PeepJobManager.Unemployed.MakeRef(), 
            new EntityRef<Building>(-1),
            size, 100);
        var jobs = new Dictionary<byte, JobAssignment>
        {
            {0, unemployedAssignment}
        };
        var p = new Peep(id, home.MakeRef(), size, jobs);
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(int id, EntityRef<MapPolygon> home,
        int size, Dictionary<byte, JobAssignment> jobs) : base(id)
    {
        if (size <= 0) throw new Exception();
        Home = home;
        Size = size;
        Jobs = jobs;
    }

    public void AddJobAssignment(JobAssignment ja, ProcedureWriteKey key)
    {
        SetJobAssgnMarker(ja, key);
        Jobs.Add(ja.Marker, ja);
        key.Data.Society.BuildingAux.LaborersDelta
            .Invoke(new Tuple<Building, int>(ja.Building.Entity(), ja.Count));
        Jobs[0].ChangeCount(-ja.Count, key);
    }

    public void GrowSize(int delta, ProcedureWriteKey key)
    {
        if (delta <= 0) throw new Exception();
        Size += delta;
        Jobs[0].ChangeCount(delta, key);
    }
    private void SetJobAssgnMarker(JobAssignment ja, ProcedureWriteKey key)
    {
        if (ja.Marker != 255) throw new Exception();
        for (byte i = 1; i <= byte.MaxValue; i++)
        {
            if (Jobs.ContainsKey(i) == false)
            {
                ja.SetMarker(i, key);
                break;
            }

            if (i == byte.MaxValue) throw new Exception("too many job assignments");
        }
    }
    private void RemoveJobAssignment(byte marker, ProcedureWriteKey key)
    {
        Jobs.Remove(marker);
    }
    public void ChangeJobAssignmentCount(byte marker, int delta, ProcedureWriteKey key)
    {
        if (marker == 0) throw new Exception();
        var ja = Jobs[marker];
        ja.ChangeCount(delta, key);
        Jobs[0].ChangeCount(-delta, key);

        if (ja.Count < 0) throw new Exception();
        if (ja.Count == 0 && ja.Marker != 0)
        {
            RemoveJobAssignment(marker, key);
        }
        key.Data.Society.BuildingAux.LaborersDelta
            .Invoke(new Tuple<Building, int>(ja.Building.Entity(), -ja.Count));
    }
}