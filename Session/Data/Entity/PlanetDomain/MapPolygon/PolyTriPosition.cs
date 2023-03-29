
using MessagePack;

public struct PolyTriPosition
{
    public int TriIndex { get; private set; }
    public int PolyId { get; private set; }
    public PolyTri Tri(Data data) => Poly(data).TerrainTris.Tris[TriIndex];
    public MapPolygon Poly(Data data) => (MapPolygon)data[PolyId];
    [SerializationConstructor] public PolyTriPosition(int polyId, int triIndex)
    {
        PolyId = polyId;
        TriIndex = triIndex;
    }
    
}
