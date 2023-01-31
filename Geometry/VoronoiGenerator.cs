using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DelaunatorSharp;

public class VoronoiGenerator
{
    private static Vector2 _dimensions;
    private static IDDispenser _id;
    private static Data _data;
    public static List<MapPolygon> GenerateMapPolygons(List<Vector2> points, Vector2 dimensions, 
        bool leftRightWrap, float polySize,
        Func<int, Vector2, MapPolygon> constructor,
        IDDispenser id,
        GenWriteKey key)
    {
        _data = key.Data;
        _id = id;
        _dimensions = dimensions;
        var edgePoints = AddBoundaryPoints(points, dimensions, polySize, leftRightWrap);
        var leftRightPoints = edgePoints.leftEdgePoints.Union(edgePoints.rightEdgePoints);
        var lrHash = leftRightPoints.ToHashSet();
        var delaunayPoints = points.Select(p => new DelaunayTriangulator.DelaunatorPoint(p)).ToList<IPoint>();
        var d = new Delaunator(delaunayPoints.ToArray());
        var voronoiCells = d.GetVoronoiCells().ToList();
        var polyIndexDic = new Dictionary<int, MapPolygon>();
        var polyCenterDic = new Dictionary<Vector2, MapPolygon>();
        var polygons = new HashSet<MapPolygon>();
        var lrByAltitude = new Dictionary<float, MapPolygon>();
        var lrPolyPairs = new Dictionary<MapPolygon, MapPolygon>();
        for (var i = 0; i < voronoiCells.Count; i++)
        {
            var voronoiCell = voronoiCells[i];
            var center = d.Points[voronoiCell.Index].GetV2();
            var polygon = constructor(voronoiCell.Index, center);
            key.Data.AddEntity<MapPolygon>(polygon, key);

            polyCenterDic.Add(center, polygon);

            polygons.Add(polygon);
            polyIndexDic.Add(voronoiCell.Index, polygon);
            
            if (lrHash.Contains(center))
            {
                if (lrByAltitude.ContainsKey(center.y)) lrPolyPairs.Add(lrByAltitude[center.y], polygon);
                else lrByAltitude.Add(center.y, polygon);
            }
        }   
        AddEdges(d, polyIndexDic, key);
        if(leftRightWrap)
        {
            var discardRight = MergeLeftRightPolys(polygons, lrPolyPairs, polyCenterDic, key);
            foreach (var mapPolygon in discardRight)
            {
                key.Data.Planet.Polygons.RemoveEntity(mapPolygon, key);
            }
            polygons.RemoveWhere(discardRight.Contains);
        }

        ConnectEdgePolys(polyCenterDic, edgePoints.leftEdgePoints, edgePoints.rightEdgePoints, 
            edgePoints.topEdgePoints, edgePoints.bottomEdgePoints, leftRightWrap, key);
        
        return polygons.ToList();
    }
    
