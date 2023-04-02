using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public partial class MapPolygon : Entity, 
    IReadOnlyGraphNode<MapPolygon, PolyBorderChain>
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public Vector2 Center { get; protected set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public Dictionary<int, PolyBorderChain> NeighborBorders { get; protected set; }
    public Color Color { get; protected set; }
    public float Altitude { get; protected set; }
    public float Roughness { get; protected set; }
    public float Moisture { get; protected set; }
    public EntityRef<Regime> Regime { get; protected set; }
    public PolyTris Tris { get; protected set; }
    public bool IsLand { get; protected set; }
    public EmploymentReport Employment { get; private set; }
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, EntityRefCollection<MapPolygon> neighbors, 
        Dictionary<int, PolyBorderChain> neighborBorders, Color color, float altitude, float roughness, 
        float moisture, EntityRef<Regime> regime, PolyTris tris, bool isLand,
        EmploymentReport employment) : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        NeighborBorders = neighborBorders;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        Regime = regime;
        Tris = tris;
        IsLand = isLand;
        Employment = employment;
    }

    public static MapPolygon Create(Vector2 center, float mapWidth, GenWriteKey key)
    {
        var mapCenter = center;
        if (mapCenter.x > mapWidth) mapCenter = new Vector2(mapCenter.x - mapWidth, center.y);
        if (mapCenter.x < 0f) mapCenter = new Vector2(mapCenter.x + mapWidth, center.y);
        var p = new MapPolygon(key.IdDispenser.GetID(), mapCenter,
            EntityRefCollection<MapPolygon>.Construct(new HashSet<int>(), key.Data),
            new Dictionary<int, PolyBorderChain>(),
            ColorsExt.GetRandomColor(),
            0f,
            0f,
            0f,
            new EntityRef<Regime>(-1),
            PolyTris.Create(new List<PolyTri>(), null, key),
            true,
            EmploymentReport.Construct()
        );
        key.Create(p);
        return p;
    }
    
    public void AddNeighbor(MapPolygon n, PolyBorderChain border, GenWriteKey key)
    {
        if (Neighbors.Contains(n)) return;
        Neighbors.AddRef(n, key);
        NeighborBorders.Add(n.Id, border);
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
    public void SetTerrainTris(PolyTris tris, GenWriteKey key)
    {
        Tris = tris;
    }

    public void SetIsLand(bool isLand, GenWriteKey key)
    {
        IsLand = isLand;
    }

    public void SetEmploymentReport(EmploymentReport employment, ProcedureWriteKey key)
    {
        Employment = employment;
    }
    PolyBorderChain IReadOnlyGraphNode<MapPolygon, PolyBorderChain>.GetEdge(MapPolygon neighbor) =>
        this.GetBorder(neighbor.Id);
    
    MapPolygon IReadOnlyGraphNode<MapPolygon>.Element => this;


    IReadOnlyCollection<MapPolygon> IReadOnlyGraphNode<MapPolygon>.Neighbors => Neighbors;

    bool IReadOnlyGraphNode<MapPolygon>.HasNeighbor(MapPolygon neighbor) => Neighbors.RefIds.Contains(neighbor.Id);
}
