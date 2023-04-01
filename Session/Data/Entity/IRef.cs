using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IRef
{
    void SyncRef(Data data);
    void ClearRef();
}


public interface IRefCollection : IRef
{
    void AddByProcedure(List<int> ids, ProcedureWriteKey key);
    void RemoveByProcedure(List<int> ids, ProcedureWriteKey key);
}