    private static void AddEdges(Delaunator d, Dictionary<int, MapPolygon> polyDic, GenWriteKey key)
    {
        d.ForEachVoronoiEdge(edge =>
        {
            var tri = Delaunator.TriangleOfEdge(edge.Index);
            var oppTri = Delaunator.TriangleOfEdge(d.Halfedges[edge.Index]);
            var commonPoints = d.PointsOfTriangle(tri).Intersect(d.PointsOfTriangle(oppTri));
            if (commonPoints.Count() == 2)
            {
                var poly1 = polyDic[commonPoints.ElementAt(0)];
                var poly2 = polyDic[commonPoints.ElementAt(1)];
                var borderPoints = new List<LineSegment> {new LineSegment(edge.P.GetIntV2(), edge.Q.GetIntV2()) };
                
                var border = MapPolygonBorder.Create(_id.GetID(), poly1, poly2, borderPoints, key);
                _data.AddEntity<MapPolygonBorder>(border, key);
                poly1.AddNeighbor(poly2, border, key);
                poly2.AddNeighbor(poly1, border, key);
            }
        });
    }
    private static HashSet<MapPolygon> MergeLeftRightPolys(IEnumerable<MapPolygon> polygons, Dictionary<MapPolygon, MapPolygon> lrPolyPairs, 
        Dictionary<Vector2, MapPolygon> polyDic, GenWriteKey key)
    {
        var firstLeft = polyDic[Vector2.Zero];
        var lastLeft = polyDic[new Vector2(0f, _dimensions.y)];
        var firstRight = polyDic[new Vector2(_dimensions.x, 0f)];
        var lastRight = polyDic[new Vector2(_dimensions.x, _dimensions.y)];
        var rights = new HashSet<MapPolygon>();
        bindPair(firstLeft, firstRight);
        foreach (var keyValuePair in lrPolyPairs)
        {
            var poly1 = keyValuePair.Key;
            var poly2 = keyValuePair.Value;
            bindPair(poly1, poly2);
        }

        void bindPair(MapPolygon poly1, MapPolygon poly2)
        {
            var left = poly1.Center.x < poly2.Center.x
                ? poly1
                : poly2;
            var right = poly1.Center.x < poly2.Center.x
                ? poly2
                : poly1;
            var leftN = left.Neighbors.Count();
            var rightN = right.Neighbors.Count();
            for (var i = 0; i < right.Neighbors.Count(); i++)
            {
                var neighbor = right.Neighbors.Refs().ElementAt(i);
                var oldBorder = right.GetBorder(neighbor, _data);
                var rightBorderPoints = oldBorder.GetPointsRel(right);
                var nBorderPoints = oldBorder.GetPointsRel(neighbor);
                var leftBorderPoints = rightBorderPoints.Select(v => v).Reverse().ToList();
                var newBorder = MapPolygonBorder.ConstructEdgeCase(_id.GetID(), left, leftBorderPoints.GetLineSegments().ToList(), 
                    neighbor, nBorderPoints.GetLineSegments().ToList(), key);
                left.AddNeighbor(neighbor, newBorder, key);
                neighbor.AddNeighbor(left, newBorder, key);
                neighbor.RemoveNeighbor(right, key);
                
            }
            rights.Add(right);
        }

        return rights;
    }

