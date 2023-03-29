using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using MessagePack;

public class PolyTris
{
    public PolyTri this[int i] => Tris[i];
    public PolyTri[] Tris;
    // private int _neFirst, _neLast, _seFirst, _seLast, _swFirst, _swLast, _nwFirst, _nwLast;
    public static PolyTris Create(List<PolyTri> tris, GenWriteKey key)
    {
        if (tris.Count > 254) throw new Exception("Too many tris");
        
        for (var i = 0; i < tris.Count; i++)
        {
            tris[i].SetIndex((byte)i, key);
        }
        var ts = new PolyTris(tris.ToArray());
        
        return ts;
    }

    [SerializationConstructor] private PolyTris(PolyTri[] tris)
    {
        Tris = tris;
        // SectionTriStartIndices = sectionTriStartIndices;
        // SectionTriCounts = sectionTriCounts;
    }
    // public PolyTri GetTriAndSection(Vector2 point, out int section)
    // {
    //     return FindByAngle(point, out section);
    // }

    public List<Triangle> GetSectionTris(int section)
    {
        var ts = new List<Triangle>();
        // var start = SectionTriStartIndices[section];
        // var count = SectionTriCounts[section];
        // for (int i = 0; i < count; i++)
        // {
        //     ts.Add(Tris[(start + i) % Tris.Length]);
        // }

        return ts;
    }
    public PolyTri GetAtPoint(Vector2 point, Data data)
    {
        return Tris.FirstOrDefault(t => t.ContainsPoint(point));
        
        // return FindByAngle(point, out _);
    }
    
    // private PolyTri FindByAngle(Vector2 pos, out int section)
    // {
    //     section = (Mathf.FloorToInt(Vector2.Right.GetCCWAngleTo(pos) / _sectionAngle) + _numSections) % _numSections;
    //     var sectionStart = SectionTriStartIndices[section];
    //     var sectionCount = SectionTriCounts[section];
    //     for (var i = 0; i < sectionCount; i++)
    //     {
    //         var triIndex = (sectionStart + i) % Tris.Length;
    //         if (Tris[triIndex].ContainsPoint(pos)) return Tris[triIndex];
    //     }
    //     
    //     return null;
    // }
}