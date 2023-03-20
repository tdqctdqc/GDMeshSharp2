using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Godot;
using System.Linq;
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

        
        // throw new PreGraphFailure(graph);
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
        GD.Print("building borders");
        var rHash = new HashSet<MapPolygon>(info.RightPolys);
        rHash.Add(info.CornerPolys[1]);
        rHash.Add(info.CornerPolys[3]);

        var constructBorders = new ConcurrentBag<Action>();
        
        void buildBorders(MapPolygon mp)
        {
            if (info.LRWrap && rHash.Contains(mp))
            {
                throw new Exception();
            }
            var neighbors = graph.GetNeighbors(mp).Where(n => rHash.Contains(n) == false).ToList();
            if (neighbors.Count == 0) throw new Exception(); 
            neighbors.ForEach(nMp =>
            {
                var edge = graph.GetEdge(mp, nMp);
                if (edge.From == edge.To)
                {
                    return;
                }

                if (edge.IsCCW(mp.Center))
                {
                    edge = edge.Reverse();
                }
                constructBorders.Add(() => MapPolygonEdge.Create(mp, nMp, new List<LineSegment> {edge}, key));
            });
        }

        Parallel.ForEach(graph.Elements, buildBorders);
        
        foreach (var a in constructBorders)
        {
            a.Invoke();
        }
        key.GenData.Events.SetPolyShapes?.Invoke();
        
    }
}
public class MapGenInfo
{
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