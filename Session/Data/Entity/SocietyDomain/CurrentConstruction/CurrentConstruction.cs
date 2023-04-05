using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class CurrentConstruction : Entity
{
    public Dictionary<PolyTriPosition, Construction> ByTri { get; private set; }
    public Dictionary<int, List<Construction>> ByPoly { get; private set; }

    public static CurrentConstruction Create(GenWriteKey key)
    {
        var cc = new CurrentConstruction(key.IdDispenser.GetID(), 
            new Dictionary<PolyTriPosition, Construction>(),
            new Dictionary<int, List<Construction>>());
        key.Create(cc);
        return cc;
    }
    [SerializationConstructor] private CurrentConstruction(int id, Dictionary<PolyTriPosition, 
            Construction> byTri,
        Dictionary<int, List<Construction>> byPoly) : base(id)
    {
        ByTri = byTri;
        ByPoly = byPoly;
    }

    public List<Construction> GetPolyConstructions(MapPolygon poly)
    {
        return ByPoly.ContainsKey(poly.Id)
            ? ByPoly[poly.Id]
            : null;
    }
    public void StartConstruction(Construction construction, ProcedureWriteKey key)
    {
        var poly = construction.Pos.Poly(key.Data);
        ByPoly.AddOrUpdate(poly.Id, construction);
        if (ByTri.ContainsKey(construction.Pos)) throw new Exception("already constructing in tri");
        ByTri.Add(construction.Pos, construction);
    }
    public void FinishConstruction(MapPolygon poly, PolyTriPosition pos, ProcedureWriteKey key)
    {
        ByPoly[poly.Id].RemoveAll(c => c.Pos.Equals(pos));
        ByTri.Remove(pos);
    }

    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
}