    private static void ConnectEdgePolys(Dictionary<Vector2, MapPolygon> polyDic,
        List<Vector2> leftPoints, List<Vector2> rightPoints, List<Vector2> topPoints, 
        List<Vector2> bottomPoints, bool leftRightWrap, GenWriteKey key)
    {
        var topLeft = leftPoints[0];
        var topRight = rightPoints[0];
        var bottomLeft = leftPoints[leftPoints.Count - 1];
        var dimX = topRight.x;
        var dimY = topRight.y;
        
        var firstLeft = polyDic[Vector2.Zero];
        var firstRight = polyDic[new Vector2(_dimensions.x, 0f)];
        var lastRight = polyDic[new Vector2(_dimensions.x, _dimensions.y)];
        var lastLeft = polyDic[new Vector2(0f, _dimensions.y)];
        
        var firstRightNOld = firstRight.Neighbors.Refs().ToList();
        var lastRightNOld = lastRight.Neighbors.Refs().ToList();
        
        topPoints.Add(rightPoints[0]);
        bottomPoints.Add(rightPoints.Last());
        LinkEdgePolys(topPoints, polyDic, topLeft, dimX, false, key);
        LinkEdgePolys(bottomPoints, polyDic, bottomLeft, dimX, false, key);

        if(leftRightWrap)
        {
            var firstRightNew = firstRight.Neighbors.Refs().Except(firstRightNOld).ElementAt(0);
            var firstRightCoords = firstRight.GetBorder(firstRightNew, _data).GetPointsRel(firstRight);
            var firstRightNCoords = firstRight.GetBorder(firstRightNew, _data).GetPointsRel(firstRightNew);
            
            var lastRightNew = lastRight.Neighbors.Refs().Except(lastRightNOld).ElementAt(0);
            var lastRightCoords = lastRight.GetBorder(lastRightNew, _data).GetPointsRel(lastRight);
            var lastRightNCoords = lastRight.GetBorder(lastRightNew, _data).GetPointsRel(lastRightNew);


            var firstBorder = MapPolygonBorder.ConstructEdgeCase(_id.GetID(), firstLeft, firstRightCoords.GetLineSegments().ToList(), 
                firstRightNew, firstRightNCoords.GetLineSegments().ToList(), key);
            firstLeft.AddNeighbor(firstRightNew, firstBorder, key);
            firstRightNew.AddNeighbor(firstLeft, firstBorder, key);
            firstRightNew.RemoveNeighbor(firstRight, key);
            
            var lastBorder = MapPolygonBorder.ConstructEdgeCase(_id.GetID(), lastLeft, lastRightCoords.GetLineSegments().ToList(),
                lastRightNew, lastRightNCoords.GetLineSegments().ToList(), key);
            lastLeft.AddNeighbor(lastRightNew, lastBorder, key);
            lastRightNew.AddNeighbor(lastLeft, lastBorder, key);
            lastRightNew.RemoveNeighbor(lastRight, key);
            
            LinkMergeEdgePolys(leftPoints, polyDic, true, key);
            CloseOffEnd(firstLeft);
            CloseOffEnd(lastLeft);
        }
        else
        {
            
            LinkEdgePolys(leftPoints, polyDic, topLeft, dimY, true, key);
            LinkEdgePolys(rightPoints, polyDic, topRight, dimY, true, key);
        
            for (var i = 1; i < leftPoints.Count - 1; i++)
            {
                var p = leftPoints[i];
                CloseOffEnd(polyDic[p]);
            }
            for (var i = 1; i < rightPoints.Count - 1; i++)
            {
                var p = rightPoints[i];
                if(p == rightPoints.First() || p == rightPoints.Last()) continue;
                CloseOffEnd(polyDic[p]);
            }

            CloseOffCorner(polyDic[leftPoints[0]]);
            CloseOffCorner(polyDic[leftPoints.Last()]);
            CloseOffCorner(polyDic[rightPoints[0]]);
            CloseOffCorner(polyDic[rightPoints.Last()]);
        }
        topPoints.ForEach(p =>
        {
            if(p == leftPoints.First() || p == leftPoints.Last()) return;
            if(p == rightPoints.First() || p == rightPoints.Last()) return;
            CloseOffEnd(polyDic[p]);
        });
        bottomPoints.ForEach(p =>
        {
            if(p == leftPoints.First() || p == leftPoints.Last()) return;
            if(p == rightPoints.First() || p == rightPoints.Last()) return;
            CloseOffEnd(polyDic[p]);
        });
    }

