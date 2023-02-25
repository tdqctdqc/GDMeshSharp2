
using System.Collections.Generic;
using Godot;

public class Forest : Vegetation, IDecaledTerrain
{
    public Forest() : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .5f, .25f, Colors.Limegreen, "Forest", false, new BlobTriBuilder())
    {
        
    }

    public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
    {
        var areaPer = 100f;
        var num = Mathf.FloorToInt(pt.GetArea() / areaPer);
        if (num < 1) return;
        var ps = pt.GetRandomPointsInside(num, 0f, 1f);
        var size = 3f;
        offset += Vector2.Down * size;

        ps.ForEach(p =>
        {
            var t = new Triangle(
                p + Vector2.Left * size + offset,
                p + Vector2.Right * size + offset,
                p + Vector2.Up * size * 2f + offset);
            mb.AddTri(t, Colors.DarkGreen);
        });
    }
}
