using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class PolyPeep : Entity
{
    public EntityRef<MapPolygon> Poly { get; private set; }
    public Dictionary<int, PeepClassFragment> ClassFragments { get; private set; }
    public int Size() => ClassFragments.Sum(kvp => kvp.Value.Size);

    public static PolyPeep Create(MapPolygon poly, CreateWriteKey key)
    {
        var p = new PolyPeep(poly.MakeRef(), new Dictionary<int, PeepClassFragment>(), 
            key.IdDispenser.GetID());
        key.Create(p);
        return p;
    }
    [SerializationConstructor] private PolyPeep(EntityRef<MapPolygon> poly,
        Dictionary<int, PeepClassFragment> classFragments, int id) : base(id)
    {
        ClassFragments = classFragments; 
        Poly = poly;
    }

    public void GrowSize(int delta, PeepClass peepClass, ProcedureWriteKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        if (ClassFragments.ContainsKey(peepClass.Id) == false)
        {
            ClassFragments.Add(peepClass.Id, new PeepClassFragment(0, peepClass.MakeRef()));
        }
        ClassFragments[peepClass.Id].Grow(delta);
    }
    public void GrowSize(int delta, PeepClass peepClass, GenWriteKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        if (ClassFragments.ContainsKey(peepClass.Id) == false)
        {
            ClassFragments.Add(peepClass.Id, new PeepClassFragment(0, peepClass.MakeRef()));
        }
        ClassFragments[peepClass.Id].Grow(delta);
    }

    public void ShrinkSize(int delta, PeepClass peepClass, ProcedureWriteKey key)
    {
        if (delta == 0) return;
        if (delta < 0) throw new Exception();
        ClassFragments[peepClass.Id].Shrink(delta);
    }

    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
}
