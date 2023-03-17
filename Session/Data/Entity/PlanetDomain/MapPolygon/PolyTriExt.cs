
public static class PolyTriExt
{
    public static float GetFertility(this PolyTri t)
    {
        return t.Landform.FertilityMod * t.Vegetation.FertilityMod * t.GetArea();
    }
}
