
public class BuildingAux : EntityAux<Building>
{
    public AuxMultiIndexer<MapPolygon, Building> ByPoly { get; private set; }
    public Entity1to1PropIndexer<Building, PolyTri> ByTri { get; private set; }
    public BuildingAux(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = AuxMultiIndexer<MapPolygon, Building>.ConstructConstant(
            data, 
            b => b.Position.Poly(data));
        ByTri = Entity1to1PropIndexer<Building, PolyTri>.CreateConstant(
            data, 
            b => b.Position.Tri(data));
    }
}