    private static void LinkMergeEdgePolys(List<Vector2> points,
        Dictionary<Vector2, MapPolygon> polyDic, bool lr, GenWriteKey key)
    {
        for (var i = 0; i < points.Count - 1; i++)
        {
            var poly = polyDic[points[i]];
            var next = polyDic[points[(i + 1)]];
            var polyBorderPoints = poly.GetNeighborBorders(_data)
                .SelectMany(nb => nb.GetPointsAbs())
                .Distinct().ToList();
            
            var nextBorderPoints = next.GetNeighborBorders(_data)
                .SelectMany(nb => nb.GetPointsAbs())
                .Distinct().ToList();
            
            var shared = polyBorderPoints.Intersect(nextBorderPoints).ToList();
            if (shared.Count == 2)
            {
                var border = MapPolygonBorder.ConstructEdgeCase(_id.GetID(), poly, shared.Select(p => p - poly.Center).GetLineSegments().ToList(), 
                    next, shared.Select(p => p - next.Center).GetLineSegments().ToList(), key);
                poly.AddNeighbor(next, border, key);
                next.AddNeighbor(poly, border, key);
            }
            else
            {
                throw new Exception();
            }
        }
        
        
    }
    private static void LinkEdgePolys(List<Vector2> points, 
        Dictionary<Vector2, MapPolygon> polyDic, Vector2 anchor, float dim, bool lr,
        GenWriteKey key)
    {
        if(points[0] != anchor) points.Insert(0, anchor);

        Vector2 getNewPoint(Vector2 joinPoint)
        {
            return new Vector2(
                lr ? anchor.x : joinPoint.x,
                lr ? joinPoint.y : anchor.y
            );
        }

        Vector2 getLastNewPoint(Vector2 joinPoint)
        {
            return lr 
                ? new Vector2(anchor.x, joinPoint.y + dim)
                : new Vector2(joinPoint.x + dim, anchor.y);
        }

        var shift = lr ? new Vector2(0f, dim) : new Vector2(dim, 0f);
        for (var i = 0; i < points.Count - 1; i++)
        {
            var poly = polyDic[points[i]];
            var next = polyDic[points[i + 1]];
            var polyBorderPoints = poly.GetNeighborBorders(_data)
                .SelectMany(nb => nb.GetPointsRel(poly))
                .Select(p => p + poly.Center).Distinct().ToList();
            
            var nextBorderPoints = next.GetNeighborBorders(_data)
                .SelectMany(nb => nb.GetPointsRel(next))
                .Select(p => p + next.Center).Distinct().ToList();
            
            var shared = polyBorderPoints.Intersect(nextBorderPoints);
            if (shared.Count() == 1)
            {
                var joinPoint = shared.ElementAt(0);
                var newPoint = getNewPoint(joinPoint);
                var borderPoints = new List<Vector2> {newPoint, joinPoint};
                var border = MapPolygonBorder.ConstructEdgeCase(_id.GetID(), poly, borderPoints.Select(p => p - poly.Center).GetLineSegments().ToList(),
                    next, borderPoints.Select(p => p - next.Center).GetLineSegments().ToList(), key);
                poly.AddNeighbor(next, border, key);
                next.AddNeighbor(poly, border, key);
            }
            else throw new Exception();
            
        }
        
        
        var anchorpoly = polyDic[anchor];
        var last = polyDic[points[points.Count - 1]];
        var anchorBorderPoints = anchorpoly.GetNeighborBorders(_data)
            .SelectMany(nb => nb.GetPointsRel(anchorpoly))
            .Select(p => p + anchor);
        
        //todo
        var lastBorderPoints = last.GetNeighborBorders(_data)
            .SelectMany(nb => nb.GetPointsRel(last))
            .Select(p => p + last.Center - shift);
        var sharedLast = anchorBorderPoints.Intersect(lastBorderPoints);
        if (sharedLast.Count() == 1)
        {
            var joinPoint = sharedLast.ElementAt(0);
            var lastJoinPoint = joinPoint + shift;
            var newPoint = getNewPoint(joinPoint);
            var lastNewPoint = getLastNewPoint(joinPoint);
            var border = MapPolygonBorder.ConstructEdgeCase(_id.GetID(), anchorpoly, 
                new List<LineSegment> {new LineSegment(joinPoint - anchor, newPoint - anchor)},
                last, new List<LineSegment> {new LineSegment(lastJoinPoint - last.Center, lastNewPoint - last.Center)}, key);
            last.AddNeighbor(anchorpoly, border, key);
            anchorpoly.AddNeighbor(last, border, key);
        }
    }

