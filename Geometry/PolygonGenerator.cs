using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DelaunatorSharp;

public class PolygonGenerator : Generator
{
    private Vector2 _dimensions;
    private IdDispenser _id;
    private bool _leftRightWrap;
    private float _polySize;
    private List<Vector2> _innerPoints;
    public PolygonGenerator(List<Vector2> innerPoints, Vector2 dimensions, 
                                    bool leftRightWrap, float polySize)
    {
        _innerPoints = innerPoints;
        _dimensions = dimensions;
        _leftRightWrap = leftRightWrap;
        _polySize = polySize;
    }
    public override GenReport Generate(GenWriteKey key)
    {
        var report = new GenReport(GetType().Name);
        _id = key.IdDispenser;
        
        report.StartSection();
        var info = new MapGenInfo(_innerPoints, _dimensions, _polySize, _leftRightWrap);
        key.GenData.GenInfo = info;
        if (info.Points.Any(v => v != v.Intify()))
        {
            throw new Exception();
        }
        report.StopSection("Setup");

        
        report.StartSection();
        var delaunayPoints = info.Points.Select(p => new DelaunayTriangulator.DelaunatorPoint(p)).ToList<IPoint>();
        CreateAndRegisterPolys(delaunayPoints, info, key);
        report.StopSection("Creating points and polys");

        report.StartSection();
        var graph = GraphGenerator.GenerateMapPolyVoronoiGraph(info, _id, key);
        report.StopSection("Generating poly graph");

        if (_leftRightWrap)
        {
            report.StartSection();
            Wrap(graph, info, key);
            report.StopSection("Wrapping");
        }
        else
        {
            throw new NotImplementedException();
        }
        report.StartSection();
        BuildBorders(info, graph, key);
        report.StopSection("Building borders");
        return report;
    }

    private void CreateAndRegisterPolys(IEnumerable<IPoint> points, MapGenInfo info, GenWriteKey key)
    {
        var polys = new List<MapPolygon>(points.Count());
        foreach (var dPoint in points)
        {
            var center = dPoint.GetIntV2();
            var polygon = MapPolygon.Create(center, _dimensions.x, key);
        }
        info.SetupPolys(key.Data.Planet.Polygons.Entities.ToList());
    }

    private void Wrap(Graph<MapPolygon, LineSegment> graph, MapGenInfo info, GenWriteKey key)
    {
        var wrapLeft = new List<MapPolygon>();
        wrapLeft.Add(info.CornerPolys[0]);
        wrapLeft.AddRange(info.LeftPolys);
        wrapLeft.Add(info.CornerPolys[2]);
            
        var wrapRight = new List<MapPolygon>();
        wrapRight.Add(info.CornerPolys[1]);
        wrapRight.AddRange(info.RightPolys);
        wrapRight.Add(info.CornerPolys[3]);
            
        GraphGenerator.WrapMapPolygonGraph(graph, wrapLeft, wrapRight, key);
    }
    private void BuildBorders(MapGenInfo info, Graph<MapPolygon, LineSegment> graph, GenWriteKey key)
    {
        var rHash = new HashSet<MapPolygon>(info.RightPolys);
        rHash.Add(info.CornerPolys[1]);
        rHash.Add(info.CornerPolys[3]);

        var borderChains = new ConcurrentDictionary<PolyBorderChain, PolyBorderChain>();
        var partitions = graph.Elements.Partition(10);
        
        Parallel.ForEach(partitions, 
            ps => ps.ForEach(p => BuildBorderSegs(p, info, rHash, graph, key, borderChains)));
        
        
        
        
        var mapWidth = key.GenData.GenMultiSettings.Dimensions.x;
        var nexuses = new Dictionary<Vector2, List<MapPolygonEdge>>();
        
        foreach (var b in borderChains)
        {
            CreateEdgeAndBorderChains(b, key, mapWidth, nexuses);
        }
        partitions.ForEach(ps => ps.ForEach(p => FlipEdgeSegsToClockwise(p)));
        GD.Print("done flipping");

        var edgeNexi = new Dictionary<MapPolygonEdge, Vector2>();
        
        CreateNexi(nexuses, edgeNexi, key);
        BindNexiToEdges(edgeNexi, key);
        
        key.Data.Notices.SetPolyShapes.Invoke();
    }
    
