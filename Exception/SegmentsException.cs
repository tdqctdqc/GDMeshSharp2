using System.Collections.Generic;
using System.Linq;
using Godot;

public class SegmentsException : DisplayableException
{
    public string Message { get; private set; }
    public List<List<LineSegment>> SegLayers { get; private set; }
    public List<string> SegLayerNames { get; private set; }
    public List<List<Vector2>> PointSets { get; private set; }
    public List<string> PointSetNames { get; private set; }

    public SegmentsException(string message)
    {
        Message = message;
        SegLayers = new List<List<LineSegment>>();
        SegLayerNames = new List<string>();
        PointSets = new List<List<Vector2>>();
        PointSetNames = new List<string>();
    }

    public void AddSegLayer(List<LineSegment> lines, string name)
    {
        SegLayers.Add(lines);
        SegLayerNames.Add(name);
    }

    public void AddPointSet(List<Vector2> pointSet, string name)
    {
        PointSets.Add(pointSet);
        PointSetNames.Add(name);
    }
    
    public override Node2D GetGraphic()
    {
        var d = SceneManager.Instance<SegmentsExceptionDisplay>();
        d.Setup(this);
        return d;
    }

    public override Control GetUi()
    {
        throw new System.NotImplementedException();
    }
}
