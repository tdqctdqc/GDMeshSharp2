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