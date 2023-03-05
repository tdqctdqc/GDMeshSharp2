
using System.Collections.Generic;
using System.Linq;

public class LogicResult
{
    public List<Procedure> Procedures { get; private set; }
    public List<Decision> Decisions { get; private set; }
    public List<Update> Updates { get; private set; }

    public LogicResult(List<Procedure> procedures, List<Decision> decisions, List<Update> updates)
    {
        Updates = updates;
        Procedures = procedures;
        Decisions = decisions;
    }

    public LogicResult(IEnumerable<LogicResult> results)
    {
        Procedures = results.SelectMany(r => r.Procedures).ToList();
        Decisions = results.SelectMany(r => r.Decisions).ToList();
        Updates = results.SelectMany(r => r.Updates).ToList();
    }
}
