using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class Data
{
    private IdDispenser _idDispenser;
    public DataNotices Notices { get; private set; }
    public Models Models { get; private set; }
    public RefFulfiller RefFulfiller { get; private set; }
    public IReadOnlyDictionary<Type, Domain> Domains => _domains;
    private Dictionary<Type, Domain> _domains;
    private Dictionary<Type, Domain> _entityTypeDomainIndex;
    
    public Dictionary<int, Entity> Entities { get; private set; }
    public Entity this[int id] => Entities[id];
    public BaseDomain BaseDomain { get; private set; }
    public PlanetDomain Planet { get; private set; }
    public SocietyDomain Society { get; private set; }
    public EntityTypeTree EntityTypeTree { get; private set; }
    public int Tick => BaseDomain.GameClock.Tick;

    public Data()
    {
        _idDispenser = new IdDispenser();
    }

    public void Setup()
    {
        Init();
    }
    protected virtual void Init()
    {
        Notices = new DataNotices();
        RefFulfiller = new RefFulfiller(this);
        Models = new Models();
        Entities = new Dictionary<int, Entity>();
        _domains = new Dictionary<Type, Domain>();
        _entityTypeDomainIndex = new Dictionary<Type, Domain>();

        EntityTypeTree = new EntityTypeTree(Game.I.Serializer.ConcreteEntityTypes);
        
        BaseDomain = new BaseDomain(this);
        Planet = new PlanetDomain(this);
        Society = new SocietyDomain(this);
        RegisterDomain(BaseDomain);
        RegisterDomain(Planet);
        RegisterDomain(Society);
        AddDomain(BaseDomain);
        AddDomain(Planet);
        AddDomain(Society);
    }
    
    public void AddEntity(Entity e, StrongWriteKey key)
    {
        if (Entities.ContainsKey(e.Id))
        {
            GD.Print($"trying to overwrite {Entities[e.Id].GetType().ToString()} with {e.GetType().ToString()}");
        }
        Entities.Add(e.Id, e);
        e.GetEntityTypeTreeNode().PropagateCreation(e);
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueUpdate(EntityCreationUpdate.Create(e, hKey));
        }
    }
    public void AddEntities(IEnumerable<Entity> es, StrongWriteKey key) 
    {
        foreach (var e in es)
        {
            if (Entities.ContainsKey(e.Id))
            {
                throw new EntityTypeException($"trying to overwrite {Entities[e.Id].GetType().ToString()} " +
                                              $"with {e.GetType().ToString()}");
            }
            Entities.Add(e.Id, e);
        }
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueUpdate(EntitiesCreationUpdate.Create(es, hKey));
        }
        foreach (var e in es)
        {
            e.GetEntityTypeTreeNode().PropagateCreation(e);
        }
    }
    public void RemoveEntities(int[] entityIds, StrongWriteKey key)
    {
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueUpdate(EntitiesDeletionUpdate.Create(entityIds, hKey));
        }
        foreach (var eId in entityIds)
        {
            var e = Entities[eId];
            e.GetEntityTypeTreeNode().PropagateDestruction(e);
        }
        foreach (var eId in entityIds)
        {
            Entities.Remove(eId);
        }
    }
    public void RemoveEntity(int eId, StrongWriteKey key)
    {
        var e = Entities[eId];
        e.GetEntityTypeTreeNode().PropagateDestruction(e);
        Entities.Remove(eId);
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueUpdate(EntityDeletionUpdate.Create(eId, hKey));
        }
    }

    public EntityRegister<T> GetRegister<T>() where T : Entity
    {
        var t = typeof(T);
        return _entityTypeDomainIndex[t].GetRegister<T>();
    }

    private void RegisterDomain(Domain d)
    {
        foreach (var domEntityType in d.EntityTypes)
        {
            _entityTypeDomainIndex.Add(domEntityType, d);
        }
    }
    protected void AddDomain(Domain dom)
    {
        
        dom.Setup();
        _domains.Add(dom.GetType(), dom);
    }
    public void GetIdDispenser(CreateWriteKey key)
    {
        key.SetIdDispenser(_idDispenser);
    }
    public void SubscribeForCreation<TEntity>(Action<EntityCreatedNotice> callback) where TEntity : Entity
    {
        EntityTypeTree[typeof(TEntity)].Created.Subscribe(callback);
    }
    public void SubscribeForDestruction<TEntity>(Action<EntityDestroyedNotice> callback) where TEntity : Entity
    {
        EntityTypeTree[typeof(TEntity)].Destroyed.Subscribe(callback);
    }
}
