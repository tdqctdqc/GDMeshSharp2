using Godot;
using System;
using System.Collections.Generic;

public class Data
{
    public IReadOnlyDictionary<Type, Domain> Domains => _domains;
    private Dictionary<Type, Domain> _domains;
    public Dictionary<int, Entity> Entities { get; private set; }
    public Dictionary<int, IRepo> EntityRepos { get; private set; }
    public Entity this[int id] => Entities[id];
    public BaseDomain BaseDomain => GetDomain<BaseDomain>();
    
    public Data()
    {
        Entities = new Dictionary<int, Entity>();
        EntityRepos = new Dictionary<int, IRepo>();
        _domains = new Dictionary<Type, Domain>();
        _domains.Add(typeof(BaseDomain), new BaseDomain(this));
    }
    
    public void AddEntity(Entity e, Type domainType, StrongWriteKey key)
    {
        Entities.Add(e.Id, e);
        var repo = _domains[domainType].Repos[e.GetType()];
        repo.AddEntity(e, key);
        EntityRepos.Add(e.Id, repo);
        if (key is HostWriteKey hKey)
        {
            var creationUpdate = EntityCreationUpdate.Encode(e, domainType, hKey);
            hKey.Server.QueueUpdate(creationUpdate);
        }
    }

    public void RemoveEntity(Entity e, StrongWriteKey key)
    {
        EntityRepos[e.Id].RemoveEntity(e, key);
        Entities.Remove(e.Id);
        EntityRepos.Remove(e.Id);
        if (key is HostWriteKey hKey)
        {
            var deletionUpdate = new EntityDeletionUpdate(e.Id);
            hKey.Server.QueueUpdate(deletionUpdate);
        }
    }
    public T GetDomain<T>() where T : Domain
    {
        return (T) _domains[typeof(T)];
    }

    protected void AddDomain(Domain dom)
    {
        _domains.Add(dom.GetType(), dom);
    }
    public Domain GetDomain(Type domainType)
    {
        return _domains[domainType];
    }

    public T GetEntity<T>(int id) where T : Entity
    {
        return (T) Entities[id];
    }
}
