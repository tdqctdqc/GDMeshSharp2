
using Godot;

public abstract class BuildingModel : IModel
{
    public string Name { get; }
    public int NumTicksToBuild { get; private set; }
    public int LaborPerTickToBuild { get; private set; }
    public Icon BuildingIcon { get; }
    public BuildingModel(string name, int numTicksToBuild, int laborPerTickToBuild)
    {
        Name = name;
        NumTicksToBuild = numTicksToBuild;
        LaborPerTickToBuild = laborPerTickToBuild;
        BuildingIcon = Icon.Create(Name, Icon.AspectRatio._1x1, 25f);
    }

    protected abstract bool CanBuildInTriSpec(PolyTri t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);

    public bool CanBuildInTri(PolyTri t, Data data)
    {
        return t.GetBuilding(data) == null && CanBuildInTriSpec(t, data);
    }

    public abstract int Capacity { get; }
    public abstract float GetPolyEfficiencyScore(MapPolygon poly, Data data);
    public abstract float GetTriEfficiencyScore(PolyTriPosition pos, Data data);
}
