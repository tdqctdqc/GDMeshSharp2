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
    public Dictionary<PeepJob, JobAssignment> Jobs { get; private set; }
    public static Peep Create(MapPolygon home, int size, CreateWriteKey key)
    {
        var id = key.IdDispenser.GetID();
        var unemployedAssignment = new JobAssignment(
            new EntityRef<Peep>(id),
            PeepJobManager.Unemployed.MakeRef(), 
            size, 100);
        var jobs = new Dictionary<PeepJob, JobAssignment>
        {
            {PeepJobManager.Unemployed, unemployedAssignment}
        };
        var p = new Peep(id, home.MakeRef(), size, jobs);
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private Peep(int id, EntityRef<MapPolygon> home,
        int size, Dictionary<PeepJob, JobAssignment> jobs) : base(id)
    {
        if (size <= 0) throw new Exception();
        Home = home;
        Size = size;
        Jobs = jobs;
    }

    public void AddJobAssignment(JobAssignment ja, ProcedureWriteKey key)
    {
        Jobs.Add(ja.Job.Model(), ja);
        key.Data.Society.BuildingAux.LaborersDelta
            .Invoke(Home.Entity());
        Jobs[PeepJobManager.Unemployed].ChangeCount(-ja.Count, key);
    }

    public void GrowSize(int delta, ProcedureWriteKey key)
    {
        if (delta <= 0) throw new Exception();
        Size += delta;
        Jobs[PeepJobManager.Unemployed].ChangeCount(delta, key);
    }
    
    private void RemoveJobAssignment(PeepJob job, ProcedureWriteKey key)
    {
        Jobs.Remove(job);
    }
    public void ChangeJobAssignmentCount(PeepJob job, int delta, ProcedureWriteKey key)
    {
        var ja = Jobs[job];
        ja.ChangeCount(delta, key);
        if (ja.Count < 0) throw new Exception();
        Jobs[PeepJobManager.Unemployed].ChangeCount(-delta, key);
        if (ja.Count == 0 && ja.Job.Model() != PeepJobManager.Unemployed)
        {
            RemoveJobAssignment(ja.Job.Model(), key);
        }
        key.Data.Society.BuildingAux.LaborersDelta
            .Invoke(Home.Entity());
    }
}