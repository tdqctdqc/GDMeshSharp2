
using System;
using Godot;

public class Icon : MeshTexture
{
    public string Name { get; private set; }
    public Vector2 Dimension { get; private set; }
    public static QuadMesh _1x1Mesh { get; private set; } = MakeMesh(Vector2.One * 15f);
    public static QuadMesh _1x2Mesh { get; private set; } = MakeMesh(new Vector2(15f, 30f));
    public static QuadMesh _2x3Mesh { get; private set; } = MakeMesh(new Vector2(30f, 45f));
    public enum AspectRatio
    {
        _1x1, _2x3, _1x2
    }
    public static Icon Create(string textureName, AspectRatio ratio)
    {
        var i = new Icon();
        QuadMesh q;
        if (ratio == AspectRatio._1x1)
        {
            q = _1x1Mesh;
        }
        else if (ratio == AspectRatio._1x2)
        {
            q = _1x2Mesh;
        }
        else if (ratio == AspectRatio._2x3)
        {
            q = _2x3Mesh;
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
