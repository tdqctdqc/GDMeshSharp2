
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BadTriangulationError : DisplayableException
{
    public MapPolygon Poly { get; private set; }
    public GenData Data { get; private set; }
    public List<Triangle> Tris { get; private set; }
    public List<Color> Colors { get; private set; }
    public List<List<LineSegment>> Outlines { get; private set; }

    public BadTriangulationError(MapPolygon poly, IReadOnlyList<Triangle> tris, List<Color> colors, GenData data,
        params List<LineSegment>[] outlines)
    {
        Poly = poly;
        Data = data;
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
