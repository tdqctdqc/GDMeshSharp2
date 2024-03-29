using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SettlementIconLayer : MapChunkGraphicLayer<int>
{
    public SettlementIconLayer(MapChunk chunk, Data data, MapGraphics mg) 
        : base(data, chunk, mg.ChunkChangedCache.SettlementTierChanged)
    {
        Init(data);
    }

    private SettlementIconLayer() : base()
    {
    }


    protected override Node2D MakeGraphic(int key, Data data)
    {
        var node = new Node2D();
        var settlement = data.Society.Settlements[key];
        var icon = settlement.Tier.Model().Icon;
        var poly = settlement.Poly.Entity();
        var urbanTris = poly.Tris.Tris
            .Where(t => t.Landform == LandformManager.Urban);
        foreach (var urbanTri in urbanTris)
        {
            var mesh = icon.GetMeshInstance();
            SetRelPos(mesh, new PolyTriPosition(poly.Id, urbanTri.Index), data);
            node.AddChild(mesh);
        }

        return node;
    }

    protected override IEnumerable<int> GetKeys(Data data)
    {
        return Chunk.Polys.Where(p => p.HasSettlement(data))
            .Select(p => p.GetSettlement(data).Id);
    }
}

