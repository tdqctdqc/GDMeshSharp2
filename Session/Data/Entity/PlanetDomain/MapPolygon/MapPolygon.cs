using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public partial class MapPolygon : Entity, 
    IGraphNode<MapPolygon, PolyBorderChain>,
    IStaticNode<MapPolygon, PolyBorderChain>
{
    public static IReadOnlyGraph<MapPolygon, PolyBorderChain> BorderGraph
        = new ImplicitGraph<MapPolygon, PolyBorderChain>(p => true, p => p.Neighbors,
            (p, q) => p.HasNeighbor(q), (p, q) => p.GetBorder(q));
    public Vector2 Center { get; protected set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public Dictionary<int, PolyBorderChain> NeighborBorders { get; private set; }
    public Color Color { get; protected set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public PolyTerrainTris TerrainTris { get; private set; }
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, EntityRefCollection<MapPolygon> neighbors, 
        Dictionary<int, PolyBorderChain> neighborBorders, Color color, float altitude, float roughness, 
        float moisture, EntityRef<Regime> regime, PolyTerrainTris terrainTris) : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        NeighborBorders = neighborBorders;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        Regime = regime;
        TerrainTris = terrainTris;
    }

    public static MapPolygon Create(Vector2 center, float mapWidth, GenWriteKey key)
    {
        var mapCenter = center;
        if (mapCenter.x > mapWidth) mapCenter = new Vector2(mapCenter.x - mapWidth, center.y);
        if (mapCenter.x < 0f) mapCenter = new Vector2(mapCenter.x + mapWidth, center.y);
        var p = new MapPolygon(key.IdDispenser.GetID(), mapCenter,
            new EntityRefCollection<MapPolygon>(new HashSet<int>()),
            new Dictionary<int, PolyBorderChain>(),
            ColorsExt.GetRandomColor(),
            0f,
            0f,
            0f,
            new EntityRef<Regime>(-1),
            PolyTerrainTris.Create(new List<PolyTri>(), key)
        );
        key.Create(p);
        return p;
    }
    
    public void AddNeighbor(MapPolygon poly, PolyBorderChain border, GenWriteKey key)
    {
        if (Neighbors.Contains(poly)) return;
        Neighbors.AddRef(poly, key);
        NeighborBorders.Add(poly.Id, border);
    }
    public void SetNeighborBorder(MapPolygon n, PolyBorderChain border, GenWriteKey key)
    {
        if (Neighbors.Contains(n) == false) throw new Exception();
        NeighborBorders[n.Id] = border;
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
    public void SetTerrainTris(PolyTerrainTris tris, GenWriteKey key)
    {
        TerrainTris = tris;
    }
    public List<LineSegment> BuildBoundarySegments(Data data)
    {
        var neighborSegs = this.GetOrderedNeighborBorders(data).SelectMany(b => b.Segments).ToList();

        if (neighborSegs.IsCircuit() == false)
        {
            var edge = new LineSegment(neighborSegs.Last().To, neighborSegs[0].From);
            neighborSegs.Add(edge);
        }

        if (neighborSegs.IsCircuit() == false || neighborSegs.IsContinuous() == false)
        {
            GD.Print("still not circuit");
            throw new Exception();
            // throw new SegmentsNotConnectedException(before, neighborSegs);
        }
        return neighborSegs;
    }
    
    PolyBorderChain IGraphNode<MapPolygon, PolyBorderChain>.GetEdge(MapPolygon n) => this.GetBorder(n);
    bool IGraphNode<MapPolygon>.HasEdge(MapPolygon n) => Neighbors.Contains(n);
    IReadOnlyCollection<MapPolygon> IGraphNode<MapPolygon>.Neighbors => Neighbors;
    MapPolygon IStaticNode<MapPolygon>.Element => this;

    IReadOnlyGraph<MapPolygon, PolyBorderChain> IStaticNode<MapPolygon, PolyBorderChain>.Graph => BorderGraph;
    IReadOnlyGraph<MapPolygon> IStaticNode<MapPolygon>.Graph => BorderGraph;
    public override Type GetDomainType() => typeof(PlanetDomain);
}
