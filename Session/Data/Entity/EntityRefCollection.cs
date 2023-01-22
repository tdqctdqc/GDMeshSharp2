using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public class EntityRefCollection<TRef> : HashSet<int>, IEntityRefCollection where TRef : Entity
{
    public IReadOnlyCollection<TRef> Refs() => _refs;
    private List<int> _idList;
    private HashSet<TRef> _refs;
    
    public static EntityRefCollection<TRef> Construct(IEnumerable<int> refIds, CreateWriteKey key)
    {
        var col = new EntityRefCollection<TRef>();
        foreach (var refId in refIds)
        {
            col.AddRef(refId, key.Data);
        }
        col._refs = col.Select(id => (TRef) key.Data[id]).ToHashSet();
        return col;
    }
    public EntityRefCollection()
    {
        _refs = new HashSet<TRef>();
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
    
    public void SyncRefs(ServerWriteKey key)
    {
        _refs.Clear();
        foreach (var id in this)
        {
            TRef refer;
            try
            {
                refer = (TRef) key.Data[id];
                _refs.Add(refer);
            }
            catch (Exception e)
            {
                GD.Print(key.Data[id].GetType().ToString() + " supposed to be " + typeof(TRef).ToString());
                throw;
            }
        }
    }
}
