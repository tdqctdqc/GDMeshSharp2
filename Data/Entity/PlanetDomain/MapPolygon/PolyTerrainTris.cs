using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using MessagePack;

public class PolyTerrainTris
{
    public PolyTri[] Tris;
    private static int _numSections = 4; //could be by how many tris
    private static float _sectionAngle => Mathf.Pi * 2f / _numSections;
    public int[] SectionTriStartIndices;
    public int[] SectionTriCounts;
    //can do with bitmask? max 200 tris in poly, so 40000 possible arrangements
    public Dictionary<PolyTri, HashSet<PolyTri>> NeighborsInside { get; private set; }
    public Dictionary<PolyTri, Tuple<PolyTri, MapPolygon>> FirstNeighborOutside { get; private set; }
    public Dictionary<PolyTri, Tuple<PolyTri, MapPolygon>> SecondNeighborOutside { get; private set; }
    
    public static PolyTerrainTris Construct(MapPolygon poly, List<PolyTri> tris, 
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

        return new PolyTerrainTris(poly, orderedTris.ToArray(), sectionTriStartIndices, sectionTriCounts);
    }

    private static int _maxTris;
    [SerializationConstructor] 
    private PolyTerrainTris(MapPolygon poly, PolyTri[] tris, int[] sectionTriStartIndices, 
        int[] sectionTriCounts)
    {
        Tris = tris;
        if (tris.Distinct().Count() > _maxTris)
        {
            _maxTris = tris.Distinct().Count();
            GD.Print($"most poly tris " + _maxTris + " at " + poly.Id);
        }
        SectionTriStartIndices = sectionTriStartIndices;
        SectionTriCounts = sectionTriCounts;
        var sw = new Stopwatch();
        sw.Start();
        ConstructNetwork();
        sw.Stop();
    }

    private void ConstructNetwork()
    {
        NeighborsInside = new Dictionary<PolyTri, HashSet<PolyTri>>();
        var hasEvenNumberNs = new HashSet<PolyTri>();
        foreach (var tri in Tris)
        {
            var ns = Tris
                .Where(n => n.AnyPoint(tri.HasPoint) && tri != n);
            NeighborsInside.AddOrUpdateRange(tri, ns.ToArray());
        }
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
        return FindByAngle(point, out _);
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