using Godot;
using System;

public class EntityOverview : WindowDialog
{
    public static EntityOverview Get() 
        => (EntityOverview)((PackedScene)GD.Load("res://Session/Client/EntityOverview/EntityOverview.tscn")).Instance();
    private VBoxContainer _container; 
    public override void _Ready()
    {
        _container = (VBoxContainer)FindNode("VBoxContainer");
        Connect("about_to_show", this, nameof(Setup));
    }
    private void Setup()
    {
        while (_container.GetChildCount() > 0)
        {
            _container.RemoveChild(_container.GetChild(0));
        }

        foreach (var keyValuePair in Game.I.Session.Data.Entities)
        {
            var entity = keyValuePair.Value;
            var entityLabel = new Label();
            entityLabel.Text = entity.GetType().Name + " " + entity.GetMeta().Serialize(entity);
            _container.AddChild(entityLabel);
        }
    }
}