    private void FlipEdgeSegsToClockwise(MapPolygon poly)
    {
        // GD.Print(poly.Id);
        var borders = poly.GetPolyBorders();
        
        var allEdgeSegs = borders.SelectMany(b => b.Segments).ToList();
        if (allEdgeSegs.Count == 0) throw new Exception();
        // GD.Print(poly.Id + " 1");
        allEdgeSegs = allEdgeSegs.FlipChainify();

        
        
        var sum = allEdgeSegs.GetAngleAroundSum(Vector2.Zero);

        if (sum == 0f) throw new Exception();
        if (sum < 0f) allEdgeSegs = allEdgeSegs.Select(e => e.Reverse()).ToList();
        
        foreach (var border in borders)
        {
            if (border.Segments.Count != 1) throw new Exception();
            var edgeSeg = border.Segments[0];
            var newSeg = allEdgeSegs.First(ls => ls.IsSame(edgeSeg) || ls.Reverse().IsSame(edgeSeg));
            if (newSeg.IsSame(edgeSeg) == false)
            {
                edgeSeg.From = newSeg.From;
                edgeSeg.To = newSeg.To;
            }
        }
    }
    private void BuildBorderSegs(MapPolygon mp, MapGenInfo info, HashSet<MapPolygon> rHash,
        Graph<MapPolygon, LineSegment> graph, GenWriteKey key, 
        ConcurrentDictionary<PolyBorderChain, PolyBorderChain> borderChains)
    {
        if (info.LRWrap && rHash.Contains(mp))
        {
            throw new Exception();
        }
        var neighbors = graph.GetNeighbors(mp).Where(n => rHash.Contains(n) == false).ToList();
        if (neighbors.Count == 0) throw new Exception();
            
        neighbors.ForEach(nMp =>
        {
            if (nMp.Id > mp.Id) return;
            var edge = graph.GetEdge(mp, nMp);
            if (edge.From == edge.To)
            {
                return;
            }

            if (edge.From != edge.From.Intify() || edge.To != edge.To.Intify())
            {
                throw new Exception();
            }
                

            var lowEdge = new LineSegment(nMp.GetOffsetTo(edge.From, key.Data), 
                nMp.GetOffsetTo(edge.To, key.Data));
            if (lowEdge.IsCCW(Vector2.Zero))
            {
                lowEdge = lowEdge.Reverse();
            }
            var highEdge = new LineSegment(mp.GetOffsetTo(edge.From, key.Data), 
                mp.GetOffsetTo(edge.To, key.Data));
            if (highEdge.IsCCW(Vector2.Zero))
            {
                highEdge = highEdge.Reverse();
            }
            var chain1 = MapPolygonEdge.ConstructBorderChain(mp, nMp,
                new List<LineSegment> {highEdge}, key.Data);
                
                
            var chain2 = MapPolygonEdge.ConstructBorderChain(nMp, mp,
                new List<LineSegment> {lowEdge}, key.Data);
            borderChains.TryAdd(chain1, chain2);
        });
    }

    private void CreateEdgeAndBorderChains(KeyValuePair<PolyBorderChain, PolyBorderChain> b, GenWriteKey key,
        float mapWidth, Dictionary<Vector2, List<MapPolygonEdge>> nexuses)
    {
        var hiChain = b.Key;
        var loChain = b.Value;
        var edge = MapPolygonEdge.Create(b.Key, b.Value, key);

        var start = hiChain.Segments.First().From + b.Key.Native.Entity().Center;
        if (start != start.Intify()) throw new Exception();
            
        var end = hiChain.Segments.Last().To + b.Key.Native.Entity().Center;
        if (end != end.Intify()) throw new Exception();

        if (start.x < 0) start.x += mapWidth;
        if (end.x < 0) end.x += mapWidth;
            
        if(nexuses.ContainsKey(start) == false) nexuses.Add(start, new List<MapPolygonEdge>());
        if(nexuses.ContainsKey(end) == false) nexuses.Add(end, new List<MapPolygonEdge>());
            
        nexuses[start].Add(edge);
        nexuses[end].Add(edge);
    }

    private void CreateNexi(Dictionary<Vector2, List<MapPolygonEdge>> nexuses,
        Dictionary<MapPolygonEdge, Vector2> edgeNexi, GenWriteKey key)
    {
        foreach (var kvp in nexuses)
        {
            var point = kvp.Key;
            var edges = kvp.Value;
            var polys = edges.Select(e => e.HighPoly.Entity())
                .Union(edges.Select(e => e.LowPoly.Entity()))
                .Distinct().ToList();
            var nexus = MapPolyNexus.Construct(point, edges, polys, key);
            foreach (var e in edges)
            {
                if (edgeNexi.ContainsKey(e) == false) edgeNexi.Add(e, Vector2.Zero);
                if (edgeNexi[e].x == 0) edgeNexi[e] = new Vector2(nexus.Id, 0);
                else
                {
                    edgeNexi[e] = new Vector2(edgeNexi[e].x, nexus.Id);
                }
            }
        }
    }

