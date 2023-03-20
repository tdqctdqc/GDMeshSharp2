using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class EntityOverviewWindow : WindowDialog
{
    public static EntityOverviewWindow Get(Data data)
    {
        var eo = SceneManager.Instance<EntityOverviewWindow>();
        eo.Setup(data);
        return eo;
    }
    private UIVar<Domain> _domain;
    private UIVar<Type> _entityType;
    private UIVar<Entity> _selectedEntity;

    private ItemListToken _domainToken, _entityTypeToken, _entityTypePropsToken, 
        _entitiesToken, _entityPropsToken;
    private Data _data;

    private void Setup(Data data)
    {
        _data = data;
        
        _domain = new UIVar<Domain>(null);
        _domain.ChangedValue += DrawEntityTypes;
        
        _entityType = new UIVar<Type>(null);
        _entityType.ChangedValue += DrawEntitiesOfType;
        
        _selectedEntity = new UIVar<Entity>(null);
        _selectedEntity.ChangedValue += DrawEntityProps;
        
        _domainToken = ItemListToken.Construct((ItemList) FindNode("Domains"));
        _entityTypeToken = ItemListToken.Construct((ItemList) FindNode("EntityTypes"));
        _entitiesToken = ItemListToken.Construct((ItemList) FindNode("Entities"));
        _entityPropsToken = ItemListToken.Construct((ItemList) FindNode("EntityProps"));
        _entityTypePropsToken = ItemListToken.Construct((ItemList) FindNode("EntityTypeProps"));
        
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
        
        _entityTypePropsToken.Setup<string>(
            new List<string>
            {
                "Number: " + repo.Entities.Count
            },
            i => i,
            i => () => { }
        );
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
