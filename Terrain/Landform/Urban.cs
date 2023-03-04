using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Urban : Landform, IDecaledTerrain
{
    
    public Urban() 
        : base("Urban", 1000f, 0f, Colors.Black)
    {
        
    }

    public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
    {
        mb.AddTri(pt.Transpose(offset), Colors.Red.Darkened(.25f));
        var ps = pt.GetPoissonPointsInside(10f);
        var size = 5f;

        ps.ForEach(p =>
        {
            var t1 = new Triangle(
                p + Vector2.Left * size + offset,
                p + Vector2.Right * size + offset,
                p + Vector2.Up * size * 3f + Vector2.Left * size + offset);
            var t2 = new Triangle(
                p + Vector2.Right * size + offset,
                p + Vector2.Up * size * 3f + Vector2.Left * size + offset,
                p + Vector2.Up * size * 3f + Vector2.Right * size + offset);
            mb.AddTri(t1, Colors.DarkGray);
            mb.AddTri(t2, Colors.DarkGray);
        });
    }
    
    
    Mesh IDecaledTerrain.GetDecal()
    {
        var spacing = ((IDecaledTerrain) this).DecalSpacing;
        var offset = Vector2.Down * spacing / 2f;
        var size = 5f;
        return MeshGenerator.GetArrayMesh(new Vector2[]{
            Vector2.Left * size + offset,
            Vector2.Right * size + offset,
            Vector2.Up * size * 3f + Vector2.Left * size + offset,
            Vector2.Right * size + offset,
            Vector2.Up * size * 3f + Vector2.Left * size + offset,
            Vector2.Up * size * 3f + Vector2.Right * size + offset
        });
    }
    float IDecaledTerrain.DecalSpacing => 15f;
    Color IDecaledTerrain.GetDecalColor(PolyTri pt) => Colors.DarkGray;
}