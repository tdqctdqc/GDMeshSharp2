
using System.Collections.Generic;
using Godot;

public class Forest : Vegetation, IDecaledTerrain
{
    public Forest() : base(new HashSet<Landform>{LandformManager.Hill, LandformManager.Plain}, 
        .4f, .5f, Colors.Limegreen.Darkened(.4f), "Forest", false)
    {
        
    }

    public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
    {
        
    }
    
    
    Mesh IDecaledTerrain.GetDecal()
    {
        var size = 3f;
        var offset = Vector2.Down * size / 2f;
        return MeshGenerator.GetArrayMesh(new Vector2[]{
            Vector2.Left * size + offset,
            Vector2.Right * size + offset,
            Vector2.Up * size * 2f + offset
        });
    }
    float IDecaledTerrain.DecalSpacing => 5f;
    Color IDecaledTerrain.GetDecalColor(PolyTri pt) => Colors.DarkGreen;
}
