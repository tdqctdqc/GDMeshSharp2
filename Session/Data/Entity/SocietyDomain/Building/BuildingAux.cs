
public class BuildingAux : EntityAux<Building>
{
    public AuxMultiIndexer<MapPolygon, Building> ByPoly { get; private set; }
    public BuildingAux(Domain domain, Data data) : base(domain, data)
    {
        ByPoly = AuxMultiIndexer<MapPolygon, Building>.ConstructConstant(
            data, 
            b => b.Position.Poly.Entity());
    }
}
