
using Godot;

public abstract class BuildingModel : IModel
{
    public string Name { get; }
    public float BuildCost { get; private set; }
    public Icon BuildingIcon { get; }
    public BuildingModel(string name, float buildCost)
    {
        Name = name;
        BuildCost = buildCost;
        BuildingIcon = Icon.Create(Name, Icon.AspectRatio._1x1, 25f);
    }

    protected abstract bool CanBuildInTriSpec(PolyTri t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);

    public bool CanBuildInTri(PolyTri t, Data data)
    {
        return t.GetBuilding(data) == null && CanBuildInTriSpec(t, data);
    }
}
