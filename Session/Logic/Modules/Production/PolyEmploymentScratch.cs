using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolyEmploymentScratch
{
    public int PolyId { get; private set; }
    public Dictionary<PeepClass, Sub> ByClass { get; private set; }
    public Dictionary<PeepJob, int> ByJob { get; private set; }

    public PolyEmploymentScratch(MapPolygon poly, Data data)
    {
        ByClass = new Dictionary<PeepClass, Sub>();
        ByJob = new Dictionary<PeepJob, int>();
        Init(poly, data);
    }
    
    public void Init(MapPolygon poly, Data data)
    {
        var peep = poly.GetPeep(data);
        if (peep == null) throw new Exception();
        
        // foreach (var kvp in peep.ClassFragments)
        // {
        //     var peepClass = (PeepClass)data.Models[kvp.Key];
        //     if (ByClass.ContainsKey(peepClass) == false)
        //     {
        //         ByClass.Add(peepClass, new Sub());
        //     }
        // }
        // foreach (var kvp in ByClass)
        // {
        //     var sub = kvp.Value;
        //     var peepClass = kvp.Key;
        //     if (peep.ClassFragments.ContainsKey(peepClass.Name))
        //     {
        //         sub.Init(peep, kvp.Key);
        //     }
        //     else
        //     {
        //         sub.Clear();
        //     }
        // }
        ByClass = peep.ClassFragments.ToDictionary(kvp => (PeepClass) data.Models[kvp.Key],
            kvp =>
            {
                var s = new Sub();
                s.Init(peep, kvp.Value.PeepClass.Model());
                return s;
            });
        ByJob.Clear();
    }
    public void HandleClasses(WorkBuildingModel work, Data data)
    {
        foreach (var kvp in work.JobLaborReqs)
        {
            var peepClass = kvp.Key.PeepClass;
            var classSub = ByClass[peepClass];
            classSub.Desired += kvp.Value;
        }
    }
    public void HandleBuildingJobs(WorkBuildingModel work, Data data)
    {
        foreach (var jobReq in work.JobLaborReqs)
        {
            var attr = jobReq.Key;
            var size = jobReq.Value;
            var job = data.Models.PeepJobs.Models.First(kvp2 => kvp2.Value.Attributes.Has(attr)).Value;
            var jobClass = job.PeepClass;
            var classSub = ByClass[jobClass];
            var ratio = classSub.EffectiveRatio();
            var num = Mathf.CeilToInt(ratio * size);
            num = Mathf.Min(num, classSub.Available);
            ByJob.AddOrSum(job, num);
            classSub.Distribute(num);
        }
    }

    public void HandleJobNeed(PeepJob job, float contributeRatio, Data data)
    {
        var jobClass = job.PeepClass;
        if (ByClass.ContainsKey(jobClass) == false) return;
        var classSub = ByClass[jobClass];
        if (classSub.Total < 0) throw new Exception();
        var num = Mathf.CeilToInt(contributeRatio * classSub.Available);
        num = Mathf.Min(num, classSub.Available);
        if (num < 0)
        {
            throw new Exception();
        }
        ByJob.AddOrSum(job, num);
        classSub.Distribute(num);
    }
    public class Sub
    {
        public int Desired { get; set; }
        public int Total { get; private set; }
        public int Available { get; private set; }
        public Sub()
        {
            Desired = 0;
            Total = 0;
            Available = 0;
        }
        public void Init(PolyPeep peep, PeepClass peepClass)
        {
            Total = peep.ClassFragments[peepClass.Name].Size;
            Desired = 0;
            Available = Total;
        }
        public void Clear()
        {
            Desired = 0;
            Total = 0;
            Available = 0;
        }

        public float EffectiveRatio()
        {
            float r = 0f;
            if (Total == 0) r = 0f;
            else if (Desired == 0) r = 1f;
            else r = Mathf.Min(1f, ((float) Total) / ((float)Desired));
            if (r > 1f || r < 0f || float.IsNaN(r)) throw new Exception();

            // GD.Print($"Ratio {Total} / {Desired} to {r}");
            return r;
        }

        // public int Available()
        // {
        //     if (Total < 0) throw new Exception();
        //     if (Total == 0) return 0;
        //     if (Desired >= Total) return 0;
        //     var r = Total - Distributed;
        //     if (r < 0)
        //     {
        //         GD.Print(Total);
        //         GD.Print(Distributed);
        //         throw new Exception();
        //     }
        //     return r;
        // }
        public void Add(int num)
        {
            if (num < 0) throw new Exception();
            Total += num;
            Available += num;
        }
        public void Distribute(int num)
        {
            if (num < 0) throw new Exception();
            Available -= num;
            if (Available < 0) throw new Exception();
        }
    }
}