    private static void CloseOffCorner(MapPolygon poly)
    {
        var singlePoints = new HashSet<Vector2>();
        var borderPoints = poly.GetNeighborBorders(_data).SelectMany(nb => nb.GetPointsAbs());
        foreach (var borderPoint in borderPoints)
        {
            if (singlePoints.Contains(borderPoint)) singlePoints.Remove(borderPoint);
            else singlePoints.Add(borderPoint);
        }

        if (singlePoints.Count == 2)
        {
            poly.AddNoNeighborBorder(singlePoints.ElementAt(0) - poly.Center, Vector2.Zero);
            poly.AddNoNeighborBorder(Vector2.Zero, singlePoints.ElementAt(1) - poly.Center);
        }
        else throw new Exception();
    }
    private static void CloseOffEnd(MapPolygon poly)
    {
        var singlePoints = new HashSet<Vector2>();
        var borderPoints = poly.GetNeighborBorders(_data).SelectMany(nb => nb.GetPointsRel(poly)).ToList();
        foreach (var borderPoint in borderPoints)
        {
            if (singlePoints.Contains(borderPoint)) singlePoints.Remove(borderPoint);
            else singlePoints.Add(borderPoint);
        }

        if (singlePoints.Count == 2)
        {
            poly.AddNoNeighborBorder(singlePoints.ElementAt(0), singlePoints.ElementAt(1));
        }
        else throw new Exception();
    }
    private static (List<Vector2> leftEdgePoints, List<Vector2> rightEdgePoints,
        List<Vector2> topEdgePoints, List<Vector2> bottomEdgePoints)
        AddBoundaryPoints(List<Vector2> points, Vector2 dimensions, float polySize, bool leftRightWrap)
    {
        var lrEdgePoints = (int)(dimensions.y / polySize);
        var tbEdgePoints = (int)(dimensions.x / polySize);

        List<Vector2> leftPoints;
        List<Vector2> rightPoints;
        if (leftRightWrap)
        {
            var leftRightPoints = GetLeftRightWrapEdgePoints(dimensions, points, lrEdgePoints, .1f);
            leftPoints = leftRightPoints.leftPoints;
            rightPoints = leftRightPoints.rightPoints;
        }
        else
        {
            leftPoints = GetConstrainedRandomFloats(dimensions.y, .1f, lrEdgePoints)
                .Select(l => new Vector2(0f, l)).ToList();
            
            rightPoints = GetConstrainedRandomFloats(dimensions.y, .1f, lrEdgePoints)
                .Select(l => new Vector2(dimensions.x, l)).ToList();
            
        }
        leftPoints.Insert(0, new Vector2(0f, 0f));
        leftPoints.Add(new Vector2(0f, dimensions.y));
        rightPoints.Insert(0, new Vector2(dimensions.x, 0f));
        rightPoints.Add(new Vector2(dimensions.x, dimensions.y));
        
        var topPoints = GetConstrainedRandomFloats(dimensions.x, .1f, tbEdgePoints)
            .Select(l => new Vector2(l, 0f)).ToList();
        var bottomPoints = GetConstrainedRandomFloats(dimensions.x, .1f, tbEdgePoints)
            .Select(l => new Vector2(l, dimensions.y)).ToList();
        
        points.AddRange(leftPoints);
        points.AddRange(rightPoints);
        points.AddRange(topPoints);
        points.AddRange(bottomPoints);
        return (leftPoints, rightPoints, topPoints, bottomPoints);
    }
    private static (List<Vector2> leftPoints, List<Vector2> rightPoints) GetLeftRightWrapEdgePoints(Vector2 dimensions, List<Vector2> points, 
        int numEdgePoints, float marginRatio)
    {
        var left = new List<Vector2>();
        var right = new List<Vector2>();
        var lengthPer = dimensions.y / numEdgePoints;
        var margin = lengthPer * marginRatio;
        var lats = GetConstrainedRandomFloats(dimensions.y, .1f, numEdgePoints);
        for (int i = 0; i < lats.Count; i++)
        {
            var lat = lats[i];
            left.Add(new Vector2(0f, lat));
            right.Add(new Vector2(dimensions.x, lat));
        }

        return (left, right);
    }

    private static List<float> GetConstrainedRandomFloats(float range, float marginRatio, int count)
    {
        var result = new List<float>();
        var lengthPer = range / count;
        var margin = lengthPer * marginRatio;
        for (int i = 1; i < count - 1; i++)
        {
            var sample = Game.I.Random.RandfRange(lengthPer * i + margin, lengthPer * (i + 1) - margin);
            result.Add(sample);
        }

        return result;
    }
}
