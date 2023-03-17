
using System;

public interface IResult
{
    void Poll(Action<Procedure> addProc, Action<Decision> addDec, Action<Update> addUpdate);
}
