
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
}
