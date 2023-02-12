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
    
    
    
    public byte[] SectionTriStartIndices;
    public byte[] SectionTriCounts;
    // private List<HashSet<int>> _sections;


    public static PolyTerrainTris MakeFromSegments(List<LineSegment> borderSegsRel, List<Vector2> riverPointsRel, List<float> riverWidths)
    {
        PolyTerrainTris tris = null;
        var sw = new Stopwatch();
        sw.Start();
        if (riverPointsRel.Count == 1)
        {
            var brokenSegs = borderSegsRel.InsertOnPoints(riverPointsRel, riverWidths, out var riverSegs);
            tris = RiverSource(brokenSegs, riverSegs);
        }
        else if (riverPointsRel.Count > 1)
        {        
            // StopwatchMeta.TryStart("breaking segs");
            var brokenSegs = borderSegsRel.InsertOnPoints(riverPointsRel, riverWidths, out var riverSegs);
            StopwatchMeta.TryStop("breaking segs");

            

            tris = RiverJunction(brokenSegs, riverSegs);
        }
        else
        {
            tris = NoRivers(borderSegsRel);
        }
        sw.Stop();
        GD.Print("\t poly terrain tri construction time " + sw.Elapsed.TotalMilliseconds);
        return tris;
    }
    private static PolyTerrainTris RiverSource(List<LineSegment> borderSegsRel, HashSet<LineSegment> riverSegs)
    {
        var riverSeg = riverSegs.First();
        var riverSegIndex = borderSegsRel.IndexOf(riverSeg);

        var nonRiverSegs = new List<LineSegment>();
        for (var i = 1; i < borderSegsRel.Count; i++)
        {
            var index = (i + riverSegIndex) % borderSegsRel.Count;
            nonRiverSegs.Add(borderSegsRel[index]);
        }
        
        var riverTri = new Triangle(riverSeg.From, riverSeg.To, Vector2.Zero);

        var tris = new List<Triangle>();

        var segTris = nonRiverSegs.TriangulateSegment(
            new LineSegment(Vector2.Zero, riverSeg.To),
            new LineSegment(riverSeg.From, Vector2.Zero));
        tris.AddRange(segTris);
        tris.Add(riverTri);
        
        return PolyTerrainTris.Construct(tris);
    }

    private static PolyTerrainTris RiverJunction(List<LineSegment> borderSegsRel, HashSet<LineSegment> riverSegs)
    {
        StopwatchMeta.TryStart("river junction");
        StopwatchMeta.TryStart("not triangulating");

        
        StopwatchMeta.TryStart("not1");
        var riverIndices = riverSegs
            .OrderByClockwise(Vector2.Zero, s => s.From)
            .Select(s => borderSegsRel.IndexOf(s))
            
            .ToList();
        StopwatchMeta.TryStop("not1");


        var tris = new List<Triangle>();
        var junctionPoints = new List<Vector2>();
        for (var i = 0; i < riverIndices.Count; i++)
        {
            StopwatchMeta.TryStart("not2");

            var riverIndex = riverIndices[i];
            var prevRiverIndex = riverIndices[(i + 1) % riverIndices.Count];
            var nextRiverIndex = riverIndices[(i - 1 + riverIndices.Count) % riverIndices.Count];
            
            var seg = borderSegsRel[riverIndex];
            var nextRSeg = borderSegsRel[nextRiverIndex];
            var prevRSeg = borderSegsRel[prevRiverIndex];

            
            var prevIntersect = GetIntersection(riverIndex, prevRiverIndex);
            var nextIntersect = GetIntersection(nextRiverIndex, riverIndex);

            junctionPoints.Add(nextIntersect);

            tris.Add(new Triangle(seg.From, prevIntersect, nextIntersect));
            tris.Add(new Triangle(seg.To, seg.From, nextIntersect));

            var thisNonRiverSegs = new List<LineSegment>();
            int iter = (riverIndex + 1) % borderSegsRel.Count;
            
            while (iter != nextRiverIndex)
            {
                thisNonRiverSegs.Add(borderSegsRel[iter]);
                iter++;
                iter = iter % borderSegsRel.Count;
            }
            StopwatchMeta.TryStop("not2");

            StopwatchMeta.TryStop("not triangulating");

            
            var segTris = thisNonRiverSegs.TriangulateSegment(
                new LineSegment(nextIntersect, seg.To),
                new LineSegment(nextRSeg.From, nextIntersect)
                );

            StopwatchMeta.TryStart("not triangulating");

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
            var tri = new Triangle(junctionPoints[0], junctionPoints[i], junctionPoints[i + 1]);
            tris.Add(tri);
        }

        
        
        StopwatchMeta.TryStop("not triangulating");
        StopwatchMeta.TryStop("river junction");

        return Construct(tris);
    }

    private static PolyTerrainTris NoRivers(List<LineSegment> borderSegsRel)
    {
        var points = borderSegsRel.GenerateInteriorPoints(30f, 20f)
            .ToList();
        points.AddRange(borderSegsRel.GetPoints());
        var tris = DelaunayTriangulator.TriangulatePoints(points);
        
        return Construct(tris);
    }
    
    public static PolyTerrainTris Construct(List<Triangle> tris)
    {
        StopwatchMeta.TryStart("construct");
        if (tris.Count > byte.MaxValue - 1)
        {
            throw new Exception($"{tris.Count} is too many tris");
        }
        var vertexIndices = new Dictionary<Vector2, int>();
        var vertices = new List<Vector2>();
        var triVertexIndices = new HashSet<Vector3>();

        var sectionTriStartIndices = new byte[_numSections];
        var sectionTriCounts = new byte[_numSections];
        
        var sectionTris = Enumerable.Range(0, _numSections)
        .Select(i =>
        {
            // var centerRot = Vector2.Right.Rotated(_sectionAngle * (i + .5f)) * 10000f;
            var startRot = Vector2.Right.Rotated(_sectionAngle * i);
            var endRot = Vector2.Right.Rotated(_sectionAngle * (i + 1));
            
            var ts = tris.Where(t =>
                {
                    return t.InSection(startRot, endRot);
                }
            ).ToHashSet();
            return ts;
        }).ToList();

        var orderedTris = new List<Triangle>();
        for (var i = 0; i < _numSections; i++)
        {
            var section = sectionTris[i];
            var prev = sectionTris[(_numSections + i - 1) % _numSections];
            var next = sectionTris[(i + 1) % _numSections];
            var sharedWPrev = section.Intersect(prev).Distinct();
            var sharedWNext = section.Intersect(next).Distinct();
            var exclusive = section.Except(sharedWNext).Except(sharedWPrev).Distinct();

            var currCount = orderedTris.Count;
            sectionTriStartIndices[i] = Convert.ToByte(currCount);
            sectionTriCounts[i] = Convert.ToByte(sharedWPrev.Count() + exclusive.Count() + sharedWNext.Count());
            
            orderedTris.AddRange(sharedWPrev);
            orderedTris.AddRange(exclusive);
        }


        if (orderedTris.Count != tris.Count)
        {
            // GD.Print($"{tris.Count} tris {orderedTris.Count} ordered");
            // throw new Exception();
        }

        
        var polyTris = new PolyTri[orderedTris.Count];
        for (var i = 0; i < orderedTris.Count; i++)
        {
            var tri = orderedTris[i];
            
            var polyTri = new PolyTri();
            polyTris[i] = polyTri;
            int triIter = 0;

            tri.DoForEachPoint(p =>
            {
                int vertexIndex;
                if (vertexIndices.ContainsKey(p) == false)
                {
                    vertexIndex = vertexIndices.Count;
                    vertexIndices.Add(p, vertexIndex);
                    vertices.Add(p);
                }
                else
                {
                    vertexIndex = vertexIndices[p];
                }
                polyTri.Set(triIter++, (byte)vertexIndex);
            });
        }

        if (vertices.Count > byte.MaxValue - 1) throw new Exception();
        
        
        StopwatchMeta.TryStop("construct");

        return new PolyTerrainTris(vertices.ToArray(), polyTris, sectionTriStartIndices, sectionTriCounts);
    }

    private int FindByAngle(Vector2 pos, out int section)
    {
        section = (Mathf.FloorToInt(Vector2.Right.GetClockwiseAngleTo(pos) / _sectionAngle) + _numSections) % _numSections;
        var sectionStart = SectionTriStartIndices[section];
        var sectionCount = SectionTriCounts[section];
        for (var i = 0; i < sectionCount; i++)
        {
            var triIndex = (sectionStart + i) % Tris.Length;
            if (Tris[triIndex].ContainsPoint(pos, Vertices)) return triIndex;
        }
        return -1;
    }

    
    public List<Triangle> GetTris()
    {
        return Tris.Select(pt => pt.GetTriangle(Vertices)).ToList();
    }
    [SerializationConstructor] 
    private PolyTerrainTris(Vector2[] vertices, PolyTri[] tris, byte[] sectionTriStartIndices, 
        byte[] sectionTriCounts)
    {
        Vertices = vertices;
        Tris = tris;

        SectionTriStartIndices = sectionTriStartIndices;
        SectionTriCounts = sectionTriCounts;
    }

    public int IntersectingTri(Vector2 point, out int section)
    {
        return FindByAngle(point, out section);
    }

    public Triangle GetTriangle(int index)
    {
        return Tris[index].GetTriangle(Vertices);
    }

    public List<Triangle> GetSectionTris(int section)
    {
        var ts = new List<Triangle>();
        var start = SectionTriStartIndices[section];
        var count = SectionTriCounts[section];
        for (int i = 0; i < count; i++)
        {
            ts.Add(Tris[(start + i) % Tris.Length].GetTriangle(Vertices));
        }

        return ts;
    }
}