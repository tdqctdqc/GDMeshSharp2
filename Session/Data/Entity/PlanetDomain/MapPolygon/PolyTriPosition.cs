
using MessagePack;

public struct PolyTriPosition
{
    public int TriIndex { get; private set; }
    public int PolyId { get; private set; }
    public MapPolygon Poly(Data data) => (MapPolygon)data[PolyId];
    [SerializationConstructor] public PolyTriPosition(int polyId, int triIndex)
    {
        PolyId = polyId;
        TriIndex = triIndex;
    }

    public PolyTri Tri(Data data)
    {
        if(TriIndex != -1) return Poly(data).TerrainTris.Tris[TriIndex];
        return null;
    }
}
