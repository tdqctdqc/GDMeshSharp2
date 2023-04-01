
using System;

public class BuildingAux : EntityAux<Building>
{
    public RefAction<Tuple<Building, int>> LaborersDelta { get; private set; }
    public AuxMultiIndexer<MapPolygon, Building> ByPoly { get; private set; }
    public Entity1to1PropIndexer<Building, PolyTri> ByTri { get; private set; }
    public BuildingAux(Domain domain, Data data) : base(domain, data)
    {
        LaborersDelta = new RefAction<Tuple<Building, int>>();
        ByPoly = AuxMultiIndexer<MapPolygon, Building>.ConstructConstant(
            data, 
            b => b.Position.Poly(data));
        ByTri = Entity1to1PropIndexer<Building, PolyTri>.CreateConstant(
            data, 
            b => b.Position.Tri(data));
    }
}
