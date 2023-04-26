
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Google.OrTools.LinearSolver;

public class TriIconChunkGraphic : MapChunkGraphicLayer
{
    public TriIconChunkGraphic(MapChunk chunk, Data data, MapGraphics mg) 
        : base(chunk, mg.ChunkChangedCache.BuildingsChanged)
    {
        Draw(data);
    }


    protected override void Draw(Data data)
    {
        this.ClearChildren();
        var zoom = Game.I.Client.Cam.ZoomOut;
        var scaled = Game.I.Client.Cam.ScaledZoomOut;
        var mod = 1f;
        
        var iconDic = new Dictionary<Icon, List<Vector2>>();
        foreach (var p in Chunk.Polys)
        {
            var offset = Chunk.RelTo.GetOffsetTo(p, data);
            var deposits = p.GetResourceDeposits(data);
            if (deposits != null)
            {
                var controller = new IconGroupController<ResourceDeposit>(deposits.ToList(),
                    r => "", r => r.Item.Model().Icon, 100000f);
                var group = new IconGroups(new List<IIconGroupController> {controller});
                group.Position = offset;
                group.Scale *= Mathf.Min(10f, zoom / 10f);
                AddChild(group);
            }


            var buildings = p.GetMapBuildings(data);
            if (buildings != null)
            {
                foreach (var b in buildings)
                {
                    var icon = b.Model.Model().BuildingIcon;
                    var mesh = icon.GetMeshInstance();
                    var pos = offset + b.Position.Tri(data).GetCentroid();
                    mesh.Position = pos;
                    mesh.Scale *= mod;
                    AddChild(mesh);
                }
            }

            var settlement = p.GetSettlement(data);
            if (settlement != null)
            {
                var tier = settlement.Tier.Model();
                foreach (var urban in p.Tris.Tris.Where(t => t.Landform == LandformManager.Urban))
                {
                    var icon = tier.Icon;
                    var mesh = icon.GetMeshInstance();
                    var pos = offset + urban.GetCentroid();
                    mesh.Position = pos;
                    mesh.Scale *= mod;
                    AddChild(mesh);
                }
            }
        }
    }
}
