using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class PeepJobManager : IModelManager<PeepJob>
{
    public static PeepJob Farmer { get; private set; } 
            = new PeepJob(nameof(Farmer), new FarmerAttribute());
    public static PeepJob Laborer { get; private set; } 
        = new PeepJob(nameof(Laborer), new LaborerAttribute());
    public static PeepJob Miner { get; private set; } 
        = new PeepJob(nameof(Miner), new MinerAttribute());
    public static PeepJob Bureaucrat { get; private set; } 
        = new PeepJob(nameof(Bureaucrat), new BureaucratAttribute());
    public static PeepJob Unemployed { get; private set; } 
        = new PeepJob(nameof(Unemployed));
    public Dictionary<string, PeepJob> Models { get; set; }

    public PeepJobManager()
    {
        Models = new Dictionary<string, PeepJob>();
        var jobs = typeof(PeepJobManager).GetProperties(BindingFlags.Static | BindingFlags.Public)
            .Where(p => typeof(PeepJob).IsAssignableFrom(p.PropertyType));
        AddJobs(jobs.Select(p => (PeepJob) p.GetValue(null)).ToArray());
    }

    private void AddJobs(params PeepJob[] jobs)
    {
        foreach (var job in jobs)
        {
            Models.Add(job.Name, job);
        }
    }
}