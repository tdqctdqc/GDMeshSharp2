using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

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
        var riverData = new RiverPolyTriGen().DoRivers(key);
        report.StopSection("Finding rivers");
        
        report.StartSection();
        Parallel.ForEach(polys, p => BuildTris(p, riverData, key));
        // polys.ToList().ForEach(p => BuildTris(p, riverData, key));
        report.StopSection("Building poly terrain tris");
        
        
        GD.Print("doot");


        report.StartSection();
        Parallel.ForEach(_data.Planet.PolyEdges.Entities, p => MakeDiffPolyTriPaths(p, key));
        report.StopSection("making poly tri paths");
        
        _data.Notices.SetPolyShapes.Invoke();
        return report;
    }
    

    private void BuildTris(MapPolygon poly, TempRiverData rd, GenWriteKey key)
    {
        List<PolyTri> tris;
        var graph = new Graph<PolyTri, bool>();
        if (poly.IsWater())
        {
            tris = DoSeaPoly(poly, graph, key);
        }
        else if (rd.Infos.ContainsKey(poly))
        {
            var info = rd.Infos[poly];
            tris = info.LandTris
                .Union(info.BankTris.SelectMany(kvp => kvp.Value))
                .Union(info.InnerTris.Values.Select(kvp => kvp))
                .ToList();
            foreach (var t in tris)
            {
                graph.AddNode(t);
            }
        }
        else
        {
            tris = DoLandPolyNoRivers(poly, graph, key);
        }
        
        var polyTerrainTris = PolyTris.Create(tris,  graph, key);
        if (polyTerrainTris == null) throw new Exception();
        poly.SetTerrainTris(polyTerrainTris, key);
    }
    
    private List<PolyTri> DoSeaPoly(MapPolygon poly, Graph<PolyTri, bool> graph, GenWriteKey key)
    {
        var borderSegs = poly.GetOrderedBoundarySegs(key.Data);
        if (borderSegs.Count == 0) throw new Exception();

        var tris = new List<PolyTri>();
        
        for (var i = 0; i < borderSegs.Count; i++)
        {
            //todo produces overlap b/c non clockwise segs
            var seg = borderSegs[i];
            var pt = PolyTri.Construct(seg.From, seg.To, Vector2.Zero, LandformManager.Sea.MakeRef(),
                VegetationManager.Barren.MakeRef());
            tris.Add(pt);
            graph.AddNode(pt);
        }
        return tris;
    }
    
    private List<PolyTri> DoLandPolyNoRivers(MapPolygon poly, Graph<PolyTri, bool> graph, GenWriteKey key)
    {
        // var borderSegs = poly.GetOrderedBoundarySegs(key.Data);
        var borderPs = poly.GetOrderedBoundaryPoints(_data);
        List<PolyTri> tris = borderPs.PolyTriangulate(key.Data, poly);
        foreach (var polyTri in tris)
        {
            //todo actually build graph
            graph.AddNode(polyTri);
        }

        return tris;
    }

    

    private void MakeDiffPolyTriPaths(MapPolygonEdge edge, GenWriteKey key)
    {
        var lo = edge.LowPoly.Entity();
        var hi = edge.HighPoly.Entity();
        
        if (lo.Tris == null) throw new Exception();
        if (lo.Tris.Tris == null) throw new Exception();
        if (lo.GetBorder(hi.Id).Segments == null) throw new Exception();
        
        var loEdgeTris = lo.GetBorder(hi.Id).Segments
            .Select(seg => lo.Tris.Tris.FirstOrDefault(t => t.PointIsVertex(seg.From) && t.PointIsVertex(seg.To)))
            .Where(t => t != null)            
            .ToList();

        if (hi.Tris == null) throw new Exception();
        if (hi.Tris.Tris == null) throw new Exception();
        if (hi.GetBorder(lo.Id).Segments == null) throw new Exception();
        
        
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
