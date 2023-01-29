using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepJobManager : IModelRepo<PeepJob>
{
    public static PeepJob Farmer = new PeepJob(nameof(Farmer));
    public Dictionary<string, PeepJob> Models { get; set; }

    public PeepJobManager()
    {
        Models = new Dictionary<string, PeepJob>();
        AddJob(Farmer);
    }

    private void AddJob(PeepJob job)
    {
        Models.Add(job.Name, job);
    }
}