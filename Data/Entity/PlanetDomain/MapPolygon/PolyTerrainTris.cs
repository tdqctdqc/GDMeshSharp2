using System;
using System.Collections.Generic;
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

    public static PolyTerrainTris Construct(List<Triangle> tris)
    {
        if (tris.Count > byte.MaxValue - 1) throw new Exception();
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
                t.InSection(startRot, endRot)
            ).ToHashSet();
            return ts;
        }).ToList();

        var orderedTris = new List<Triangle>();
        for (var i = 0; i < _numSections; i++)
        {
            var section = sectionTris[i];
            GD.Print("section length " + section.Count);
            var prev = sectionTris[(_numSections + i - 1) % _numSections];
            var next = sectionTris[(i + 1) % _numSections];
            var sharedWPrev = section.Intersect(prev);
            var sharedWNext = section.Intersect(next);
            var exclusive = (section.Where(t => prev.Contains(t) == false && next.Contains(t) == false));
            
            var currCount = orderedTris.Count;
            sectionTriStartIndices[i] = Convert.ToByte(currCount);
            sectionTriCounts[i] = Convert.ToByte(section.Count());
            
            orderedTris.AddRange(sharedWPrev);
            orderedTris.AddRange(exclusive);
        }


        if (orderedTris.Count != tris.Count)
        {
            GD.Print($"{tris.Count} tris {orderedTris.Count} ordered");
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
        return new PolyTerrainTris(vertices.ToArray(), polyTris, sectionTriStartIndices, sectionTriCounts);
    }

    private int FindByAngle(Vector2 pos, out int section)
    {
        section = (Mathf.FloorToInt(pos.Angle() / _sectionAngle) + _numSections) % _numSections;
        var sectionStart = SectionTriStartIndices[section];
        var sectionCount = SectionTriCounts[section];
        
        for (var i = 0; i < sectionCount; i++)
        {
            var triIndex = (sectionStart + i) %  Tris.Length;
            if (Tris[triIndex].ContainsPoint(pos, Vertices)) return triIndex;
        }
        return -1;
    }
    private bool ContainsPoint(int tIndex, Vector2 p, Vector2[] vertices)
    {
        var t = Tris[tIndex];
        var t1 = vertices[t.A];
        var t2 = vertices[t.B];
        var t3 = vertices[t.C];
        var d1 = (p.x - t2.x) * (t1.y - t2.y) - (t1.x - t2.x) * (p.y - t2.y);
        var d2 = (p.x - t3.x) * (t2.y - t3.y) - (t2.x - t3.x) * (p.y - t3.y);
        var d3 = (p.x - t1.x) * (t3.y - t1.y) - (t3.x - t1.x) * (p.y - t1.y);

        return !((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0));
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
        GD.Print("vertices " + vertices.Length);
        Tris = tris;
        GD.Print("tris " + tris.Length);

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
}