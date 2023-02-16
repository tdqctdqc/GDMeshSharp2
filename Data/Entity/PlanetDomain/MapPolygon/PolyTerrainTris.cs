using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using MessagePack;

public class PolyTerrainTris
{
    public Vector2[] Vertices;
    public PolyTri[] Tris;
    private static int _numSections = 4; //could be by how many tris
    private static float _sectionAngle => Mathf.Pi * 2f / _numSections;
    
    
    
    public int[] SectionTriStartIndices;
    public int[] SectionTriCounts;
    // private List<HashSet<int>> _sections;

    public static PolyTerrainTris MakeWheel(MapPolygon poly, GenData data)
    {
        var segs = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, data)).SelectMany(b => b.GetSegsRel(poly))
            .ToList();
        var tris = new List<PolyTri>();
        for (var i = 0; i < segs.Count; i++)
        {
            tris.Add(new PolyTri(segs[i].From, segs[i].To, Vector2.Zero, 
                
                poly.IsLand() ? LandformManager.Plain : LandformManager.Sea, 
                
                VegetationManager.Barren));
        }

        return Construct(tris, data);
    }
    public static PolyTerrainTris MakeGeneric(MapPolygon poly, GenData data)
    {
        var segs = poly.Neighbors.Refs().Select(n => poly.GetBorder(n, data)).SelectMany(b => b.GetSegsRel(poly))
            .ToList();

        var ps = segs.GenerateInteriorPoints(50f, 25f);
        ps.AddRange(segs.GetPoints());


        var tris = DelaunayTriangulator.TriangulatePoints(ps).Select(t => new PolyTri(t.A, t.B, t.C,
            poly.IsLand() ? LandformManager.Plain : LandformManager.Sea, VegetationManager.Barren))
            .ToList();

        return Construct(tris, data);
    }
    public static PolyTerrainTris MakeFromSegments(MapPolygon poly, GenData data)
    {
        StopwatchMeta.TryStart("make poly terrain tris");
        var borderSegsRel = poly.GetAllBorderSegmentsClockwise(data);

        var riverBs = poly.Neighbors.Refs()
            .Select(n => poly.GetBorder(n, data))
            .Where(b => b.MoistureFlow > 0f);
        var riverPointsRel = riverBs
            .Select(b => b.GetSegsRel(poly).GetMiddlePoint()).ToList();
        var riverWidths = riverBs.Select(b => b.MoistureFlow / 10f).ToList();
        
        
        PolyTerrainTris tris = null;
        
        if (poly.IsWater())
        {
            tris = Sea(borderSegsRel, data);
        }
        // else if (riverPointsRel.Count == 1)
        // {
        //     var brokenSegs = borderSegsRel.InsertOnPoints(riverPointsRel, riverWidths, out var riverSegs);
        //     tris = RiverSource(poly, brokenSegs, riverSegs, data);
        // }
        // else if (riverPointsRel.Count > 1)
        // {        
        //     var brokenSegs = borderSegsRel.InsertOnPoints(riverPointsRel, riverWidths, out var riverSegs);
        //     tris = RiverJunction(poly, brokenSegs, riverSegs, data);
        // }
        else
        {
            tris = LandNoRivers(poly, borderSegsRel, data);
        }
        StopwatchMeta.TryStop("make poly terrain tris");

        return tris;
    }
    private static PolyTerrainTris RiverSource(MapPolygon poly, 
        List<LineSegment> borderSegsRel, HashSet<LineSegment> riverSegs,
        GenData data)
    {
        var lf = data.Models.Landforms;
        var v = data.Models.Vegetation;
        var riverSeg = riverSegs.First();
        var riverSegIndex = borderSegsRel.IndexOf(riverSeg);

        var nonRiverSegs = new List<LineSegment>();
        for (var i = 1; i < borderSegsRel.Count; i++)
        {
            var index = (i + riverSegIndex) % borderSegsRel.Count;
            nonRiverSegs.Add(borderSegsRel[index]);
        }
        
        var riverTri = new PolyTri(riverSeg.From, riverSeg.To, Vector2.Zero);
        riverTri.Landform = LandformManager.River;
        riverTri.Vegetation = VegetationManager.Barren;
        
        
        var tris = new List<PolyTri>();
        tris.Add(riverTri);
        var segTris = nonRiverSegs.TriangulateSegment(
            new LineSegment(Vector2.Zero, riverSeg.To),
            new LineSegment(riverSeg.From, Vector2.Zero));
        var segPolyTris = new List<PolyTri>();
        for (int i = 0; i < segTris.Count; i++)
        {
            var t = segTris[i];
            var pt = new PolyTri(t.A, t.B, t.C);
            pt.Landform = lf.GetAtPoint(poly, pt.GetCentroid(), data);
            pt.Vegetation = v.GetAtPoint(poly, pt.GetCentroid(), pt.Landform, data);
                segPolyTris.Add(pt);
        }
        
        tris.AddRange(segPolyTris);
        
        return Construct(tris, data);
    }

    private static PolyTerrainTris RiverJunction(MapPolygon poly,
        List<LineSegment> borderSegsRel, HashSet<LineSegment> riverSegs, 
        Data data)
    {
        var lf = data.Models.Landforms;
        var v = data.Models.Vegetation;
        
        var clockwise = riverSegs.ToList();
        clockwise.OrderByClockwise(Vector2.Zero, s => s.From);
        
        var riverIndices = clockwise
            .Select(s => borderSegsRel.IndexOf(s))
            .ToList();

        var tris = new List<PolyTri>();
        var junctionPoints = new List<Vector2>();
        for (var i = 0; i < riverIndices.Count; i++)
        {

            var riverIndex = riverIndices[i];
            var prevRiverIndex = riverIndices[(i + 1) % riverIndices.Count];
            var nextRiverIndex = riverIndices[(i - 1 + riverIndices.Count) % riverIndices.Count];
            
            var seg = borderSegsRel[riverIndex];
            var nextRSeg = borderSegsRel[nextRiverIndex];
            var prevRSeg = borderSegsRel[prevRiverIndex];

            
            var prevIntersect = GetIntersection(riverIndex, prevRiverIndex);
            var nextIntersect = GetIntersection(nextRiverIndex, riverIndex);

            junctionPoints.Add(nextIntersect);

            tris.Add(new PolyTri(seg.From, prevIntersect, nextIntersect, 
                LandformManager.River, VegetationManager.Barren));
            
            tris.Add(new PolyTri(seg.To, seg.From, nextIntersect,
                 LandformManager.River, VegetationManager.Barren));

            var thisNonRiverSegs = new List<LineSegment>();
            int iter = (riverIndex + 1) % borderSegsRel.Count;
            
            while (iter != nextRiverIndex)
            {
                thisNonRiverSegs.Add(borderSegsRel[iter]);
                iter++;
                iter = iter % borderSegsRel.Count;
            }

            
            var segTris = thisNonRiverSegs.TriangulateSegment(
                new LineSegment(nextIntersect, seg.To),
                new LineSegment(nextRSeg.From, nextIntersect)
                ).Select(t =>
            {
                var land = lf.GetAtPoint(poly, t.GetCentroid(), data);
                var veg = v.GetAtPoint(poly, t.GetCentroid(), land, data);
                return new PolyTri(t.A, t.B, t.C, land, veg);
            });

            tris.AddRange(segTris);
        }


        Vector2 GetIntersection(int fromIndex, int toIndex)
        {

            var to = borderSegsRel[fromIndex];
            var from = borderSegsRel[toIndex];
            if (to.Mid().Normalized() == -from.Mid().Normalized())
            {
                if (from.Mid() == Vector2.Zero) throw new Exception();
                if (to.Mid() == Vector2.Zero) throw new Exception();
                return to.From.LinearInterpolate(from.To,
                    from.Mid().Length() / (from.Mid().Length() + to.Mid().Length()));
            }
            
            var has = Vector2Ext.GetLineIntersection(to.From, to.From - to.Mid(),
                from.To, from.To - from.Mid(), 
                out var intersect);

            if (Mathf.IsNaN(intersect.x) || Mathf.IsNaN(intersect.y)) throw new Exception();
            

            return intersect;
        }

        for (var i = 1; i < junctionPoints.Count - 1; i++)
        {
            tris.Add(new PolyTri(junctionPoints[0], junctionPoints[i], junctionPoints[i + 1],
                LandformManager.River, VegetationManager.Barren));
        }

        return Construct(tris, data);
    }
    private static PolyTerrainTris Sea(List<LineSegment> borderSegsRel, Data data)
    {
        var points = borderSegsRel.GetPoints().ToList();
        var tris = new List<PolyTri>();
        var anchor = borderSegsRel.First().From;
        for (var i = 1; i < borderSegsRel.Count - 1; i++)
        {
            tris.Add( new PolyTri(borderSegsRel[i].From, borderSegsRel[i].To, anchor, LandformManager.Sea, VegetationManager.Barren));
        }
        return Construct(tris, data);
    }
    private static PolyTerrainTris LandNoRivers(MapPolygon poly, List<LineSegment> borderSegsRel, Data data)
    {
        var lf = data.Models.Landforms;
        var v = data.Models.Vegetation;
        var points = borderSegsRel.GenerateInteriorPoints(50f, 10f)
            .ToList();
        points.AddRange(borderSegsRel.GetPoints());
        borderSegsRel.ForEach(b => b.GeneratePointsAlong(50f, 10f, points));
        
        var tris = DelaunayTriangulator.TriangulatePoints(points)
            .Select(t =>
            {
                var land = lf.GetAtPoint(poly, t.GetCentroid(), data);
                var veg = v.GetAtPoint(poly, t.GetCentroid(), land, data);
                return new PolyTri(t.A, t.B, t.C, land, veg);
            })
            .ToList();
        return Construct(tris, data);
    }
    
    public static PolyTerrainTris Construct(List<PolyTri> tris, 
        Data data)
    {
        var vertexIndices = new Dictionary<Vector2, int>();
        var vertices = new List<Vector2>();
        var triVertexIndices = new HashSet<Vector3>();

        var sectionTriStartIndices = new int[_numSections];
        var sectionTriCounts = new int[_numSections];
        
        
        var sectionTris = Enumerable.Range(0, _numSections)
        .Select(i =>
        {
            var startRot = Vector2.Right.Rotated(_sectionAngle * i);
            var endRot = Vector2.Right.Rotated(_sectionAngle * (i + 1));
            return tris.Where(t => t.InSection(startRot, endRot)).ToHashSet();
        }).ToList();

        var orderedTris = new List<PolyTri>();
        for (var i = 0; i < _numSections; i++)
        {
            var section = sectionTris[i];
            var prev = sectionTris[(_numSections + i - 1) % _numSections];
            var next = sectionTris[(i + 1) % _numSections];
            var sharedWPrev = section.Intersect(prev).Distinct();
            var sharedWNext = section.Intersect(next).Distinct();
            var exclusive = section.Except(sharedWNext).Except(sharedWPrev).Distinct();

            var currCount = orderedTris.Count;
            sectionTriStartIndices[i] = currCount;
            sectionTriCounts[i] = sharedWPrev.Count() + exclusive.Count() + sharedWNext.Count();
            
            orderedTris.AddRange(sharedWPrev);
            orderedTris.AddRange(exclusive);
        }

        

        return new PolyTerrainTris(vertices.ToArray(), orderedTris.ToArray(), sectionTriStartIndices, sectionTriCounts);
    }

    
    [SerializationConstructor] 
    private PolyTerrainTris(Vector2[] vertices, PolyTri[] tris, int[] sectionTriStartIndices, 
        int[] sectionTriCounts)
    {
        Vertices = vertices;
        Tris = tris;

        SectionTriStartIndices = sectionTriStartIndices;
        SectionTriCounts = sectionTriCounts;
    }

    public PolyTri GetTriAndSection(Vector2 point, out int section)
    {
        return FindByAngle(point, out section);
    }

    public List<Triangle> GetSectionTris(int section)
    {
        var ts = new List<Triangle>();
        var start = SectionTriStartIndices[section];
        var count = SectionTriCounts[section];
        for (int i = 0; i < count; i++)
        {
            ts.Add(Tris[(start + i) % Tris.Length]);
        }

        return ts;
    }
    public PolyTri GetAtPoint(Vector2 point, Data data)
    {
        return (FindByAngle(point, out _));
    }
    
    private PolyTri FindByAngle(Vector2 pos, out int section)
    {
        section = (Mathf.FloorToInt(Vector2.Right.GetClockwiseAngleTo(pos) / _sectionAngle) + _numSections) % _numSections;
        var sectionStart = SectionTriStartIndices[section];
        var sectionCount = SectionTriCounts[section];
        for (var i = 0; i < sectionCount; i++)
        {
            var triIndex = (sectionStart + i) % Tris.Length;
            if (Tris[triIndex].ContainsPoint(pos)) return Tris[triIndex];
        }
        return null;
    }
}