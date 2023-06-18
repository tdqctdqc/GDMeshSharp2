using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public class PolyTriChunkGraphic : MapChunkGraphicModule
{
    public PolyTriChunkGraphic(MapChunk chunk, Data data, MapGraphics mg)
    {
        var lfLayer = new PolyTriLayer(data, t => t.Landform.Color, 
            chunk, null);
        AddLayer(new Vector2(0f, 1f), lfLayer);
        var vegLayer = new PolyTriLayer(data, t => t.Vegetation.Color.Darkened(t.Landform.DarkenFactor), 
            chunk, null);
        AddLayer(new Vector2(0f, 1f), vegLayer);
    }

    private PolyTriChunkGraphic()
    {
        
    }

    private class PolyTriLayer : MapChunkGraphicLayer
    {
        private Func<PolyTri, Color> _getColor;
        public PolyTriLayer(Data data, Func<PolyTri, Color> getColor, MapChunk chunk, ChunkChangeListener listener) 
            : base(chunk, listener)
        {
            _getColor = getColor;
            Draw(data);
        }
        public override void Draw(Data data)
        {
            var first = Chunk.RelTo;
            var mb = new MeshBuilder();
            foreach (var p in Chunk.Polys)
            {
                var offset = first.GetOffsetTo(p, data);
                var tris = p.Tris.Tris;
                for (var j = 0; j < tris.Length; j++)
                {
                    var t = tris[j];
                    // if (t.GetMinAltitude() < 10f) continue;
                    mb.AddTri(t.Transpose(offset), 
                        _getColor(t)
                    );
                }
            }

            if (mb.Tris.Count == 0) return;
            var mesh = mb.GetMeshInstance();
            AddChild(mesh);
        }
    }
}