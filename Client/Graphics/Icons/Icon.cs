
using System;
using Godot;

public class Icon : MeshTexture
{
    public static Mesh IconMesh { get; private set; } = MakeMesh();

    public static Icon Create(string textureName)
    {
        var i = new Icon();
        i.Mesh = IconMesh;
        i.BaseTexture = TextureManager.Textures[textureName];
        return i;
    }
    public Icon()
    {
    }
    private static Mesh MakeMesh()
    {
        var m = new QuadMesh();
        m.Size = Vector2.One * 30f;
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
