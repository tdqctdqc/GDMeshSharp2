using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class PeepJobManager : IModelManager<PeepJob>
{
    public static PeepJob Farmer { get; private set; } 
            = new PeepJob(nameof(Farmer), PeepClassManager.Laborer, new FarmerAttribute());
    public static PeepJob Prole { get; private set; } 
        = new PeepJob(nameof(Prole), PeepClassManager.Laborer, new ProleAttribute());
    public static PeepJob Miner { get; private set; } 
        = new PeepJob(nameof(Miner), PeepClassManager.Laborer, new MinerAttribute());
    public static PeepJob Bureaucrat { get; private set; } 
        = new PeepJob(nameof(Bureaucrat), PeepClassManager.Professional, new BureaucratAttribute());
    public static PeepJob Builder { get; private set; } 
        = new PeepJob(nameof(Builder), PeepClassManager.Laborer, new ConstructionAttribute());
    public static PeepJob Unemployed { get; private set; } 
        = new PeepJob(nameof(Unemployed), PeepClassManager.Laborer);
    public Dictionary<string, PeepJob> Models { get; set; }

    public PeepJobManager()
    {
        Models = GetType().GetStaticPropertiesOfType<PeepJob>().ToDictionary(pj => pj.Name, pj => pj);
    }

}