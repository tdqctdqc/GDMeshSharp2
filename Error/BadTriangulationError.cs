
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BadTriangulationError : DisplayableException
{
    public List<Triangle> Tris { get; private set; }
    public List<Color> Colors { get; private set; }
    public List<List<LineSegment>> Outlines { get; private set; }

    public BadTriangulationError(IReadOnlyList<Triangle> tris, List<Color> colors, 
        params List<LineSegment>[] outlines)
    {
        Tris = tris.ToList();
        Colors = colors;
        Outlines = outlines.ToList();
    }

    public override Control GetDisplay()
    {
        var d = SceneManager.Instance<BadTriangulationDisplay>();
        d.Setup(this);
        return d;
    }
}
