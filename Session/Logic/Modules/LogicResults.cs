
using System.Collections.Generic;
using System.Linq;

public class LogicResults
{
    public List<Procedure> Procedures { get; private set; }
    public List<Decision> Decisions { get; private set; }
    public List<Update> Updates { get; private set; }

    public LogicResults(IEnumerable<IResult> results)
    {
        Updates = new List<Update>();
        Procedures = new List<Procedure>();
        Decisions = new List<Decision>();
        foreach (var r in results)
        {
            r.Poll(Procedures.Add, Decisions.Add, Updates.Add);
        }
    }
}
