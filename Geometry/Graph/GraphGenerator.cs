using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;

public static class GraphGenerator
{
    public static Graph<MapPolygon, LineSegment> GenerateMapPolyVoronoiGraph
        (MapGenInfo info, IdDispenser id, GenWriteKey key)
    {
        var g = GenerateVoronoiGraph<MapPolygon, LineSegment>(
            info.Polys,
            mp => mp.Center,
            (v1, v2, mp1, mp2) =>
            {
                return new LineSegment(v1, v2);
            },
            new Vector2(key.GenData.Planet.Width, key.GenData.Planet.Height)
        );
        return g;
    }

    public static void WrapMapPolygonGraph(Graph<MapPolygon, LineSegment> graph,
        List<MapPolygon> keepMergePolys, List<MapPolygon> discardMergePolys, 
        GenWriteKey key)
    {
        for (var i = 0; i < keepMergePolys.Count - 1; i++)
        {
            var keep = keepMergePolys[i];
            var discard = discardMergePolys[i];
            var nextKeep = keepMergePolys[i + 1];
            var nextDiscard = discardMergePolys[i + 1];
            var discardEdge = graph.GetEdge(discard, nextDiscard);
            var keepEdge = graph.GetEdge(keep, nextKeep);
            var newPoints = new List<Vector2> {keepEdge.From, keepEdge.To, discardEdge.From, discardEdge.To}
                .Where(p => p.x != 0 
                            && p.x < key.Data.Planet.Width
                            && p.y <= key.Data.Planet.Height).ToList();
            if (newPoints.Count > 1 && newPoints[0] != newPoints[1])
            {
                graph.SetEdgeValue(keep, nextKeep, new LineSegment(newPoints[0], newPoints[1]));
            }
        }
        WrapVoronoiGraph<MapPolygon, LineSegment>(
            graph, keepMergePolys, discardMergePolys, 
            new MapPolygonEdgeConverter(key)
        );
        var check = new List<MapPolygon>(keepMergePolys);
        var discardHash = discardMergePolys.ToHashSet();
        discardMergePolys.ForEach(discard =>
        {
            var ns = discard.Neighbors.Refs().ToList();
            for (var i = ns.Count - 1; i >= 0; i--)
            {
                var discardN = ns[i];
                check.Add(discardN);
                var border = discard.GetBorder(discardN, key.Data);
                discardN.RemoveNeighbor(discard, key);
                discard.RemoveNeighbor(discardN, key);
                key.Delete(border);
            }
            key.Delete(discard);
        });
        check.ForEach(n =>
        {
            var badNs = n.Neighbors.Refs().Intersect(discardHash).ToList();
            foreach (var badN in badNs)
            {
                n.RemoveNeighbor(badN, key);
            }
        });
    }
    public static Graph<TNode, TEdge> GenerateVoronoiGraph<TNode, TEdge>
    (List<TNode> elements, Func<TNode, Vector2> posFunc,
        Func<Vector2, Vector2, TNode, TNode, TEdge> getEdgeFunc, Vector2 bounds) where TNode : class
    {
        var graph = new Graph<TNode, TEdge>();
        elements.ForEach(e => graph.AddNode(e));
        var iPoints = elements.Select(posFunc).Select(v => v.GetIPoint()).ToArray();
        var d = new Delaunator(iPoints);
        var voronoiCells = d.GetVoronoiCells().ToList();

        var pointElements = new Dictionary<IPoint, TNode>();
        var elementCells = new Dictionary<TNode, IVoronoiCell>();
        var cellElements = new Dictionary<IVoronoiCell, TNode>();
        for (var i = 0; i < elements.Count; i++)
        {
            elementCells.Add(elements[i], voronoiCells[i]);
            cellElements.Add(voronoiCells[i], elements[i]);
            pointElements.Add(iPoints[i], elements[i]);
        }
        d.ForEachTriangleEdge(edge =>
        {
            var tri = Mathf.FloorToInt(edge.Index / 3);
            var circum = d.GetTriangleCircumcenter(tri);
            var p1 = edge.P;
            var p2 = edge.Q;
            var el1 = pointElements[p1];
            var el2 = pointElements[p2];
            if (graph.HasEdge(el1, el2)) return;
            if (d.Halfedges[edge.Index] != -1)
            {
                var oppEdgeIndex = d.Halfedges[edge.Index];
                var oppTri = Mathf.FloorToInt(oppEdgeIndex / 3);
                var oppCircum = d.GetTriangleCircumcenter(oppTri);
                var p = circum.GetIntV2();
                var oP = oppCircum.GetIntV2();

                if (p.y >= bounds.y) p.y = bounds.y;
                if (oP.y >= bounds.y) oP.y = bounds.y;
                if (p.x >= bounds.x) p.x = bounds.x;
                if (oP.x >= bounds.x) oP.x = bounds.x;
                if (p == oP)
                {
                    GD.Print("degenerate");
                    return;
                }
                var tEdge = getEdgeFunc(p, oP, el1, el2);
                graph.AddEdge(el1, el2, tEdge);
            }
            else
            {
                var secondPoint = ((edge.P.GetIntV2() + edge.Q.GetIntV2()) / 2f).Intify();
                var tEdge = getEdgeFunc(circum.GetIntV2(), secondPoint, el1, el2);
                graph.AddEdge(el1, el2, tEdge);
            }
        });
        
        return graph;
    }

