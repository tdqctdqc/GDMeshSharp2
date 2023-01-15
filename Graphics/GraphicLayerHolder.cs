using Godot;
using System;
using System.Collections.Generic;

public class GraphicLayerHolder : Control
{
    private Dictionary<string, GraphicView> _views;
    private Dictionary<string, ButtonToken> _viewTokens;
    private Container _viewButtonsContainer;
    private Container _overlayButtonsContainer;

    public void Setup()
    {
        _viewButtonsContainer = (Container) FindNode("ViewButtons");
        _overlayButtonsContainer = (Container) FindNode("OverlayButtons");
        _viewTokens = new Dictionary<string, ButtonToken>();
        _views = new Dictionary<string, GraphicView>();
    }

    public Node2D GenerateView<T>(List<T> elements, Func<T, Node2D> getNode, string name)
    {
        var graphics = new Node2D();
        for (var i = 0; i < elements.Count; i++)
        {
            var el = elements[i];
            var graphic = getNode(el);
            graphics.AddChild(graphic);
        }
        AddView(graphics, name);
        return graphics;
    }

    public void Clear()
    {
        foreach (var keyValuePair in _views)
        {
            keyValuePair.Value.Clear();
        }
        foreach (var keyValuePair in _viewTokens)
        {
            keyValuePair.Value.Free();
        }
        _viewTokens.Clear();
        _views.Clear();

        while (_viewButtonsContainer.GetChildCount() > 0)
        {
            _viewButtonsContainer.GetChild(0).Free();
        }
    }

    public void AddView(Node2D layer, string name)
    {
        var view = new GraphicView(layer);
        bool vis = true;
        if (_views.Count > 0) vis = false;
            
        view.Toggle(vis);
        _views.Add(name, view);
        var button = new Button();
        button.Text = layer.Visible
            ? "Selected " + name
            : "Turn on " + name;
        _viewButtonsContainer.AddChild(button);
        var token = new ButtonToken();
        token.Setup(button, () => _views[name].Toggle(vis));
        _viewTokens.Add(name, token);
    }

    public void AddOverlay(string viewName, string overlayName, Node2D overlay)
    {
        _views[viewName].AddOverlay(overlay, overlayName, _overlayButtonsContainer);
    }
    private void ToggleLayer(string activeViewName)
    {
        foreach (var keyValuePair in _views)
        {
            var layer = keyValuePair.Value;
            var name = keyValuePair.Key;
            var vis = name == activeViewName;
            _views[name].Toggle(vis);
            _viewTokens[name].Button.Text = vis
                ? "Selected " + name
                : "Turn on " + name;
        }
    }
}
