using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Graphics
{
    public static void BuildGraphics(Node node, GraphicLayerHolder holder, WorldData worldData)
    {
        var polyGraphics = worldData.GeoPolygons.Select(p => new PolygonGraphic(p)).ToList();
        var polySegmenter = new GraphicsSegmenter<PolygonGraphic>();
        polySegmenter.Setup(polyGraphics, 10, p => p.Poly.Center);
        holder.AddView(polySegmenter, "polygons");
        node.AddChild(polySegmenter);
        AddPolyViewMode(polyGraphics, g => g.Color, "Polys");
        AddPolyViewMode(polyGraphics, g => g.Cell.Seed.Color, "Poly Cells");
        AddPolyViewMode(polyGraphics, g => g.Cell.Plate.SeedPoly.Color, "Poly Plates");
        AddPolyViewMode(polyGraphics, g => g.Cell.Plate.Mass.SeedPoly.Color, "Poly Masses");
        AddPolyViewMode(polyGraphics, g => g.Cell.Plate.Mass.Continent.SeedPoly.Color, "Poly Continents");
        AddPolyViewMode(polyGraphics, g => g.IsLand ? Colors.SaddleBrown : Colors.Blue, "Land/Sea");
        AddPolyViewMode(polyGraphics, g => Colors.White.LinearInterpolate(Colors.Red, g.Roughness), "Poly Roughness");
        // AddPolyViewMode(polyGraphics, g => LandformManager.GetLandformFromPolyRoughness(g).Color, "Poly Landforms");
        
        // var faultLineGraphics = worldData.FaultLines.Select(f => new FaultLineGraphic(f)).ToList();
        // faultLineGraphics.ForEach(mg =>
        // {
        //     mg.Position = mg.Origin;
        // });
        // var faultLineNode = new GraphicsSegmenter<FaultLineGraphic>();
        // faultLineNode.Setup(faultLineGraphics, 10, m => m.Origin);
        // holder.AddOverlay("polygons", "fault lines", faultLineNode);
        // node.AddChild(faultLineNode);

        var landformTris = GetTerrainAspectTrisGraphics(worldData.Landforms);
        holder.AddOverlay("polygons", "Landform Tris", landformTris);
        node.AddChild(landformTris);
        
        var vegTris = GetTerrainAspectTrisGraphics(worldData.Vegetation);
        holder.AddOverlay("polygons", "Vegetation Tris", vegTris);
        node.AddChild(vegTris);
    }

    private static void AddPolyViewMode(List<PolygonGraphic> polyGraphics, Func<GeologyPolygon, Color> getColor, string name)
    {
        Root.ButtonContainer.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeologyPolygon;
                var color = getColor(g);
                p.SetColor(color);
            });
        }, "Show " + name);
    }

    private static Node2D GetTerrainAspectTrisGraphics<T>(TerrainAspectManager<T> aspectManager)
        where T : TerrainAspect
    {
        var els = new List<Node2D>();
        for (var i = aspectManager.LandByPriority.Count - 1; i >= 0; i--)
        {
            var aspect = aspectManager.LandByPriority[i];
            var holder = aspectManager.Holders[aspect];
            foreach (var kvp2 in holder.Tris)
            {
                if (kvp2.Value.Count == 0) continue;
                var tris = new List<Vector2>();
                kvp2.Value.ForEach(tri =>
                {
                    tris.Add(tri.A);
                    tris.Add(tri.B);
                    tris.Add(tri.C);
                });
                var mesh = MeshGenerator.GetMeshInstance(tris);
                mesh.Position = kvp2.Key.Center;
                mesh.Modulate = aspect.Color;
                els.Add(mesh);
            }
        }
        var res = new GraphicsSegmenter<Node2D>();
        res.Setup(els, 10, e => e.Position);
        return res;
    }
}
