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

        Root.ButtonContainer.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeologyPolygon;
                var color = g.Color;
                p.SetColor(color);
            });
        }, "Show Polys");
        Root.ButtonContainer.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeologyPolygon;
                var color = g.Cell.Seed.Color;
                p.SetColor(color);
            });
        }, "Show Poly Cells");
        
        Root.ButtonContainer.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeologyPolygon;
                var color = g.Cell.Plate.SeedPoly.Color;
                p.SetColor(color);
            });
        }, "Show Poly Plates");
        
        Root.ButtonContainer.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeologyPolygon;
                var color = g.Cell.Plate.Mass.SeedPoly.Color;
                p.SetColor(color);
            });
        }, "Show Poly Masses");
        Root.ButtonContainer.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeologyPolygon;
                var color = g.Cell.Plate.Mass.Continent.SeedPoly.Color;
                p.SetColor(color);
            });
        }, "Show Poly Continents");
        Root.ButtonContainer.AddButton(() =>
        {
            polyGraphics.ForEach(p =>
            {
                var g = p.Poly as GeologyPolygon;
                var color = g.IsLand ? Colors.SaddleBrown : Colors.Blue;
                p.SetColor(color);
            });
        }, "Show Land/Sea");
        var mtnGraphics = worldData.FaultLines.Select(f => f.GetFaultLineGraphic());
        var mtnNode = new GraphicsSegmenter<Node2D>();
        // mtnNode.Setup();
        holder.AddOverlay("polygons", "mountain footprints", mtnNode);
        node.AddChild(mtnNode);
    }


    public static Node2D GetFaultLineGraphic(this FaultLine f)
    {
        var result = new Node2D();
        f.Segments.ForEach(seg =>
        {
            var segLines = MeshGenerator.GetLinesMesh(seg, 10f, false);
            var segPoints = MeshGenerator.GetPointsMesh(seg, 20f);
            result.AddChild(segLines);
            result.AddChild(segPoints);
        });
        var color = Colors.Gray;//ColorsExt.GetRandomColor();
        result.Modulate = color;
        var tris = new List<Vector2>();
        f.PolyFootprint.ForEach(p =>
        {
            tris.AddRange(p.GetTrisAbs());
        });
        var triMesh = MeshGenerator.GetMeshInstance(tris);
        triMesh.Modulate = color;
        return triMesh;
    }
}
