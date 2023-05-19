using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class IconsChunkGraphic : MapChunkGraphicModule
{
    public IconsChunkGraphic(MapChunk chunk, Data data, MapGraphics mg)
    {
        var triIcons = new TriIconChunkGraphic(chunk, data, mg);
        AddLayer(new Vector2(0f, .5f), triIcons);
        var regimeIcons = new RegimeIconChunkLayer(chunk, data, mg);
        AddLayer(new Vector2(.4f, .8f), regimeIcons);
    }

    private IconsChunkGraphic()
    {
    }
    private class TriIconChunkGraphic : MapChunkGraphicLayer
    {
        public TriIconChunkGraphic(MapChunk chunk, Data data, MapGraphics mg) 
            : base(chunk, mg.ChunkChangedCache.BuildingsChanged)
        {
            Draw(data);
        }


        public override void Draw(Data data)
        {
            this.ClearChildren();
            var zoom = Game.I.Client.Cam.ZoomOut;
            var scaled = Game.I.Client.Cam.ScaledZoomOut;
            var mod = 1f;
            
            var iconDic = new Dictionary<Icon, List<Vector2>>();
            foreach (var p in Chunk.Polys)
            {
                var offset = Chunk.RelTo.GetOffsetTo(p, data);
                // var deposits = p.GetResourceDeposits(data);
                // if (deposits != null)
                // {
                //     var controller = new IconGroupController<ResourceDeposit>(deposits.ToList(),
                //         r => "", r => r.Item.Model().Icon, 100000f);
                //     var group = new IconGroups(new List<IIconGroupController> {controller});
                //     group.Position = offset;
                //     group.Scale *= Mathf.Min(10f, zoom / 10f);
                //     AddChild(group);
                // }


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
    private class RegimeIconChunkLayer : MapChunkGraphicLayer
    {
        public RegimeIconChunkLayer(MapChunk chunk, Data data, MapGraphics mg) 
            : base(chunk, mg.ChunkChangedCache.PolyRegimeChanged)
        {
            Draw(data);
        }
        public override void Draw(Data data)
        {
            this.ClearChildren();
            foreach (var p in Chunk.Polys)
            {
                var reg = data.Society.Regimes.Entities.FirstOrDefault(r => r.Capital.RefId == p.Id);
                if (reg != null)
                {
                    var center = reg.Polygons.Select(po => Chunk.RelTo.GetOffsetTo(po.Center, data)).Avg();
                    var flagDim = new Vector2(150f, 100f);
                    var scale = 3f;
                    var vbox = new VBoxContainer();
                    vbox.RectScale = Vector2.One * scale;
                    var backRect = new ColorRect();
                    var margin = 10f;
                    backRect.RectMinSize = flagDim + margin * Vector2.One;
                    backRect.Color = Colors.Black;
                    var flagRect = new TextureRect();
                    flagRect.RectPosition = Vector2.One * margin / 2f;
                    flagRect.RectMinSize = flagDim;
                    flagRect.Expand = true;
                    flagRect.Texture = reg.Template.Model().Flag;
                    backRect.AddChild(flagRect);
                    vbox.AddChild(backRect);
                    var label = new Label();
                    label.Autowrap = true;
                    label.Theme = (Theme) GD.Load("res://Assets/Themes/DefaultTheme.tres");
                    label.Text = reg.Name;
                    vbox.AddChild(label);
                    vbox.RectPosition = center - scale * flagDim / 2f;;
                    AddChild(vbox);
                }
            }
        }
    }
}
