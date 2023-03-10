using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public partial class MapPolygon : Entity, 
    IGraphNode<MapPolygon, PolyBorderChain>,
    IStaticGraphNode<MapPolygon, PolyBorderChain>
{
    public static IReadOnlyGraph<MapPolygon, PolyBorderChain> BorderGraph
        = new ImplicitGraph<MapPolygon, PolyBorderChain>(p => true, p => p.Neighbors,
            (p, q) => p.HasNeighbor(q), (p, q) => p.GetBorder(q));

    public override Type GetDomainType() => typeof(PlanetDomain);
    public Vector2 Center { get; protected set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public Dictionary<int, PolyBorderChain> NeighborBorders { get; private set; }
    public Color Color { get; protected set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public bool IsCoast() => IsLand() && Neighbors.Refs().Any(n => n.IsWater());
    public MapPolygonEdge GetEdge(MapPolygon neighbor, Data data) 
        => data.Planet.PolyEdges.GetEdge(this, neighbor);

    public PolyBorderChain GetBorder(MapPolygon neighbor) => NeighborBorders[neighbor.Id];
    
    
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, EntityRefCollection<MapPolygon> neighbors, 
        Dictionary<int, PolyBorderChain> neighborBorders, Color color, float altitude, float roughness, 
        float moisture, EntityRef<Regime> regime) : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        NeighborBorders = neighborBorders;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        Regime = regime;
    }

    public static MapPolygon Create(int id, Vector2 center, float mapWidth, GenWriteKey key)
    {
        var mapCenter = center;
        if (mapCenter.x > mapWidth) mapCenter = new Vector2(mapCenter.x - mapWidth, center.y);
        if (mapCenter.x < 0f) mapCenter = new Vector2(mapCenter.x + mapWidth, center.y);
        var p = new MapPolygon(id, mapCenter,
            new EntityRefCollection<MapPolygon>(new HashSet<int>()),
            new Dictionary<int, PolyBorderChain>(),
            ColorsExt.GetRandomColor(),
            0f,
            0f,
            0f,
            new EntityRef<Regime>(-1)
        );
        key.Create(p);
        return p;
    }
    public bool HasNeighbor(MapPolygon p)
    {
        return Neighbors.Refs().Contains(p);
    }
    public IEnumerable<MapPolygonEdge> GetNeighborEdges(Data data) => Neighbors.Refs()
        .Select(n => GetEdge(n, data));
    public IEnumerable<PolyBorderChain> GetNeighborBorders() => Neighbors.Refs()
        .Select(n => GetBorder(n));
    public IEnumerable<LineSegment> GetNeighborEdgeSegments(Data data)
    {
        return GetNeighborBorders().UnionSegs<PolyBorderChain, LineSegment, Vector2>();
    }
    public PolyTerrainTris GetTerrainTris(Data data) => data.Planet.TerrainTris.ByPoly[this];
    public void AddNeighbor(MapPolygon poly, PolyBorderChain border, GenWriteKey key)
    {
        if (Neighbors.Contains(poly)) return;
        Neighbors.AddRef(poly, key);
        NeighborBorders.Add(poly.Id, border);
    }

    public void SetNeighborBorder(MapPolygon poly, PolyBorderChain border, GenWriteKey key)
    {
        if (Neighbors.Contains(poly) == false) throw new Exception();
        NeighborBorders[poly.Id] = border;
    }
    public void RemoveNeighbor(MapPolygon poly, GenWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.RemoveRef(poly, key);
    }
    public void SetRegime(Regime r, CreateWriteKey key)
    {
        GetMeta().UpdateEntityVar<EntityRef<Regime>>(nameof(Regime), this, key, new EntityRef<Regime>(r.Id));
    }

    public List<LineSegment> GetBoundarySegments(Data data)
    {
        return data.Cache.PolyBoundarySegments[this];
    }
    public List<LineSegment> BuildBoundarySegments(Data data)
    {
        var neighborSegs = GetNeighborEdgeSegments(data).ToList();

        var before = data.Cache.PolyBoundarySegments != null 
            ? data.Cache.PolyBoundarySegments[this]
            : new List<LineSegment>();

        if (neighborSegs.IsCircuit() == false)
        {
            var edge = new LineSegment(neighborSegs.Last().To, neighborSegs[0].From);
            neighborSegs.Add(edge);
        }

        if (neighborSegs.IsCircuit() == false || neighborSegs.IsContinuous() == false)
        {
            GD.Print("still not circuit");
            throw new SegmentsNotConnectedException(data, this, before, neighborSegs);
        }
        return neighborSegs;
    }
    
    PolyBorderChain IGraphNode<MapPolygon, PolyBorderChain>.GetEdge(MapPolygon n) => GetBorder(n);
    bool IGraphNode<MapPolygon>.HasEdge(MapPolygon n) => Neighbors.Contains(n);
    IReadOnlyCollection<MapPolygon> IGraphNode<MapPolygon>.Neighbors => Neighbors;
    MapPolygon IStaticGraphNode<MapPolygon>.Element => this;

    IReadOnlyGraph<MapPolygon, PolyBorderChain> IStaticGraphNode<MapPolygon, PolyBorderChain>.Graph => BorderGraph;
    IReadOnlyGraph<MapPolygon> IStaticGraphNode<MapPolygon>.Graph => BorderGraph;
}
