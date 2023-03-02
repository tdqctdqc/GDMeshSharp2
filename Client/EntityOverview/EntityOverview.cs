using Godot;
using System;
using System.Diagnostics;
using System.Linq;

public class EntityOverview : WindowDialog
{
    public static EntityOverview Get(Data data)
    {
        var eo = SceneManager.Instance<EntityOverview>();
        eo.Setup(data);
        return eo;
    }
    private UIVar<Domain> _domain;
    private UIVar<Type> _entityType;
    private UIVar<Entity> _selectedEntity;
    

    private ItemList _domainContainer, _entityTypeContainer, _entitiesContainer, _entityPropsContainer;
    private ItemListToken _domainToken, _entityTypeToken, _entitiesToken, _entityPropsToken;
    private Data _data;

    private void Setup(Data data)
    {
        _data = data;
        this.AssignChildNode(ref _domainContainer, "Domains");
        _domainToken = ItemListToken.Construct(_domainContainer);
        
        _domain = new UIVar<Domain>(null);
        _domain.ChangedValue += DrawEntityTypes;
        
        _entityType = new UIVar<Type>(null);
        _entityType.ChangedValue += DrawEntitiesOfType;
        
        _selectedEntity = new UIVar<Entity>(null);
        _selectedEntity.ChangedValue += DrawEntityProps;
        
        this.AssignChildNode(ref _entityTypeContainer, "EntityTypes");
        _entityTypeToken = ItemListToken.Construct(_entityTypeContainer);
        
        this.AssignChildNode(ref _entitiesContainer, "Entities");
        _entitiesToken = ItemListToken.Construct(_entitiesContainer);
        
        this.AssignChildNode(ref _entityPropsContainer, "EntityProps");
        _entityPropsToken = ItemListToken.Construct(_entityPropsContainer);
        
        Connect("about_to_show", this, nameof(Draw));
    }
    private void Draw()
    {
        GD.Print("drawing");
        _domainToken.Setup(_data.Domains.Values.ToList(), 
            d => d.GetType().Name,
            d => () => _domain.SetValue(d));
    }

    private void DrawEntityTypes(Domain d)
    {
        _entityTypeToken.Setup(d.Repos.Select(r => r.Value.EntityType).ToList(),
            t => t.Name,
            t => () => _entityType.SetValue(t));
    }

    private void DrawEntitiesOfType(Type type)
    {
        var d = _domain.Value;
        var repo = d.GetRepo(type);
        
        _entitiesToken.Setup(repo.Entities.ToList(), 
            e => e.Id.ToString(),
            e => () => _selectedEntity.SetValue(e));
    }
    private void DrawEntityProps(Entity e)
    {
        var meta = e.GetMeta();
        var vals = meta.GetPropertyValues(e);
        _entityPropsToken.Setup<int>(
            Enumerable.Range(0, e.GetMeta().FieldNames.Count).ToList(),
            i => meta.FieldNames[i] + ": " + vals[i].ToString(),
            i => () => { }
        );
    }
}
