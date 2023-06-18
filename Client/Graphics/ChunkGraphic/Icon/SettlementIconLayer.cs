using System;
using System.Collections.Generic;
using System.Linq;

public class SettlementIconLayer : TriIconChunkLayer
{
    public SettlementIconLayer(MapChunk chunk, Data data, MapGraphics mg, 
        params ChunkChangeListener[] listeners) : base(chunk, data, mg.ChunkChangedCache.SettlementTierChanged)
    {
    }

    private SettlementIconLayer() : base()
    {
    }

    protected override IEnumerable<TriIcon> GetIcons(MapPolygon poly, Data data)
    {
        var settlement = poly.GetSettlement(data);
        if (settlement == null) return null;
        var tier = settlement.Tier.Model();
        
        return poly.Tris.Tris.Where(t => t.Landform == LandformManager.Urban)
            .Select(t => new TriIcon(tier.Icon, t.GetPosition()));
    }
}

