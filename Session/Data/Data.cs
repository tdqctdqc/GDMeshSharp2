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
        
        AddDomain(BaseDomain);
        AddDomain(Planet);
        AddDomain(Society);
    }
    
    public void AddEntity<TEntity>(TEntity e, StrongWriteKey key) where TEntity : Entity
    {
        if (Entities.ContainsKey(e.Id))
        {
            GD.Print($"trying to overwrite {Entities[e.Id].GetType().ToString()} with {e.GetType().ToString()}");
        }
        Entities.Add(e.Id, e);
        EntityTypeTree.Propagate(new EntityCreatedNotice(e));
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueUpdate(EntityCreationUpdate.Create(e, hKey));
        }
    }
    public void RemoveEntity<TEntity>(TEntity e, StrongWriteKey key) where TEntity : Entity
    {
        EntityTypeTree.Propagate(new EntityDestroyedNotice(e));
        Entities.Remove(e.Id);
        if (key is HostWriteKey hKey)
        {
            hKey.HostServer.QueueUpdate(EntityDeletionUpdate.Create(e.Id, hKey));
        }
    }

    public EntityRegister<T> GetRegister<T>() where T : Entity
    {
        var t = typeof(T);
        return _entityTypeDomainIndex[t].GetRegister<T>();
    }
    protected void AddDomain(Domain dom)
    {
        foreach (var domEntityType in dom.EntityTypes)
        {
            _entityTypeDomainIndex.Add(domEntityType, dom);
        }
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

    public void SubscribeForValueChange<TEntity, TProperty>(string fieldName, 
        Action<ValChangeNotice<TProperty>> callback)
    {
        EntityTypeTree[typeof(TEntity)].EntityValChanged.Subscribe(fieldName, callback);
    }
}
