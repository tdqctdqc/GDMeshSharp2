using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class EntityRefCollection<TRef> : HashSet<int>, IRef<HashSet<int>> where TRef : Entity
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

    public static EntityRefCollection<TRef> DeserializeConstructor(HashSet<int> col)
    {
        return new EntityRefCollection<TRef>(col);
    }
    HashSet<int> IRef<HashSet<int>>.GetUnderlying() => this;
}
