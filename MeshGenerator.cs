using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class MeshGenerator 
{
    public static MeshInstance2D GetMeshInstance(List<Vector2> triPoints, IEnumerable<Color> colors = null)
    {
        var mesh = colors != null 
            ? GetArrayMesh(triPoints.ToArray(), colors.ToArray())
            : GetArrayMesh(triPoints.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        return meshInstance;
    }

    public static MeshInstance2D GetBoundingBoxMesh(this BoundingBox bb, float thickness)
    {
        var result = GetLinesMesh(bb.CornerPoints.ToList(), thickness, true);
        result.Modulate = Colors.Red;
        return result;
    }
    public static MeshInstance2D GetLinesMesh(List<Vector2> points,
        float thickness, bool close)
    {
        var triPoints = new List<Vector2>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            var from = points[i];
            var to = points[i + 1];
            var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from + perpendicular * .5f * thickness;
            var fromIn = from - perpendicular * .5f * thickness;
            var toOut = to + perpendicular * .5f * thickness;
            var toIn = to - perpendicular * .5f *thickness;
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
        }

        if (close && points.Count > 2)
        {
            var from = points[points.Count - 1];
            var to = points[0];
            var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from + perpendicular * .5f * thickness;
            var fromIn = from - perpendicular * .5f * thickness;
            var toOut = to + perpendicular * .5f * thickness;
            var toIn = to - perpendicular * .5f *thickness;
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
        }

        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static MeshInstance2D GetLinesMesh(List<Vector2> froms,
        List<Vector2> tos, float thickness)
    {
        var triPoints = new List<Vector2>();
        for (int i = 0; i < froms.Count; i++)
        {
            var from = froms[i];
            var to = tos[i];
            var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from + perpendicular * .5f * thickness;
            var fromIn = from - perpendicular * .5f * thickness;
            var toOut = to + perpendicular * .5f * thickness;
            var toIn = to - perpendicular * .5f *thickness;
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
        }

        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static MeshInstance2D GetLinesMeshCustomWidths(List<Vector2> froms,
        List<Vector2> tos, List<float> widths)
    {
        var triPoints = new List<Vector2>();
        for (int i = 0; i < froms.Count; i++)
        {
            var from = froms[i];
            var to = tos[i];
            var width = widths[i];
            var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
            var fromOut = from + perpendicular * width / 2f;
            var fromIn = from - perpendicular * width / 2f;
            var toOut = to + perpendicular * width / 2f;
            var toIn = to - perpendicular * width / 2f;
        
            triPoints.Add(fromIn);
            triPoints.Add(fromOut);
            triPoints.Add(toOut);
            triPoints.Add(toIn);
            triPoints.Add(toOut);
            triPoints.Add(fromIn);
        }

        var meshInstance = new MeshInstance2D();
        var mesh = GetArrayMesh(triPoints.ToArray());
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static MeshInstance2D GetLineMesh(Vector2 from, Vector2 to, float thickness)
    {
        var meshInstance = new MeshInstance2D();
        var perpendicular = (from - to).Normalized().Rotated(Mathf.Pi / 2f);
        var fromOut = from + perpendicular * thickness / 2f;
        var fromIn = from - perpendicular * thickness / 2f;
        var toOut = to + perpendicular * thickness / 2f;
        var toIn = to - perpendicular * thickness / 2f;
        
        var triPoints = new Vector2[]
        {
            fromIn, fromOut, toOut,
            toIn, toOut, fromIn
        };
        
        var mesh = GetArrayMesh(triPoints);
        meshInstance.Mesh = mesh;
        return meshInstance;
    }

    public static MeshInstance2D GetCircleMesh(Vector2 center, float radius, int resolution)
    {
        var angleIncrement = Mathf.Pi * 2f / (float) resolution;
        var triPoints = new List<Vector2>();
        for (int i = 0; i < resolution; i++)
        {
            var startAngle = angleIncrement * i;
            var startPoint = center + Vector2.Up.Rotated(startAngle) * radius;
            var endAngle = startAngle + angleIncrement;
            var endPoint = center + Vector2.Up.Rotated(endAngle) * radius;
            triPoints.Add(center);
            triPoints.Add(startPoint);
            triPoints.Add(endPoint);
        }

        var mesh = GetArrayMesh(triPoints.ToArray());
        var meshInstance = new MeshInstance2D();
        meshInstance.Mesh = mesh;
        return meshInstance;
    }
    public static Node2D GetArrowGraphic(Vector2 from, Vector2 to, float thickness)
    {
        var arrow = new Node2D();
        var length = from.DistanceTo(to);
        var lineTo = from + (to - from).Normalized() * length * .8f;
        var perpendicular = (to - from).Normalized().Rotated(Mathf.Pi / 2f);
        var line = GetLineMesh(from, lineTo, thickness);
        arrow.AddChild(line);
        var triPoints = new Vector2[]
        {
            to,
            lineTo + perpendicular * thickness,
            lineTo - perpendicular * thickness,
        };
        var triMesh = GetArrayMesh(triPoints);
        var tri = new MeshInstance2D();
        tri.Mesh = triMesh;
        arrow.AddChild(tri);
        return arrow;
    }

    public static Node2D GetSubGraphMesh<TNode, TEdge>(SubGraph<TNode, TEdge> subGraph,
        float thickness,
        Func<TNode, Vector2> getVertexPos,
        Color color,
        Color foreignEdgeColor)
    {
        var node = new Node2D();
        for (var i = 0; i < subGraph.Elements.Count; i++)
        {
            var e = subGraph.Elements[i];
            var vertexPos = getVertexPos(e);
            var vertex = GetCircleMesh(vertexPos, thickness * 2f, 12);
            vertex.SelfModulate = color;
            node.AddChild(vertex);
            foreach (var n in subGraph.Graph[e].Neighbors)
            {
                var nPos = getVertexPos(n);
                var edge = GetLineMesh(vertexPos, nPos, thickness);
                edge.SelfModulate = foreignEdgeColor;
                node.AddChild(edge);
                if (subGraph.Graph.NodeSubGraphs.ContainsKey(n)
                    && subGraph.Graph.NodeSubGraphs[n] == subGraph)
                {
                    edge.SelfModulate = color;
                }
            }
        }
        return node;
    }
    public static Node2D GetGraphMesh<TNode, TEdge>(Graph<TNode, TEdge> graph,
        float thickness,
        Func<TNode, Vector2> getVertexPos,
        Color color,
        Color foreignEdgeColor)
    {
        var node = new Node2D();
        for (var i = 0; i < graph.Elements.Count; i++)
        {
            var e = graph.Elements[i];
            var vertexPos = getVertexPos(e);
            var vertex = GetCircleMesh(vertexPos, thickness * 2f, 12);
            vertex.SelfModulate = color;
            node.AddChild(vertex);
            foreach (var n in graph[e].Neighbors)
            {
                var nPos = getVertexPos(n);
                var edge = GetLineMesh(vertexPos, nPos, thickness);
                edge.SelfModulate = foreignEdgeColor;
                node.AddChild(edge);
                edge.SelfModulate = color;
            }
        }
        return node;
    }

    public static Node2D GetPointsMesh(List<Vector2> points, float markerSize)
    {
        var triPoints = PointsGenerator.GetSquareMarkerMesh(points, markerSize);
        return MeshGenerator.GetMeshInstance(triPoints);
    }
    public static ArrayMesh GetArrayMesh(Vector2[] triPoints, Color[] triColors = null)
    {
        var arrayMesh = new ArrayMesh();
        var arrays = new Godot.Collections.Array();
        
        arrays.Resize((int)ArrayMesh.ArrayType.Max);

        arrays[(int)ArrayMesh.ArrayType.Vertex] = triPoints;
        arrays[(int)ArrayMesh.ArrayType.Color] = ConvertTriToVertexColors(triColors); 
        arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

        return arrayMesh; 
    }

    public static HashSet<Vector2Pair> GetBorderSegs(List<Vector2> triPoints)
    {
        var borderSegs1 = new HashSet<Vector2Pair>();
        var borderSegs2 = new HashSet<Vector2Pair>();

        void checkSeg(Vector2Pair seg)
        {
            var has1 = borderSegs1.Contains(seg);
            var has2 = borderSegs2.Contains(seg);
            if (has1 && has2)
            {
                borderSegs1.Remove(seg);
                borderSegs2.Remove(seg);
            }
            else if(has1)
            {
                borderSegs2.Add(seg);
            }
            else
            {
                borderSegs1.Add(seg);
            }
        }
        for (int i = 0; i < triPoints.Count; i += 3)
        {
            var a = triPoints[i];
            var b = triPoints[i + 1];
            var c = triPoints[i + 2];
            var seg = new Vector2Pair(a, b);
            checkSeg(seg);
        }

        return borderSegs1;
    }

    public static List<Vector2> GetSimplifiedTrisFromBorder(List<Vector2> borderPointsClockwise)
    {
        //points need to be clockwise
        //only use when it wont be overlapping ie can draw line to all border points from center 
        //w/out crossing a line
        var concaveLengths = new List<List<Vector2>>();
        concaveLengths.Add(new List<Vector2>{borderPointsClockwise[0], borderPointsClockwise[1]});
        for (var i = 0; i < borderPointsClockwise.Count; i++)
        {
            var point = borderPointsClockwise[i];
            var nextPoint = borderPointsClockwise[(i + 1) % borderPointsClockwise.Count];
            var nextNextPoint = borderPointsClockwise[(i + 2) % borderPointsClockwise.Count];
            var seg1 = nextPoint - point;
            var seg2 = nextNextPoint - nextPoint;
            if (seg2.AngleTo(seg1) < 0f)
            {
                concaveLengths.Add(new List<Vector2>{nextPoint, nextNextPoint});
            }
            else
            {
                concaveLengths[concaveLengths.Count - 1].Add(nextNextPoint);
            }
        }

        if (concaveLengths.Count > 1)
        {
            var ends = new List<Vector2>();
            concaveLengths.ForEach(concave =>
            {
                ends.Add(concave[0] - concave[concave.Count - 1]);
            });
            var result = new List<Vector2>(GetSimplifiedTrisFromBorder(ends));
            concaveLengths.ForEach(concave =>
            {
                result.AddRange(GetTrisForConcaveBorder(concave));
            });
            return result;
        }
        else
        {
            return GetTrisForConcaveBorder(concaveLengths[0]);
        }
    }

    private static List<Vector2> GetTrisForConcaveBorder(List<Vector2> border)
    {
        var result = new List<Vector2>();
        var anchor = border[0];
        for (var i = 1; i < border.Count - 1; i++)
        {
            result.Add(anchor);
            result.Add(border[i]);
            result.Add(border[i + 1]);
        }

        return result;
    }

    private static Color[] ConvertTriToVertexColors(Color[] triColors)
    {
        if (triColors == null) return null;
        var vertexColors = new Color[triColors.Length * 3];
        for (int i = 0; i < triColors.Length; i++)
        {
            vertexColors[3 * i] = triColors[i];
            vertexColors[3 * i + 1] = triColors[i];
            vertexColors[3 * i + 2] = triColors[i];
        }

        return vertexColors;
    }
}