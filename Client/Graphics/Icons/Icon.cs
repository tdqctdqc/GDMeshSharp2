
using System;
using Godot;

public class Icon : MeshTexture
{
    public string Name { get; private set; }
    public Vector2 Dimension { get; private set; }
    public static QuadMesh SquareMesh { get; private set; } = MakeMesh(Vector2.One * 30f);
    public static QuadMesh OnePointFiveMesh { get; private set; } = MakeMesh(new Vector2(30f, 45f));
    public enum AspectRatio
    {
        Square, OneByOnePointFive
    }
    public static Icon Create(string textureName, AspectRatio ratio)
    {
        var i = new Icon();
        QuadMesh q;
        if (ratio == AspectRatio.Square)
        {
            q = SquareMesh;
        }
        else if (ratio == AspectRatio.OneByOnePointFive)
        {
            q = OnePointFiveMesh;
        }
        else throw new Exception();

        i.Mesh = q;
        i.Dimension = q.Size;
        i.Name = textureName;
        i.BaseTexture = TextureManager.Textures[textureName];
        return i;
    }
    public Icon()
    {
    }
    private static QuadMesh MakeMesh(Vector2 size)
    {
        var m = new QuadMesh();
        m.Size = size;
        return m;
    }

    public MeshInstance2D GetMeshInstance()
    {
        var mi = new MeshInstance2D();
        mi.Mesh = Mesh;
        mi.Texture = BaseTexture;
        return mi;
    }

    public TextureRect GetTextureRect(Vector2 dim)
    {
        var rect = new TextureRect();
        rect.Expand = true;
        rect.RectSize = dim;
        rect.Texture = BaseTexture;
        rect.RectScale = new Vector2(1f, -1f);
        return rect;
    }
}