    private class MapPolygonEdgeConverter : EdgeConverter<MapPolygon, LineSegment>
    {
        public MapPolygonEdgeConverter(GenWriteKey key)
            : base((discard, discardNeighbor, keep, oldEdge) =>
                {
                    discardNeighbor.RemoveNeighbor(discard, key);
                    discard.RemoveNeighbor(discardNeighbor, key);
                    return oldEdge;
                }
            )
        {
            
        }
    }
    private class EdgeConverter<TNode, TEdge>
    {
        private Func<TNode, TNode, TNode, TEdge, TEdge> _convertEdge;

        public EdgeConverter(Func<TNode, TNode, TNode, TEdge, TEdge> convertEdge)
        {
            _convertEdge = convertEdge;
        }

        public TEdge Convert(TNode discard, TNode discardNeighbor, TNode keep, TEdge oldEdge)
        {
            return _convertEdge(discard, discardNeighbor, keep, oldEdge);
        }
    }
    private static void WrapVoronoiGraph<TNode, TEdge>(Graph<TNode, TEdge> graph, 
        List<TNode> wrapKeep, List<TNode> wrapDiscard,
        EdgeConverter<TNode, TEdge> edgeConverter)
    {
        for (var i = 0; i < wrapKeep.Count; i++)
        {
            var keep = wrapKeep[i];
            var discard = wrapDiscard[i];
            var discardNeighbors = graph.GetNeighbors(discard);
            for (var j = 0; j < discardNeighbors.Count; j++)
            {
                var discardNeighbor = discardNeighbors[j];
                var oldEdge = graph.GetEdge(discard, discardNeighbor);
                var newEdge = edgeConverter.Convert(discard, discardNeighbor, keep, oldEdge);
                graph.AddEdge(keep, discardNeighbor, newEdge);
            }
        }
        
        wrapDiscard.ForEach(discard =>
        {
            graph.Remove(discard);
        });
    }
    
    
    
    public static Graph<TNode, TEdge> GenerateDelaunayGraph<TNode, TEdge>
        (List<TNode> elements, Func<TNode, Vector2> posFunc,
            Func<TNode,TNode,TEdge> getEdgeFunc)
    {
        var graph = new Graph<TNode, TEdge>();
        var poses = new List<Vector2>();
        for (int i = 0; i < elements.Count; i++)
        {
            poses.Add(posFunc(elements[i]));
            var node = new GraphNode<TNode, TEdge>(elements[i]);
            graph.AddNode(node);
        }
        var tris = DelaunayTriangulator.TriangulatePoints(poses);
        for (int i = 0; i < tris.Count; i++)
        {
            var t = tris[i];
            var elementA = elements[poses.IndexOf(t.A)];
            var a = graph[elementA];
            
            var elementB = elements[poses.IndexOf(t.B)];
            var b = graph[elementB];
            
            var elementC = elements[poses.IndexOf(t.C)];
            var c = graph[elementC];

            graph.AddUndirectedEdge(a, b, getEdgeFunc(a.Element, b.Element));
            graph.AddUndirectedEdge(a, c, getEdgeFunc(a.Element, c.Element));
            graph.AddUndirectedEdge(c, b, getEdgeFunc(c.Element,b.Element));
        }

        return graph;
    }
    public static Graph<Vector2, float> GenerateDelaunayGraph(List<Vector2> poses)
    {
        var graph = new Graph<Vector2, float>();

        foreach (var pos in poses)
        {
            var node = new GraphNode<Vector2, float>(pos);
            graph.AddNode(node);
        }
        var tris = DelaunayTriangulator.TriangulatePoints(poses);
        for (int i = 0; i < tris.Count; i++)
        {
            var a = graph[tris[i].A];
            var b = graph[tris[i].B];
            var c = graph[tris[i].C];

            graph.AddUndirectedEdge(a,b,1f);
            graph.AddUndirectedEdge(a,c,1f);
            graph.AddUndirectedEdge(c,b,1f);
        }

        return graph;
    }
    public static Graph<Vector2, float> GenerateSpanningGraph(Graph<Vector2, float> delaunayGraph,
                                                        int branchLength,
                                                        int maxVertexDegree)
    {
        var graph = new Graph<Vector2, float>();
        foreach (var e in delaunayGraph.Elements)
        {
            graph.AddNode(e);
        }
        var openNodes = new List<GraphNode<Vector2, float>>(graph.Nodes);

        while(openNodes.Count > 0)
        {
            var node = openNodes[0];
            var branchNodes = new List<GraphNode<Vector2, float>>();
            int branch = 0;
            branchNodes.Add(node);
            openNodes.Remove(node);
            
            while(branch < branchLength)
            {
                var backingNode = delaunayGraph[node.Element];
                var openNs = backingNode.Neighbors
                    .Where(n => 
                        branchNodes.Contains(delaunayGraph[n]) == false
                    )
                    .Select(n => delaunayGraph[n]);
                if(openNs.Count() == 0) break;
                var next = openNs.ElementAt(0);
                graph.AddUndirectedEdge(node, next, 1f);
                node = next;
                branch++;
                branchNodes.Add(node);
                openNodes.Remove(node);
            }
        }
        return graph; 
    }
}
