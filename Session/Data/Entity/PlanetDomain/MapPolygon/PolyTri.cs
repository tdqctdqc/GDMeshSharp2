
using System;
using Godot;

public class PolyTri : Triangle
{
    public int Id { get; private set; }
    public Landform Landform => LandformModel.Ref();
    public Vegetation Vegetation => VegetationModel.Ref();
    public ModelRef<Landform> LandformModel { get; private set; }
    public ModelRef<Vegetation> VegetationModel { get; private set; }

    public PolyTri(int id, Vector2 a, Vector2 b, Vector2 c, ModelRef<Landform> landformModel, 
        ModelRef<Vegetation> vegetationModel)
    : base(a,b,c)
    {
        Id = id;
        LandformModel = landformModel;
        VegetationModel = vegetationModel;
    }

    public void SetLandform(Landform lf, GenWriteKey key)
    {
        LandformModel = lf.GetRef();
    }
    public void SetVegetation(Vegetation v, GenWriteKey key)
    {
        VegetationModel = v.GetRef();
    }
    public PolyTri Transpose(Vector2 offset)
    {
        return new PolyTri(Id, A + offset, B + offset, C + offset, LandformModel.Copy(), VegetationModel.Copy());
    }
}
