using System;
using System.Linq;
using Godot;

public class SettlementsChunkGraphicFactory : ChunkGraphicFactory
{
    public SettlementsChunkGraphicFactory(string name, bool active) : base(name, active)
    {
    }

    public override Node2D GetNode(MapChunk c, Data d)
    {
        return TriIconChunkGraphic.Create<PolyTriPosition>(c, d, 
            p => p.Tris.Tris.Where(t => t.Landform == LandformManager.Urban)
                .Select(t => new PolyTriPosition(p.Id, t.Index)),
            pt => pt.Tri(d), pt => pt.Poly(d).GetSettlement(d).Tier.Model().Icon, 8f);
    }
}