
using System;

public class BuildingAux : EntityAux<MapBuilding>
{
    public RefAction<MapPolygon> LaborersDelta { get; private set; }
    public AuxMultiIndexer<MapPolygon, MapBuilding> ByPoly { get; private set; }
    public Entity1to1PropIndexer<MapBuilding, PolyTri> ByTri { get; private set; }
    public BuildingAux(Domain domain, Data data) : base(domain, data)
    {
        LaborersDelta = new RefAction<MapPolygon>();
        ByPoly = AuxMultiIndexer<MapPolygon, MapBuilding>.ConstructConstant(
            data, 
            b => b.Position.Poly(data));
        ByTri = Entity1to1PropIndexer<MapBuilding, PolyTri>.CreateConstant(
            data, 
            b => b.Position.Tri(data));
    }
}
