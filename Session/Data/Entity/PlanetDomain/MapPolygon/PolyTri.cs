
using System;
using Godot;

public class PolyTri : Triangle
{
    public Landform Landform => LandformModel.Model();
    public Vegetation Vegetation => VegetationModel.Model();
    public ModelRef<Landform> LandformModel { get; private set; }
    public ModelRef<Vegetation> VegetationModel { get; private set; }

    public PolyTri(Vector2 a, Vector2 b, Vector2 c, ModelRef<Landform> landformModel, 
        ModelRef<Vegetation> vegetationModel)
    : base(a,b,c)
    {
        LandformModel = landformModel;
        VegetationModel = vegetationModel;
    }

    public void SetLandform(Landform lf, GenWriteKey key)
    {
        LandformModel = lf.MakeRef();
    }
    public void SetVegetation(Vegetation v, GenWriteKey key)
    {
        VegetationModel = v.MakeRef();
    }
    public PolyTri Transpose(Vector2 offset)
    {
        return new PolyTri(A + offset, B + offset, C + offset, LandformModel.Copy(), VegetationModel.Copy());
    }
}
