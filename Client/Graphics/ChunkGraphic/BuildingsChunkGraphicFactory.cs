
using System;
using Godot;

public class BuildingsChunkGraphicFactory : ChunkGraphicFactory
{
    public BuildingsChunkGraphicFactory(string name, bool active) : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        return TriIconChunkGraphic.Create(c, d, p => p.GetBuildings(d),
            b => b.Position.Tri(d), b => b.Model.Model().BuildingIcon, 2.5f);
    }
}
