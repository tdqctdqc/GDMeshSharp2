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
    public byte[] TriNativeNeighbors { get; private set; }
    public Dictionary<byte, PolyTriPosition> TriForeignNeighbors { get; private set; }

    public static PolyTris Create(List<PolyTri> tris, 
        IReadOnlyGraph<PolyTri> graph, 
        GenWriteKey key)
    {
        if (tris.Count > 254) throw new Exception("Too many tris");

        for (var i = 0; i < tris.Count; i++)
        {
            tris[i].SetIndex((byte)i, key);
        }
        
        var triNativeNeighbors = new List<byte>();
        // int neighborStartIter = 0;
        // for (var i = 0; i < tris.Count; i++)
        // {
        //     var tri = tris[i];
        //     tri.SetNeighborStart(neighborStartIter, key);
        //     var neighbors = graph.GetNeighbors(tri);
        //     for (var j = 0; j < neighbors.Count; j++)
        //     {
        //         triNativeNeighbors.Add(neighbors.ElementAt(j).Index);
        //     }
        //
        //     neighborStartIter += neighbors.Count;
        // }
        
        var ts = new PolyTris(tris.ToArray(), triNativeNeighbors.ToArray(), new Dictionary<byte, PolyTriPosition>());
        
        return ts;
    }

    [SerializationConstructor] private PolyTris(PolyTri[] tris,
        byte[] triNativeNeighbors, Dictionary<byte, PolyTriPosition> triForeignNeighbors)
    {
        Tris = tris;
        TriNativeNeighbors = triNativeNeighbors;
        TriForeignNeighbors = triForeignNeighbors;
    }

    public PolyTri GetAtPoint(Vector2 point, Data data)
    {
        return Tris.FirstOrDefault(t => t.ContainsPoint(point));
    }
}