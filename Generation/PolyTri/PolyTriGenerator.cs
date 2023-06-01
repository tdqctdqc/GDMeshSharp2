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
        Parallel.ForEach(polys, p =>
        {
            BuildTris(p, riverData, key);
        });
        report.StopSection("Building poly terrain tris");
        
        report.StartSection();
        Parallel.ForEach(_data.Planet.PolyEdges.Entities, p => MakeDiffPolyTriPaths(p, key));
        report.StopSection("making poly tri paths");
        
        _data.Notices.SetPolyShapes.Invoke();
        
        report.StartSection();
        Postprocess(key);
        report.StopSection("postprocessing polytris");
        return report;
    }
    

    private void BuildTris(MapPolygon poly, TempRiverData rd, GenWriteKey key)
    {
        List<PolyTri> tris;
        if (poly.IsWater())
        {
            tris = DoSeaPoly(poly, key);
        }
        else if (poly.GetNexi(key.Data).Any(n => n.IsRiverNexus()))
        {
            tris = NewRiverTriGen.DoPoly(poly, key.Data, rd, key);
        }
        else
        {
            tris = DoLandPolyNoRivers(poly, key);
        }
        
        var polyTerrainTris = PolyTris.Create(tris,  key);
        if (polyTerrainTris == null) throw new Exception();
        poly.SetTerrainTris(polyTerrainTris, key);
    }
    
    private List<PolyTri> DoSeaPoly(MapPolygon poly, GenWriteKey key)
    {
        var tris = new List<PolyTri>();
        var boundaryPs = poly.GetOrderedBoundaryPoints(_data);
        var triPIndices = Geometry.TriangulatePolygon(boundaryPs);
        for (var i = 0; i < triPIndices.Length; i+=3)
        {
            var a = boundaryPs[triPIndices[i]];
            var b = boundaryPs[triPIndices[i+1]];
            var c = boundaryPs[triPIndices[i+2]];
            tris.Add(PolyTri.Construct(a,b,c,LandformManager.Sea.MakeRef(),
                VegetationManager.Barren.MakeRef()));
        }
        return tris;
    }
    
    private List<PolyTri> DoLandPolyNoRivers(MapPolygon poly, GenWriteKey key)
    {
        var borderPs = poly.GetOrderedBoundaryPoints(_data);
        List<PolyTri> tris = borderPs.PolyTriangulate(key.Data, poly);

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

    private void Postprocess(GenWriteKey key)
    {
        var polys = key.Data.Planet.Polygons.Entities;
        var erodeChance = .75f;
        var mountainNoise = new OpenSimplexNoise();
        mountainNoise.Period = key.Data.Planet.Width;
        foreach (var poly in polys)
        {
            foreach (var tri in poly.Tris.Tris)
            {
                erode(poly, tri);
                irrigate(poly, tri);
                mountainRidging(poly, tri);
            }
        }

        void erode(MapPolygon poly, PolyTri tri)
        {
            if (
                (tri.Landform == LandformManager.Mountain || tri.Landform == LandformManager.Peak)
                && tri.AnyNeighbor(poly, n => n.Landform.IsWater)
                && Game.I.Random.Randf() < erodeChance
            )
            {
                var v = key.Data.Models.Vegetation.GetAtPoint(poly, tri.GetCentroid(), 
                    LandformManager.Hill, _data);
                tri.SetLandform(LandformManager.Hill, key);
                tri.SetVegetation(v, key);
            }
        }

        void irrigate(MapPolygon poly, PolyTri tri)
        {
            if (tri.Landform.IsLand
                && tri.Vegetation.MinMoisture < VegetationManager.Grassland.MinMoisture
                && VegetationManager.Grassland.AllowedLandforms.Contains(tri.Landform)
                && tri.AnyNeighbor(poly, n => n.Landform.IsWater))
            {
                tri.SetVegetation(VegetationManager.Grassland, key);
                tri.ForEachNeighbor(poly, nTri =>
                {
                    if (nTri.Landform.IsLand
                        && nTri.Vegetation.MinMoisture < VegetationManager.Steppe.MinMoisture
                        && VegetationManager.Steppe.AllowedLandforms.Contains(nTri.Landform))
                    {
                        nTri.SetVegetation(VegetationManager.Steppe, key);
                    }
                });
            }
        }

        void mountainRidging(MapPolygon poly, PolyTri tri)
        {
            if (tri.Landform.IsLand && tri.Landform.MinRoughness >= LandformManager.Peak.MinRoughness)
            {
                var globalPos = tri.GetCentroid() + poly.Center;
                var noise = mountainNoise.GetNoise2d(globalPos.x, globalPos.y);
                if(noise < 0f) tri.SetLandform(LandformManager.Mountain, key);
            }
        }
    }
}
