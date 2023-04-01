using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class EntityRefCollection<TRef>
    : IRefCollection, IReadOnlyHash<TRef> where TRef : Entity
{
    public HashSet<int> RefIds { get; private set; }
    private Dictionary<int, TRef> _refs;
    public int Count() => RefIds.Count;
    public static EntityRefCollection<TRef> Construct(HashSet<int> refIds, Data data)
    {
        var col = new EntityRefCollection<TRef>(refIds);
        col._refs = col.RefIds.ToDictionary(id => id, id => (TRef) data[id]);
        return col;
    }
    [SerializationConstructor] private EntityRefCollection(HashSet<int> refIds)
    {
        RefIds = refIds == null ? new HashSet<int>() : new HashSet<int>(refIds);
        _refs = null;
    }
    public IReadOnlyCollection<TRef> Entities()
    {
        if (_refs == null)
        {
            Game.I.RefFulfiller.Fulfill(this);
        }
        return _refs.Values;
    }

    public bool Contains(TRef entity)
    {
        return RefIds.Contains(entity.Id);
    }
    public void AddRef(TRef t, GenWriteKey key)
    {
        if (RefIds.Contains(t.Id)) return;
        RefIds.Add(t.Id);
        _refs?.Add(t.Id, t);
    }
    public void RemoveRef(TRef t, GenWriteKey key)
    {
        RefIds.Remove(t.Id);
        _refs?.Remove(t.Id);
    }
    
    public void SyncRef(Data data)
    {
        _refs = new Dictionary<int, TRef>();
        foreach (var id in RefIds)
        {
            TRef refer = (TRef) data[id];
            _refs.Add(id, refer);
        }
    }

    public void ClearRef()
    {
        RefIds.Clear();
        _refs.Clear();
    }

    public void AddByProcedure(List<int> ids, ProcedureWriteKey key)
    {
        RefIds.AddRange(ids);
    }

    public void RemoveByProcedure(List<int> ids, ProcedureWriteKey key)
    {
        ids.ForEach(id => RefIds.Remove(id));
    }

    public IEnumerator<TRef> GetEnumerator()
    {
        if (_refs == null)
        {
            Game.I.RefFulfiller.Fulfill(this);
        }
        return _refs.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    int IReadOnlyCollection<TRef>.Count => RefIds.Count;
}
