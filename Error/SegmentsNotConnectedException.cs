using System.Collections.Generic;
using System.Linq;
using Godot;

public class SegmentsNotConnectedException : DisplayableException
{
    public List<LineSegment> SegsBefore { get; private set; }
    public List<LineSegment> SegsAfter { get; private set; }
    public List<List<LineSegment>> Partials { get; private set; }
    public SegmentsNotConnectedException(List<LineSegment> segsBefore, List<LineSegment> segsAfter, 
        params List<LineSegment>[] partials)
    {
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
