
using MessagePack;

public class PolyTriPositionSerializable
{
    public EntityRef<MapPolygon> Poly { get; private set; }
    public int TriIndex { get; private set; }
    public PolyTri Tri() => Poly.Entity().TerrainTris.Tris[TriIndex];
    
    [SerializationConstructor] public PolyTriPositionSerializable(EntityRef<MapPolygon> poly, int triIndex)
    {
        Poly = poly;
        TriIndex = triIndex;
    }

    public PolyTriPositionSerializable(MapPolygon poly, PolyTri tri)
    {
        Poly = poly.MakeRef();
        TriIndex = poly.TerrainTris.Tris.IndexOf(tri);
    }
    
}
