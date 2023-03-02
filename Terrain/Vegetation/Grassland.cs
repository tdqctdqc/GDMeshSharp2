
using System.Collections.Generic;
using Godot;

public class Grassland : Vegetation, IDecaledTerrain
{
    public Grassland() 
        : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
            .3f, 1f, Colors.Limegreen, "Grassland", true)
    {
    }

    public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
    {
        // var ps = pt.GetPoissonPointsInside(5f);
        // var size = .5f;
        // offset += Vector2.Down * size;
        //
        // ps.ForEach(p =>
        // {
        //     var t1 = new Triangle(
        //         p + Vector2.Left * size + offset,
        //         p + Vector2.Right * size + offset,
        //         p + Vector2.Up * size * 2f + offset);
        //     var t2 = t1.Transpose(Vector2.Left * 2f);
        //     var t3 = t1.Transpose(Vector2.Right * 2f);
        //     mb.AddTri(t1, Colors.Limegreen.Darkened(.2f));
        //     mb.AddTri(t2, Colors.Limegreen.Darkened(.2f));
        //     mb.AddTri(t3, Colors.Limegreen.Darkened(.2f));
        // });
    }
}
