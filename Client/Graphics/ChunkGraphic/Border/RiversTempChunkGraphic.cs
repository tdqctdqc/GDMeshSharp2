using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RiversTempChunkGraphic : MapChunkGraphicModule
{
    public RiversTempChunkGraphic(MapChunk chunk, Data data)
    {
        var layer = new RiversTempChunkLayer(chunk, null);
        AddLayer(new Vector2(0f, 1f), layer);
        layer.Draw(data);
    }

    private RiversTempChunkGraphic()
    {
        
    }

    private class RiversTempChunkLayer : MapChunkGraphicLayer
    {
        public RiversTempChunkLayer(MapChunk chunk, ChunkChangeListener listener) 
            : base(chunk, listener)
        {
        }

        private RiversTempChunkLayer()
        {
            
        }
        public override void Draw(Data data)
        {
            // var rd = data.Planet.GetRegister<TempRiverData>().Entities.First();
            // var relTo = Chunk.RelTo;
            // var edges = Chunk.Polys
            //     .SelectMany(p => p.GetEdges(data))
            //     .Distinct();
            // var mb = new MeshBuilder();
            //
            // foreach (var poly in Chunk.Polys)
            // {
            //     if (rd.Infos.ContainsKey(poly) == false) continue;
            //     var info = rd.Infos[poly];
            //     var offset = relTo.GetOffsetTo(poly, data);
            //     int i = 0;
            //     var col = Colors.Blue;//ColorsExt.GetRandomColor();
            //     
            //     foreach (var kvp in info.BankTris)
            //     {
            //         kvp.Value.ForEach(t => 
            //             mb.AddTri(t.Transpose(offset), col)
            //         );
            //     }
            //     foreach (var kvp in info.InnerTris)
            //     {
            //         mb.AddTri(kvp.Value.Transpose(offset), Colors.Blue);
            //     }
            //     int iter = 0;
            //     // foreach (var lt in info.LandTris)
            //     // {
            //     //     mb.AddTri(lt.Transpose(offset), col.GetPeriodicShade(iter++));
            //     // }
            //     // mb.AddArrowsRainbow(
            //     //     info.InnerBoundary
            //     //         .Select(ls => new LineSegment(ls.From * .9f, ls.To * .9f))
            //     //         .Select(ls => ls.Translate(relTo.GetOffsetTo(poly, data)))
            //     //         .ToList(),
            //     //     2f
            //     // );
            // }
            // if(mb.Tris.Count > 0) 
            //     AddChild(mb.GetMeshInstance());
        }
    }
}
