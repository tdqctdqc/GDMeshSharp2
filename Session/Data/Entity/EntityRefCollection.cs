using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

[MessagePackObject(keyAsPropertyName: true)] 
public partial class EntityRefCollection<TRef>
    : IRef<List<int>> where TRef : Entity
{
    public List<int> RefIds { get; private set; }
    private HashSet<TRef> _refs;
    public int Count() => RefIds.Count;
    public static EntityRefCollection<TRef> Construct(List<int> refIds, CreateWriteKey key)
    {
        var col = new EntityRefCollection<TRef>(refIds);
        col._refs = col.RefIds.Select(id => (TRef) key.Data[id]).ToHashSet();
        return col;
    }
    public EntityRefCollection(List<int> refIds)
    {
        RefIds = new List<int>(refIds);
        _refs = new HashSet<TRef>();
    }

    public IReadOnlyCollection<TRef> Refs()
    {
        if (_refs == null)
        {
            Game.I.RefFulfiller.Fulfill(this);
        }
        return _refs;
    }

    public bool Contains(TRef entity)
    {
        return RefIds.Contains(entity.Id);
    }
    public void AddRef(TRef t, Data data)
    {
        //todo need to make this procedure
        RefIds.Add(t.Id);
        _refs.Add(t);
    }
    public void AddRef(int id, Data data)
    {
        //todo need to make this procedure
        RefIds.Add(id);
        _refs.Add((TRef)data[id]);
    }
    public void RemoveRef(TRef t, Data data)
    {
        //todo need to make this procedure
        RefIds.Remove(t.Id);
        _refs.Remove(t);
    }
    public void RemoveRef(int id, Data data)
    {
        //todo need to make this procedure
        RefIds.Remove(id);
        _refs.Remove((TRef)data[id]);
    }
    
    public void SyncRef(Data data)
    {
        _refs.Clear();
        foreach (var id in RefIds)
        {
            TRef refer = (TRef) data[id];
            _refs.Add(refer);
        }
    }
    public List<int> GetUnderlying() => RefIds;
    public void Set(List<int> underlying, StrongWriteKey key)
    {
        RefIds = new List<int>(underlying);
    }
}
