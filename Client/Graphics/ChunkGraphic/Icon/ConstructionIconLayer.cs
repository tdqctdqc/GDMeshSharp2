using System;
using System.Collections.Generic;
using System.Linq;

public class ConstructionIconLayer : TriIconChunkLayer
{
    public ConstructionIconLayer(MapChunk chunk, Data data, MapGraphics mg) 
        : base(chunk, data, mg.ChunkChangedCache.ConstructionsChanged)
    {
    }
    private ConstructionIconLayer() : base()
    {
    }
    protected override IEnumerable<TriIcon> GetIcons(MapPolygon poly, Data data)
    {
        var constructions = poly.GetCurrentConstructions(data);
        if (constructions == null) return null;
        return poly.GetCurrentConstructions(data)
            .Select(b => new TriIcon(b.Model.Model().BuildingIcon, b.Pos));
    }
}
