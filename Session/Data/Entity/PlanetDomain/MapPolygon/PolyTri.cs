
using System;
using Godot;

public class PolyTri : Triangle
{
    public byte Index { get; private set; }
    public Landform Landform => LandformModel.Model();
    public Vegetation Vegetation => VegetationModel.Model();
    public ModelRef<Landform> LandformModel { get; private set; }
    public ModelRef<Vegetation> VegetationModel { get; private set; }

    public PolyTri(Vector2 a, Vector2 b, Vector2 c, ModelRef<Landform> landformModel, 
        ModelRef<Vegetation> vegetationModel, byte index)
    : base(a,b,c)
    {
        Index = index;
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

    public void SetIndex(byte index, GenWriteKey key)
    {
        Index = index;
    }
    public PolyTri Transpose(Vector2 offset)
    {
        return new PolyTri(A + offset, B + offset, C + offset, 
            LandformModel.Copy(), VegetationModel.Copy(), Index);
    }
}
