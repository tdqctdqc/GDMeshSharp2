using System.Collections.Generic;
using System.Linq;
using Godot;

public class SegmentsNotConnectedException : DisplayableException
{
    public MapPolygon Poly { get; private set; }
    public List<LineSegment> SegsBefore { get; private set; }
    public List<LineSegment> SegsAfter { get; private set; }
    public List<List<LineSegment>> Partials { get; private set; }
    public Data Data { get; private set; }
    public SegmentsNotConnectedException(Data data, MapPolygon poly, List<LineSegment> segsBefore, List<LineSegment> segsAfter, 
        params List<LineSegment>[] partials)
    {
        Data = data;
        Poly = poly;
        SegsBefore = segsBefore;
        SegsAfter = segsAfter;
        Partials = partials.ToList();
        if (Partials == null) Partials = new List<List<LineSegment>>();
    }
    public override Control GetDisplay()
    {
        var d = SceneManager.Instance<SegmentsNotConnectedDisplay>();
        d.Setup(this);
        return d;
    }
}
