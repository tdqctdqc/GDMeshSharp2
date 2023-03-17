
using System;
using System.Collections.Generic;

public class AggregateResult : IResult
{
    private List<IResult> _results;

    public AggregateResult(List<IResult> results)
    {
        _results = results;
    }

    public void Poll(Action<Procedure> addProc, Action<Decision> addDec, Action<Update> addUpdate)
    {
        _results.ForEach(r => r.Poll(addProc, addDec, addUpdate));
    }
}
