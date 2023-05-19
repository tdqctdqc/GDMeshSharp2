
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BorderChunkGraphic : MapChunkGraphicModule
{
    public static BorderChunkGraphic ConstructRegimeBorder(MapChunk chunk, MapGraphics mg, float thickness, Data data)
    {
        return new BorderChunkGraphic(chunk, mg, thickness, data);
    }

    private BorderChunkGraphic(MapChunk chunk, MapGraphics mg, float thickness, Data data)
    {
        var layer = new RegimeBorderChunkLayer(chunk, thickness, data, mg);
        AddLayer(new Vector2(0f, 1f), layer);
    }
    private BorderChunkGraphic()
    {
        
    }
    
    
    private class RegimeBorderChunkLayer : MapChunkGraphicLayer
    {
        private float _thickness;

        public RegimeBorderChunkLayer(MapChunk chunk, float thickness, Data data, MapGraphics mg)
            : base(chunk, mg.ChunkChangedCache.PolyRegimeChanged)
        {
            _thickness = thickness;
            Draw(data);
        }
        private RegimeBorderChunkLayer() : base()
        {
        }

        public override void Draw(Data data)
        {
            this.ClearChildren();
            var mb = new MeshBuilder();
            var regPolys = Chunk.Polys.Where(p => p.Regime.Empty() == false);
            foreach (var p in regPolys)
            {
                var color = p.Regime.Entity().SecondaryColor;
                var offset = Chunk.RelTo.GetOffsetTo(p, data);
                foreach (var n in p.Neighbors.Entities())
                {
                    if (n.Regime.RefId == p.Regime.RefId) continue;
                    mb.DrawMapPolyEdge(p, n, data, _thickness, color, offset);
                }
            }
        
            if (mb.Tris.Count == 0) return;
            AddChild(mb.GetMeshInstance());
        }
    }
    
}
