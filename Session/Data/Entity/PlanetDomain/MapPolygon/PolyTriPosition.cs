
public class PolyTriPosition
{
    public EntityRef<MapPolygon> Poly { get; private set; }
    public int TriIndex { get; private set; }
    public PolyTri Tri() => Poly.Entity().TerrainTris.Tris[TriIndex];
    
    public PolyTriPosition(EntityRef<MapPolygon> poly, int triIndex)
    {
        Poly = poly;
        TriIndex = triIndex;
    }
}
