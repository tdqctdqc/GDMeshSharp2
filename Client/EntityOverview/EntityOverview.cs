using Godot;
using System;

public class EntityOverview : WindowDialog
{
    public static EntityOverview Get(Data data) 
    {
        var overview = (EntityOverview) ((PackedScene) GD.Load("res://Client/EntityOverview/EntityOverview.tscn"))
            .Instance();
        overview.Setup(data);
        return overview;
    }
    private VBoxContainer _container;
    private Data _data;

    private void Setup(Data data)
    {
        _data = data;
        _container = (VBoxContainer)FindNode("VBoxContainer");
        Connect("about_to_show", this, nameof(Draw));
    }
    private void Draw()
    {
        while (_container.GetChildCount() > 0)
        {
            _container.RemoveChild(_container.GetChild(0));
        }

        foreach (var keyValuePair in _data.Entities)
        {
            var entity = keyValuePair.Value;
            var entityLabel = new Label();
            entityLabel.Text = entity.GetType().Name + " " + entity.GetMeta().Serialize(entity);
            _container.AddChild(entityLabel);
        }
    }
}
