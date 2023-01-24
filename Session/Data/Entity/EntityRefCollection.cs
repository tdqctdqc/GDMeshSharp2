using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class EntityRefCollection<TRef> : HashSet<int>, IRef where TRef : Entity
{
    private HashSet<TRef> _refs;
    public static EntityRefCollection<TRef> Construct(IEnumerable<int> refIds, CreateWriteKey key)
    {
        var col = new EntityRefCollection<TRef>(refIds);
        col._refs = col.Select(id => (TRef) key.Data[id]).ToHashSet();
        return col;
    }
    public EntityRefCollection(IEnumerable<int> refIds)
    {
        foreach (var refId in refIds)
        {
            Add(refId);
        }
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
        return Contains(entity.Id);
    }
    public void AddRef(TRef t, Data data)
    {
        //todo need to make this procedure
        Add(t.Id);
        _refs.Add(t);
    }
    public void AddRef(int id, Data data)
    {
        //todo need to make this procedure
        Add(id);
        _refs.Add((TRef)data[id]);
    }
    public void RemoveRef(TRef t, Data data)
    {
        //todo need to make this procedure
        Remove(t.Id);
        _refs.Remove(t);
    }
    public void RemoveRef(int id, Data data)
    {
        //todo need to make this procedure
        Remove(id);
        _refs.Remove((TRef)data[id]);
    }
    public void Set(IEnumerable<int> ids, Data data, CreateWriteKey key)
    {
        //todo need to make this procedure
        Clear();
        foreach (var id in ids)
        {
            Add(id);
        }
        _refs = this.Select(id => (TRef) data[id]).ToHashSet();
    }
    public void Set(IEnumerable<TRef> refs, Data data, CreateWriteKey key)
    {
        //todo need to make this procedure
        Clear();
        foreach (var entity in refs)
        {
            if (data[entity.Id].GetType() != entity.GetType())
            {
                GD.Print($"{data[entity.Id].GetType()} should be {entity.GetType()}");
            }
            Add(entity.Id);
        }
        _refs = refs.ToHashSet();
    }
    
    public void SyncRef(Data data)
    {
        _refs.Clear();
        foreach (var id in this)
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
}
