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
    private VBoxContainer _container;
    private Data _data;

    private void Setup(Data data)
    {
        _data = data;
        this.AssignChildNode(ref _container, "VBoxContainer");
        Connect("about_to_show", this, nameof(Draw));
    }
    private void Draw()
    {
        while (_container.GetChildCount() > 0)
        {
            _container.RemoveChild(_container.GetChild(0));
        }

        var sw = new Stopwatch();
        sw.Start();

        foreach (var keyValuePair in _data.Entities)
        {
            var entity = keyValuePair.Value;
            var json = entity.GetMeta().GetPropertyValues(entity);
            if (entity is MapPolygon || entity is MapPolygonBorder) continue;
            // if (sw.ElapsedMilliseconds > 2000)
            // {
            //     break;
            // }
            var entityLabel = new Label();
            entityLabel.Text = entity.GetType().Name + " " + json;
            _container.AddChild(entityLabel);
        }
        sw.Stop();
    }
}
