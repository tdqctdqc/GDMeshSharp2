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

    public void Clear()
    {
        
    }
}
