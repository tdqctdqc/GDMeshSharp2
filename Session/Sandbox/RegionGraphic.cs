using System.Collections.Generic;
using Godot;

public class RegionGraphic : Node2D
{
    private Dictionary<Vector2, Node2D> _regionGraphics;
    private IRegion<MockNode<Vector2>> _region;
    private IReadOnlyGraph<Vector2, LineSegment> _graph;

    public void Draw()
    {
        Clear();
    }

    public void Clear()
    {
        foreach (var kvp in _regionGraphics)
        {
            kvp.Value.QueueFree();
        }

        _regionGraphics = new Dictionary<Vector2, Node2D>();
    }
    
}