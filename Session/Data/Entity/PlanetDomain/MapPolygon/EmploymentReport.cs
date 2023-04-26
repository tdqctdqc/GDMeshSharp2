using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EmploymentReport
{
    public Dictionary<string, int> Counts { get; private set; }
    public static EmploymentReport Construct()
    {
        return new EmploymentReport(new Dictionary<string, int>());
    }
    [SerializationConstructor] private EmploymentReport(Dictionary<string, int> counts)
    {
        Counts = new Dictionary<string, int>();
    }

    public void Copy(EmploymentReport toCopy, ProcedureWriteKey key)
    {
        Counts.Clear();
        Counts.AddRange(toCopy.Counts);
    }

    public int NumUnemployed()
    {
        if (Counts.ContainsKey(PeepJobManager.Unemployed.Name) == false) return 0;
        return Counts[PeepJobManager.Unemployed.Name];
    }
    public void Clear()
    {
        
    }
}
