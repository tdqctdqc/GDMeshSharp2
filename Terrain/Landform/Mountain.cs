
using Godot;

public class Mountain : Landform, IDecaledTerrain
{
    public Mountain() 
        : base("Mountain", .6f, 0f, Colors.DimGray)
    {
    }

    public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
    {
        var size = 30f;
        var color = Color.Darkened(.25f);
        offset += Vector2.Down * size / 2f;
        var p = pt.GetCentroid();
        var t = new Triangle(
            p + Vector2.Left * size + offset,
            p + Vector2.Right * size + offset,
            p + Vector2.Up * size + offset);
        mb.AddTri(t, color);
    }
}
