using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

[RefAttribute] public class EntityRefCollection<TRef> : IRef<List<int>> where TRef : Entity
{
    public List<int> RefIds { get; private set; }
    private HashSet<TRef> _refs;
    public int Count() => RefIds.Count;
    public static EntityRefCollection<TRef> Construct(IEnumerable<int> refIds, CreateWriteKey key)
    {
        var col = new EntityRefCollection<TRef>(refIds);
        col._refs = col.RefIds.Select(id => (TRef) key.Data[id]).ToHashSet();
        return col;
    }
    public EntityRefCollection(IEnumerable<int> refIds)
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
            TRef refer;
            try
            {
                refer = (TRef) data[id];
                _refs.Add(refer);
            }
            catch (Exception e)
            {
                GD.Print(data[id].GetType().ToString() + " supposed to be " + typeof(TRef).ToString());
                throw;
            }
        }
    }

    public static EntityRefCollection<TRef> DeserializeConstructor(List<int> col)
    {
        return new EntityRefCollection<TRef>(col);
    }
    public List<int> GetUnderlying() => RefIds;
    public void Set(List<int> underlying, StrongWriteKey key)
    {
        RefIds = new List<int>(underlying);
    }
}
