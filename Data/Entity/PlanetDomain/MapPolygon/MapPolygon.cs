using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public partial class MapPolygon : Entity
{
    public override Type GetDomainType() => typeof(PlanetDomain);
    public Vector2 Center { get; protected set; }
    
    //todo check this works w serialization, or put in local cache
    public List<LineSegment> BorderSegments { get; private set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public Color Color { get; protected set; }
    public float Altitude { get; private set; }
    public float Roughness { get; private set; }
    public float Moisture { get; private set; }
    public float SettlementSize { get; private set; }
    public EntityRef<Regime> Regime { get; private set; }
    public bool IsLand() => Altitude > .5f;
    public bool IsWater() => IsLand() == false;
    public bool IsCoast() => IsLand() && Neighbors.Refs().Any(n => n.IsWater());
    public MapPolygonBorder GetBorder(MapPolygon neighbor, Data data) 
        => data.Planet.PolyBorders.GetBorder(this, neighbor);
    public LineSegment OutsideEdge { get; private set; }
    
    
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, EntityRefCollection<MapPolygon> neighbors, 
        Color color, float altitude, float roughness, 
        float moisture, float settlementSize, EntityRef<Regime> regime,
        List<LineSegment> borderSegments) : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        SettlementSize = settlementSize;
        Regime = regime;
        BorderSegments = borderSegments;
    }

    public static MapPolygon Create(int id, Vector2 center, float mapWidth, GenWriteKey key)
    {
        var mapCenter = center;
        if (mapCenter.x > mapWidth) mapCenter = new Vector2(mapCenter.x - mapWidth, center.y);
        if (mapCenter.x < 0f) mapCenter = new Vector2(mapCenter.x + mapWidth, center.y);
        var p = new MapPolygon(id, mapCenter,
            new EntityRefCollection<MapPolygon>(new HashSet<int>()),
            ColorsExt.GetRandomColor(),
            0f,
            0f,
            0f,
            0f,
            new EntityRef<Regime>(-1),
            new List<LineSegment>()
        );
        key.Create(p);
        return p;
    }
    public bool HasNeighbor(MapPolygon p)
    {
        return Neighbors.Refs().Contains(p);
    }
    public IEnumerable<MapPolygonBorder> GetNeighborBorders(Data data) => Neighbors.Refs()
        .Select(n => GetBorder(n, data));

    public PolyTerrainTris GetTerrainTris(Data data) => data.Planet.TerrainTris.ByPoly[this];
    public void AddNeighbor(MapPolygon poly, MapPolygonBorder border, GenWriteKey key)
    {
        if (Neighbors.Contains(poly)) return;
        Neighbors.AddRef(poly, key.Data);
    }
    public void RemoveNeighbor(MapPolygon poly, GenWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.RemoveRef(poly, key.Data);
    }
    public void SetRegime(Regime r, CreateWriteKey key)
    {
        GetMeta().UpdateEntityVar<EntityRef<Regime>>(nameof(Regime), this, key, new EntityRef<Regime>(r.Id));
    }

    public void SetBorderSegments(GenWriteKey key)
    {
        var neighborSegs = Neighbors.Refs().Select(n => GetBorder(
                n, key.Data))
            .SelectMany(b => b.GetSegsRel(this))
            .ToList();
        
        neighborSegs.CorrectSegmentsToClockwise(Vector2.Zero);
        neighborSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);
        neighborSegs = neighborSegs.OrderEndToStart(key.GenData, this);

        var before = BorderSegments.Count > 0
            ? BorderSegments.ToList() 
            : neighborSegs.ToList();

        if (neighborSegs.IsCircuit() == false)
        {
            var edge = new LineSegment(neighborSegs.Last().To, neighborSegs[0].From);
            neighborSegs.Add(edge);
        }

        if (neighborSegs.IsCircuit() == false || neighborSegs.IsContinuous() == false)
        {
            GD.Print("still not circuit");
            throw new SegmentsNotConnectedException(key.GenData, this, before, neighborSegs);
        }

        BorderSegments = neighborSegs;
    }
}
