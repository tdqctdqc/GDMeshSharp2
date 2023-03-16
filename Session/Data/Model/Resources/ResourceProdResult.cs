
public class ResourceProdResult
{
    public Resource Resource { get; private set; }
    public MapPolygon Poly { get; private set; }
    public float ProdAmount { get; private set; }

    public ResourceProdResult(Resource resource, MapPolygon poly, float prodAmount)
    {
        Resource = resource;
        Poly = poly;
        ProdAmount = prodAmount;
    }
}
