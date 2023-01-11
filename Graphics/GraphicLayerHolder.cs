using Godot;
using System;
using System.Collections.Generic;

public class GraphicLayerHolder : Control
{
    private Dictionary<string, Node2D> _layers;
    private Dictionary<string, ButtonToken> _tokens;
    private Container _container;

    public void Setup()
    {
        _container = (Container) FindNode("Container");
        _tokens = new Dictionary<string, ButtonToken>();
        _layers = new Dictionary<string, Node2D>();
    }

    public Node2D GenerateLayer<T>(List<T> elements, Func<T, Node2D> getNode, string name)
    {
        var graphics = new Node2D();
        for (var i = 0; i < elements.Count; i++)
        {
            var el = elements[i];
            var graphic = getNode(el);
            graphics.AddChild(graphic);
        }
        AddLayer(graphics, name);
        return graphics;
    }

    public void Clear()
    {
        foreach (var keyValuePair in _tokens)
        {
            keyValuePair.Value.Free();
        }
        _tokens.Clear();

        while (_container.GetChildCount() > 0)
        {
            _container.GetChild(0).Free();
        }

        _layers = new Dictionary<string, Node2D>();
    }

    public void AddLayer(Node2D layer, string name)
    {
        if (_layers.Count > 0) layer.Visible = false;
        _layers.Add(name, layer);
        var button = new Button();
        button.Text = layer.Visible
            ? "Selected " + name
            : "Turn on " + name;
        _container.AddChild(button);
        var token = new ButtonToken();
        token.Setup(button, () => ToggleLayer(name));
        _tokens.Add(name, token);
    }
    private void ToggleLayer(string layerName)
    {
        foreach (var keyValuePair in _layers)
        {
            var layer = keyValuePair.Value;
            var name = keyValuePair.Key;
            if(name == layerName)
                layer.Visible = true;
            else
                layer.Visible = false;
            
            _tokens[name].Button.Text = layer.Visible
                ? "Selected " + name
                : "Turn on " + name;
        }
    }
    
}
