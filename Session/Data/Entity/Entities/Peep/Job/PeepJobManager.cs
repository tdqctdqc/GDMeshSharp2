using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepJobManager : IModelManager<PeepJob>
{
    public static PeepJob Farmer = new PeepJob(nameof(Farmer));
    public static PeepJob Laborer = new PeepJob(nameof(Laborer));
    public Dictionary<string, PeepJob> Models { get; set; }

    public PeepJobManager()
    {
        Models = new Dictionary<string, PeepJob>();
        AddJobs(new PeepJob[]
        {
            Farmer,
            Laborer
        });
    }

    private void AddJobs(params PeepJob[] jobs)
    {
        foreach (var job in jobs)
        {
            Models.Add(job.Name, job);
        }
    }
}