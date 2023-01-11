using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Graphics
{
    public static void BuildGraphics(Node node, GraphicLayerHolder holder, WorldData worldData)
    {
        var polyGraphics = holder.GenerateLayer(worldData.GeoPolygons, p => new PolygonGraphic(p), "polygons");
        node.AddChild(polyGraphics);

        var cellGraphics = holder.GenerateLayer(worldData.Cells, p => new CellGraphic(p), "cells");
        node.AddChild(cellGraphics);

        var plateGraphics = holder.GenerateLayer(worldData.Plates, p => p.GetPlateGraphic(), "plates");
        node.AddChild(plateGraphics);

        var massGraphics = holder.GenerateLayer(worldData.Masses, p => Graphics.GetMassGraphic(p), "masses");
        node.AddChild(massGraphics);

        var contGraphics =
            holder.GenerateLayer(worldData.Continents, p => Graphics.GetContinentGraphic(p), "continents");
        node.AddChild(contGraphics);

        // var contBbs = holder.GenerateLayer(worldData.Continents, p => p.BoundingBox.GetBoundingBoxMesh(100f), "continent BBs");
        // node.AddChild(contBbs);

        var landWaterGraphics = GetLandWaterGraphics(worldData);
        node.AddChild(landWaterGraphics);
        holder.AddLayer(landWaterGraphics, "landWater");

        // var polyLabels = GetPolygonLabelGraphics(worldData);
        // node.AddChild(polyLabels);

        // var mtnGraphics = holder.GenerateLayer(worldData.FaultLines, m => m.GetFaultLineGraphic(), "mountains");
        // node.AddChild(mtnGraphics);
    }

    public static Node2D GetLandWaterGraphics(WorldData data)
    {
        var tris = new List<Vector2>();
        var colors = new List<Color>();
        data.GeoPolygons.ForEach(p =>
        {
            var color = p.IsLand ? Colors.SaddleBrown : Colors.Blue;
            var newTris = p.GetTris();
            tris.AddRange(newTris);
            for (int i = 0; i < newTris.Count; i+=3)
            {
                colors.Add(color);
            }
        });
        var mesh = MeshGenerator.GetMeshInstance(tris, colors);
        return mesh;
    }

    public static Node2D GetContinentGraphic(this Continent cont, Color? contColor = null)
    {
        var result = new Node2D();
        if (contColor == null) contColor = ColorsExt.GetRandomColor();

        foreach (var mass in cont.Masses)
        {
            result.AddChild(GetMassGraphic(mass, contColor));
        }

        return result;
    }

    public static Node2D GetMassGraphic(this GeologyMass mass, Color? color = null)
    {
        var result = new Node2D();
        if (color == null) color = ColorsExt.GetRandomColor();
        foreach (var plate in mass.Plates)
        {
            result.AddChild(GetPlateGraphic(plate, color));
        }

        return result;
    }

    public static Node2D GetPlateGraphic(this GeologyPlate plate, Color? color = null)
    {
        var result = new Node2D();
        if (color == null) color = ColorsExt.GetRandomColor();
        foreach (var cell in plate.Cells)
        {
            result.AddChild(GetCellGraphic(cell, color));
        }



        var back = MeshGenerator.GetLineMesh(plate.Center + Vector2.Up * 50f, plate.Center + Vector2.Down * 50f,
            100f);
        var num = new Label();
        num.Text = plate.Id.ToString();
        num.RectScale = Vector2.One * 4f;
        num.Modulate = Colors.Black;
        num.RectGlobalPosition = plate.Center + Vector2.Left * 50f;
        back.AddChild(num);
        result.AddChild(back);
        // int iter = 0;
        // foreach (var aPlate in plate.Neighbors)
        // {
        //     var borderPointSegs = plate.GetOrderedBorderSegmentsWithPlate(aPlate);
        //     borderPointSegs.ForEach(s =>
        //     {
        //         var borderMesh = MeshGenerator.GetLinesMesh(s, 20f, false);
        //         borderMesh.Modulate = ColorsExt.GetRainbowColor(iter);
        //         iter++;
        //         result.AddChild(borderMesh);
        //     });
        // }

        return result;
    }

    public struct Vector2Pair
    {
        public float X1 { get; set; }
        public float Y1 { get; set; }

        public float X2 { get; set; }
        public float Y2 { get; set; }

        public Vector2Pair(Vector2 v1, Vector2 v2)
        {
            if (v1 == v2) throw new Exception();
            Vector2 v;
            Vector2 w;
            if (v1.x != v2.x)
            {
                v = v1.x > v2.x ? v1 : v2;
                w = v1.x > v2.x ? v2 : v1;
            }
            else
            {
                v = v1.y > v2.y ? v1 : v2;
                w = v1.y > v2.y ? v2 : v1;
            }

            X1 = v.x;
            Y1 = v.y;

            X2 = w.x;
            Y2 = w.y;
        }
    }

    public static Node2D GetCellGraphic(this GeologyCell geologyCell, Color? color = null)
    {
        var tris = new List<Vector2>();
        foreach (var poly in geologyCell.PolyGeos)
        {
            tris.AddRange(poly.GetTris());
        }

        var triMesh = MeshGenerator.GetMeshInstance(tris);
        if (color == null) color = ColorsExt.GetRandomColor();
        triMesh.Modulate = color.Value;
        return triMesh;
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
        var color = ColorsExt.GetRandomColor();
        result.Modulate = color;
        var tris = new List<Vector2>();
        f.PolyFootprint.ForEach(p =>
        {
            tris.AddRange(p.GetTris());
        });
        var triMesh = MeshGenerator.GetMeshInstance(tris);
        triMesh.Modulate = color;
        return triMesh;
    }

    public static Node2D GetPolygonLabelGraphics(WorldData worldData)
    {
        var node = new Node2D();
        var pointsMesh = MeshGenerator.GetPointsMesh(worldData.GeoPolygons.Select(p => p.Center).ToList(), 20f);
        node.AddChild(pointsMesh);
        worldData.GeoPolygons.ForEach(p =>
        {
            var back = MeshGenerator.GetLineMesh(p.Center + Vector2.Up * 10f, p.Center + Vector2.Down * 10f,
                20f);
            var num = new Label();
            num.Text = p.Id.ToString();
            num.RectScale = Vector2.One;
            num.Modulate = Colors.Black;
            num.RectGlobalPosition = p.Center + Vector2.Left * 10f;
            back.AddChild(num);
            node.AddChild(back);
        });
        return node;
    }
}
