using System;
using System.Collections.Generic;
using System.Linq;

public class PolyPeeps : Entity
{
    public EntityRef<MapPolygon> Poly { get; private set; }
    private PolyPeeps(EntityRef<MapPolygon> poly, int id) : base(id)
    {
        Poly = poly;
    }

    public override Type GetDomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
}
