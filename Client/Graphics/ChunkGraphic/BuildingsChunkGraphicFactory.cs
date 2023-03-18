
using System;
using Godot;

public class BuildingsChunkGraphicFactory : ChunkGraphicFactory
{
    public BuildingsChunkGraphicFactory(string name, bool active)
        : base(name, active,
            (c, d) => TriIconChunkGraphic.Create(c, d, p => p.GetBuildings(d),
                b => b.Position.Tri(), b => b.Model.Model().BuildingIcon))
    {
    }
}