    private void BindNexiToEdges(Dictionary<MapPolygonEdge, Vector2> edgeNexi, GenWriteKey key)
    {
        foreach (var kvp in edgeNexi)
        {
            var edge = kvp.Key;
            var n1 = key.Data.Planet.PolyNexi[(int)kvp.Value.x];
            var n2 = key.Data.Planet.PolyNexi[(int)kvp.Value.y];
            edge.SetNexi(n1, n2, key);
        }
    }
}
public class MapGenInfo
{
    public Vector2 Dimensions { get; private set; }
    public bool LRWrap { get; private set; }
    public List<Vector2> Points,
        TopPoints,
        BottomPoints,
        CornerPoints,
        LeftPoints,
        RightPoints;
    public List<MapPolygon> Polys,
        TopPolys,
        BottomPolys,
        CornerPolys,
        LeftPolys,
        RightPolys;
    public HashSet<Vector2> LRCornerCenterHash { get; private set; }
    public HashSet<MapPolygon> LRCornerPolyHash { get; private set; }
    public Dictionary<MapPolygon, MapPolygon> LRPairs { get; private set; }
    public Dictionary<Vector2,MapPolygon> PolysByCenter { get; private set; }
    public MapGenInfo(List<Vector2> points, Vector2 dimensions, float polySize, bool leftRightWrap)
    {
        Dimensions = dimensions;
        LRWrap = leftRightWrap;
        var numLrEdgePoints = (int)(dimensions.y / polySize);
        var numTbEdgePoints = (int)(dimensions.x / polySize);
    
        if (leftRightWrap)
        {
            var leftRightPoints = GetLeftRightWrapEdgePoints(dimensions, points, numLrEdgePoints, .1f);
            LeftPoints = leftRightPoints.leftPoints;
            RightPoints = leftRightPoints.rightPoints;
        }
        else
        {
            LeftPoints = GetConstrainedRandomFloats(dimensions.y, .1f, numLrEdgePoints)
                .Select(l => new Vector2(0f, l).Intify()).ToList();
        
            RightPoints = GetConstrainedRandomFloats(dimensions.y, .1f, numLrEdgePoints)
                .Select(l => new Vector2(dimensions.x, l).Intify()).ToList();
        
        }
        CornerPoints = new List<Vector2> { new Vector2(0f, 0f).Intify(), new Vector2(dimensions.x, 0f).Intify(), 
            new Vector2(0f, dimensions.y).Intify(), new Vector2(dimensions.x, dimensions.y).Intify()};
        
        TopPoints = GetConstrainedRandomFloats(dimensions.x, .1f, numTbEdgePoints)
            .Select(l => new Vector2(l, 0f).Intify()).ToList();
        BottomPoints = GetConstrainedRandomFloats(dimensions.x, .1f, numTbEdgePoints)
            .Select(l => new Vector2(l, dimensions.y).Intify()).ToList();

        LRCornerCenterHash = LeftPoints.Union(RightPoints).Union(CornerPoints).ToHashSet();

        CornerPolys = new List<MapPolygon>();
        LRPairs = new Dictionary<MapPolygon, MapPolygon>();
        PolysByCenter = new Dictionary<Vector2, MapPolygon>();
        Points = points
            .Union(CornerPoints)
            .Union(LeftPoints)
            .Union(RightPoints)
            .Union(TopPoints)
            .Union(BottomPoints).ToList();
    }

    public void SetupPolys(List<MapPolygon> polys)
    {
        Polys = polys;
        var centerHash = Points.ToHashSet();
        Polys.ForEach(p =>
        {
            if (centerHash.Contains(p.Center) == false)
            {
                throw new Exception();
            }
        });
        PolysByCenter = polys.ToDictionary(p => p.Center, p => p);
        LeftPolys = LeftPoints.Select(p => PolysByCenter[p]).ToList();
        RightPolys = RightPoints.Select(p => PolysByCenter[p]).ToList();
        TopPolys = TopPoints.Select(p => PolysByCenter[p]).ToList();
        BottomPolys = BottomPoints.Select(p => PolysByCenter[p]).ToList();
        CornerPolys = CornerPoints.Select(p => PolysByCenter[p]).ToList();
        LRCornerPolyHash = LRCornerCenterHash.Select(v => PolysByCenter[v]).ToHashSet();
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
            left.Add(new Vector2(0f, lat).Intify());
            right.Add(new Vector2(dimensions.x, lat).Intify());
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