
using System.Collections.Generic;
using Godot;

public static class PolyTriExt
{
    public static float GetFertility(this PolyTri t)
    {
        return t.Landform.FertilityMod * t.Vegetation.FertilityMod;
    }
    public static bool HasBuilding(this PolyTri t, Data data)
    {
        return data.Society.BuildingAux.ByTri[t] != null;
    }
    public static MapBuilding GetBuilding(this PolyTri t, Data data)
    {
        return data.Society.BuildingAux.ByTri[t];
    }
    
    public static List<PolyTri> TriangulateArbitrary(this IReadOnlyList<LineSegment> outline, MapPolygon poly, 
        GenWriteKey key, Graph<PolyTri, bool> graph, bool generateInterior)
    {
        HashSet<Vector2> interior = generateInterior 
            ? outline.GenerateInteriorPoints(30f, 10f).ToHashSet()
            : null;
        return outline.PolyTriangulate(key.GenData, poly, key.IdDispenser, graph, interior);
    }
}
