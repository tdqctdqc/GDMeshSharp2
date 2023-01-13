using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonGraphic : Node2D
{
    public Polygon Poly { get; private set; }
    private Node2D _triMesh;
    public PolygonGraphic()
    {
        
    }

    public PolygonGraphic(Polygon poly, bool border = false)
    {
        Poly = poly;
        var tris = poly.GetTrisRel();
        // Position = poly.Center;
        _triMesh = MeshGenerator.GetMeshInstance(tris);
        _triMesh.Modulate = new Color(Poly.Color, .5f);
        AddChild(_triMesh);
        

        if(poly.Id % 25 == 0)
        {
            // AddBorderGraphic(poly);

            // AddBorderPolysGraphic(poly, color.Value.Inverted());
        }

    }

    public void SetColor(Color color)
    {
        _triMesh.Modulate = color;
    }

    public void ClearColor()
    {
        _triMesh.Modulate = Poly.Color;
    }
    private void AddBorderPolysGraphic(Polygon poly, Color color)
    {
        
        for (var i = 0; i < poly.Neighbors.Count; i++)
        {
            var edge = poly.GetEdge(poly.Neighbors[i]);
            var offset = edge.GetOffsetToOtherPoly(poly);
            var centerArrow = MeshGenerator.GetArrowGraphic(Vector2.Zero, offset, 10f);
            AddChild(centerArrow);
            


            var next = (i + 1) % poly.Neighbors.Count;
            
            var from = poly.GetEdge(poly.Neighbors[i]).GetPointsRel(poly).Avg();
            var to = poly.GetEdge(poly.Neighbors[next]).GetPointsRel(poly).Avg();
            var arrow = MeshGenerator.GetArrowGraphic(from, to, 5f);
            arrow.Modulate = color;
            AddChild(arrow);
        }
    }
    private void AddBorderGraphic(Polygon poly)
    {
        var borders = poly.NeighborBorders.ToList();
        var iter = 0;
        for (var i = 0; i < borders.Count; i++)
        {
            var border = borders[i].GetSegsRel(poly);
            for (var j = 0; j < border.Count; j++)
            {
                var seg = border[j];
                var from = seg.From * .9f;
                var to = seg.To * .9f;
                if (from == to) continue;
                var arrow = MeshGenerator.GetArrowGraphic(from, to, 5f);
                arrow.Modulate = ColorsExt.GetRainbowColor(iter);
                iter++;
                AddChild(arrow);
            }
        }
    }

    private void AddLabel()
    {
        var back = MeshGenerator.GetLineMesh(Poly.Center + Vector2.Up * 10f, Poly.Center + Vector2.Down * 10f,
            20f);
        var num = new Label();
        num.Text = Poly.Id.ToString();
        num.RectScale = Vector2.One;
        num.Modulate = Colors.Black;
        num.RectGlobalPosition = Poly.Center + Vector2.Left * 10f;
        back.AddChild(num);
        AddChild(back);
    }
}