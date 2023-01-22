using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PolygonGraphic : Node2D
{
    public MapPolygon Poly { get; private set; }
    private Node2D _triMesh;
    public PolygonGraphic()
    {
        
    }

    public PolygonGraphic(MapPolygon poly, Data data, bool border = false)
    {
        Poly = poly;
        var tris = poly.GetTrisRel(data);
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