using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class IconsChunkGraphic : MapChunkGraphicModule
{
    public IconsChunkGraphic(MapChunk chunk, Data data, MapGraphics mg)
    {
        // var triIcons = new TriIconChunkGraphic(chunk, data, mg);
        // AddLayer(new Vector2(0f, .5f), triIcons);
        var buildings = new BuildingIconLayer(chunk, data, mg);
        AddLayer(new Vector2(0f, .5f), buildings);

        var constructions = new ConstructionIconLayer(chunk, data, mg);
        AddLayer(new Vector2(0f, .5f), constructions);

        var settlements = new SettlementIconLayer(chunk, data, mg);
        AddLayer(new Vector2(0f, .5f), settlements);
        
        var regimeIcons = new RegimeFlagChunkLayer(chunk, data, mg);
        AddLayer(new Vector2(.4f, .8f), regimeIcons);
    }

    private IconsChunkGraphic()
    {
    }
    // private class TriIconChunkGraphic : MapChunkGraphicLayer
    // {
    //     public TriIconChunkGraphic(MapChunk chunk, Data data, MapGraphics mg) 
    //         : base(chunk, 
    //             mg.ChunkChangedCache.BuildingsChanged,
    //             mg.ChunkChangedCache.ConstructionsChanged
    //             )
    //     {
    //         Draw(data);
    //     }
    //
    //     //todo Split this up into construction ,building, etc
    //     public override void Draw(Data data)
    //     {
    //         this.ClearChildren();
    //         
    //         foreach (var p in Chunk.Polys)
    //         {
    //             var offset = Chunk.RelTo.GetOffsetTo(p, data);
    //             // var deposits = p.GetResourceDeposits(data);
    //             // if (deposits != null)
    //             // {
    //             //     var controller = new IconGroupController<ResourceDeposit>(deposits.ToList(),
    //             //         r => "", r => r.Item.Model().Icon, 100000f);
    //             //     var group = new IconGroups(new List<IIconGroupController> {controller});
    //             //     group.Position = offset;
    //             //     group.Scale *= Mathf.Min(10f, zoom / 10f);
    //             //     AddChild(group);
    //             // }
    //
    //
    //             var buildings = p.GetBuildings(data);
    //             if (buildings != null)
    //             {
    //                 foreach (var b in buildings)
    //                 {
    //                     var icon = b.Model.Model().BuildingIcon;
    //                     var mesh = icon.GetMeshInstance();
    //                     var pos = offset + b.Position.Tri(data).GetCentroid();
    //                     mesh.Position = pos;
    //                     AddChild(mesh);
    //                 }
    //             }
    //
    //             var settlement = p.GetSettlement(data);
    //             if (settlement != null)
    //             {
    //                 var tier = settlement.Tier.Model();
    //                 foreach (var urban in p.Tris.Tris.Where(t => t.Landform == LandformManager.Urban))
    //                 {
    //                     var icon = tier.Icon;
    //                     var mesh = icon.GetMeshInstance();
    //                     var pos = offset + urban.GetCentroid();
    //                     mesh.Position = pos;
    //                     AddChild(mesh);
    //                 }
    //             }
    //
    //             var constructions = p.GetCurrentConstructions(data);
    //             if (constructions != null)
    //             {
    //                 foreach (var c in constructions)
    //                 {
    //                     var bIcon = c.Model.Model().BuildingIcon;
    //                     var bMesh = bIcon.GetMeshInstance();
    //                     var pos = offset + c.Pos.Tri(data).GetCentroid();
    //                     bMesh.Position = pos;
    //                     bMesh.Modulate = Colors.Black;
    //                     AddChild(bMesh);
    //                 }
    //             }
    //         }
    //     }
    // }
    
}
