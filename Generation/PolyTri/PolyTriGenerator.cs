using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Poly2Tri;
using Poly2Tri.Triangulation.Polygon;
using Poly2Tri.Triangulation.Sets;
using Poly2Tri.Utility;

public class PolyTriGenerator : Generator
{
    private GenData _data;
    private IdDispenser _idd;
    public PolyTriGenerator()
    {
    }
    public override GenReport Generate(GenWriteKey key)
    {
        _idd = key.IdDispenser;
        _data = key.GenData;
        var report = new GenReport(GetType().Name);
        var polys = _data.Planet.Polygons.Entities;
        
        report.StartSection();
        new RiverPolyTriGen().DoRivers(key);
        report.StopSection("Finding river segs");
        
        report.StartSection();
        Parallel.ForEach(polys, p => BuildTris(p, key));
        report.StopSection("Building poly terrain tris");
        
        report.StartSection();
        Parallel.ForEach(_data.Planet.PolyEdges.Entities, p => MakeDiffPolyTriPaths(p, key));
        report.StopSection("making poly tri paths");
        
        _data.Notices.SetPolyShapes.Invoke();
        return report;
    }
    

    private void BuildTris(MapPolygon poly, GenWriteKey key)
    {
        List<PolyTri> tris;
        var graph = new Graph<PolyTri, bool>();
        if (poly.IsWater())
        {
            tris = DoSeaPoly(poly, graph, key);
        }
        // else if (poly.Neighbors.Entities()
        //          .Select(n => poly.GetEdge(n, _data))
        //          .Any(e => _riverEdgeWidths.ContainsKey(e)))
        // {
        //     tris = DoRiverPoly(poly, graph, key);
        // }
        else
        {
            tris = DoLandPolyNoRivers(poly, graph, key);
        }
            
        var polyTerrainTris = PolyTris.Create(tris,  graph, key);
        poly.SetTerrainTris(polyTerrainTris, key);
    }
    
    private List<PolyTri> DoSeaPoly(MapPolygon poly, Graph<PolyTri, bool> graph, GenWriteKey key)
    {
        var borderSegsRel = poly.NeighborBorders.SelectMany(b => b.Value.Segments)
            .Ordered<LineSegment, Vector2>().ToList();
        // if (borderSegsRel.IsCircuit() == false)
        // {
        //     GD.Print("completing " + poly.Id);
        //     borderSegsRel.Add(new LineSegment(borderSegsRel.Last().To, borderSegsRel[0].From));
        // }

        // if (borderSegsRel.IsCircuit() == false)
        // {
        //     throw new Exception();
        // }
        

        var lf = _data.Models.Landforms;
        var v = _data.Models.Vegetation;
        var points = borderSegsRel.GetPoints().ToList();
        points.Add(Vector2.Zero);
        
        var tris = DelaunayTriangulator.TriangulatePointsAndGetTriAdjacencies<PolyTri>(
                points, graph, (a, b, c) =>
                {
                    return PolyTri.Construct(a, b, c, LandformManager.Sea.MakeRef(),
                        VegetationManager.Barren.MakeRef());
                }
            )
            .ToList();
        return tris;
    }
    
    private List<PolyTri> DoLandPolyNoRivers(MapPolygon poly, Graph<PolyTri, bool> graph, GenWriteKey key)
    {
        var borderSegs = poly.NeighborBorders.SelectMany(b => b.Value.Segments).ToList();
        var borderPoints = borderSegs.GetPoints().ToList();

        List<PolyTri> tris = new List<PolyTri>();
    
        var innerPoints = borderSegs.GenerateInteriorPoints(50f, 10f);
        var allPoints = innerPoints.Union(borderPoints);
        try
        {
            tris = DelaunayTriangulator.TriangulatePointsAndGetTriAdjacencies<PolyTri>
            (
                allPoints.ToList(),
                graph,
                (a, b, c) =>
                {
                    var lf = key.Data.Models.Landforms.GetAtPoint(poly, (a + b + c) / 3f, key.Data);
                    var v = key.Data.Models.Vegetation.GetAtPoint(poly, (a + b + c) / 3f, lf, key.Data);
                    return PolyTri.Construct(a, b, c, lf.MakeRef(), v.MakeRef());
                }
            );
        }
        catch
        {
            GD.Print("bad triangulation at " + poly.Id + " " + poly.Center);
            GD.Print(borderPoints.Select(bp => bp.ToString()).ToArray());

            tris = DelaunayTriangulator.TriangulatePointsAndGetTriAdjacencies<PolyTri>
            (
                borderPoints.ToList(),
                graph,
                (a, b, c) =>
                {
                    var lf = key.Data.Models.Landforms.GetAtPoint(poly, (a + b + c) / 3f, key.Data);
                    var v = key.Data.Models.Vegetation.GetAtPoint(poly, (a + b + c) / 3f, lf, key.Data);
                    return PolyTri.Construct(a, b, c, lf.MakeRef(), v.MakeRef());
                }
            );
        }

        return tris;
    }

    

    private List<LineSegment> GetOutline(MapPolygon poly, Vector2 fanPoint, List<LineSegment> between)
    {
        List<LineSegment> generateBlade(Vector2 bladeStart, Vector2 bladeEnd)
        {
            return bladeEnd.GeneratePointsAlong(50f, 5f, true, null, bladeStart)
                .GetLineSegments()
                .ToList();
        }
        
        var end = generateBlade(between.Last().To, fanPoint);
        var start = generateBlade(fanPoint, between[0].From);
        var res = new List<LineSegment>();
        res.AddRange(start);
        res.AddRange(between);
        res.AddRange(end);

        if (res.IsCircuit() == false)
        {
            var ex = new SegmentsException();
            ex.AddSegLayer(between, "between");
            ex.AddSegLayer(res, "res");
            ex.AddSegLayer(start, "start");
            ex.AddSegLayer(end, "end");
            throw ex;
        }
        
        return res;
    }

    private void MakeDiffPolyTriPaths(MapPolygonEdge edge, GenWriteKey key)
    {
        var lo = edge.LowPoly.Entity();
        var hi = edge.HighPoly.Entity();
        
        var loEdgeTris = lo.GetBorder(hi.Id).Segments
            .Select(seg => lo.Tris.Tris.FirstOrDefault(t => t.PointIsVertex(seg.From) && t.PointIsVertex(seg.To)))
            .Where(t => t != null)            
            .ToList();
        
        var hiEdgeTris = hi.GetBorder(lo.Id).Segments
            .Select(seg => hi.Tris.Tris.FirstOrDefault(t => t.PointIsVertex(seg.From) && t.PointIsVertex(seg.To)))
            .Where(t => t != null)
            .Reverse()
            .ToList();
            
        if (loEdgeTris.Count != hiEdgeTris.Count)
        {
            return;
            // throw new Exception();
        }
        for (var i = 0; i < loEdgeTris.Count; i++)
        {
            edge.LoToHiTriPaths[loEdgeTris[i].Index] = hiEdgeTris[i].Index;
            edge.HiToLoTriPaths[hiEdgeTris[i].Index] = loEdgeTris[i].Index;
        }
    }
}
