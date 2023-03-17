
using System.Linq;
using Godot;

public class BuildingChunkGraphic : Node2D
{
    public BuildingChunkGraphic(MapChunk chunk, Data data)
    {
        foreach (var p in chunk.Polys)
        {
            var offset = chunk.RelTo.GetOffsetTo(p, data);
            var buildings = p.GetBuildings(data);
            if (buildings == null) continue;
            foreach (var b in buildings)
            {
                var r = new MeshInstance2D();
                var mesh = new QuadMesh();
                mesh.Size = Vector2.One * 20f;
                r.Scale = new Vector2(1f, -1f);
                r.Mesh = mesh;
                r.Texture = b.Model.Model().Icon;
                r.Position = offset + b.Position.Tri().GetCentroid();
                AddChild(r);
            }
        }
    }
}
