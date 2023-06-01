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

    public static PolyTris Create(List<PolyTri> tris, 
        GenWriteKey key)
    {
        if (tris.Count == 0) throw new Exception();
        if (tris.Count > 254) throw new Exception("Too many tris");
        

        for (var i = 0; i < tris.Count; i++)
        {
            tris[i].SetIndex((byte)i, key);
        }
        
        
        
        var ts = new PolyTris(tris.ToArray(), new byte[0]);
        
        ts.SetNeighbors(key);
        
        return ts;
    }

    [SerializationConstructor] private PolyTris(PolyTri[] tris,
        byte[] triNativeNeighbors)
    {
        Tris = tris;
        TriNativeNeighbors = triNativeNeighbors;
    }

    public PolyTri GetAtPoint(Vector2 point, Data data)
    {
        return Tris.FirstOrDefault(t => t.ContainsPoint(point));
    }

    private void SetNeighbors(GenWriteKey key)
    {
        var dic = new Dictionary<Vector2, LinkedList<int>>();
        var graph = new Graph<PolyTri, bool>();
            
        for (var i = 0; i < Tris.Length; i++)
        {
            var tri = Tris[i];
            graph.AddNode(tri);
            tri.ForEachPoint(p =>
            {
                if (dic.ContainsKey(p) == false)
                {
                    dic.Add(p, new LinkedList<int>());
                }
                foreach (var j in dic[p])
                {
                    graph.AddEdge(tri, Tris[j], true);
                }
                dic[p].AddLast(i);
            });
        }
        
        var triNativeNeighbors = new List<byte>();
        int neighborStartIter = 0;
        for (var i = 0; i < Tris.Length; i++)
        {
            var tri = Tris[i];
            tri.SetNeighborStart(neighborStartIter, key);
            var neighbors = graph.GetNeighbors(tri);
            tri.SetNeighborCount(neighbors.Count, key);
        
            for (var j = 0; j < neighbors.Count; j++)
            {
                triNativeNeighbors.Add(neighbors.ElementAt(j).Index);
            }
            neighborStartIter += neighbors.Count;
        }

        TriNativeNeighbors = triNativeNeighbors.ToArray();
    }
}