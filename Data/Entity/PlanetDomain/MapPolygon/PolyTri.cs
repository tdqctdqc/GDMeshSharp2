
using System;
using Godot;

public class PolyTri : Triangle
{
    public int Id { get; private set; }
    public Landform Landform => LandformModel.Ref();
    public Vegetation Vegetation => VegetationModel.Ref();
    public ModelRef<Landform> LandformModel { get; private set; }
    public ModelRef<Vegetation> VegetationModel { get; private set; }
    
    public int NumNeighborsInPoly { get; private set; }
    public int NumNeighborsOutsidePoly { get; private set; }

    public PolyTri(int id, Vector2 a, Vector2 b, Vector2 c, ModelRef<Landform> landform, 
        ModelRef<Vegetation> vegetation)
    : base(a,b,c)
    {
        Id = id;
        LandformModel = landform;
        VegetationModel = vegetation;
    }
    
    public PolyTri(Vector2 a, Vector2 b, Vector2 c)
        : base(a,b,c)
    {
    }

    public void SetLandform(Landform lf, GenWriteKey key)
    {
        LandformModel = lf.GetRef();
    }
    public void SetVegetation(Vegetation v, GenWriteKey key)
    {
        VegetationModel = v.GetRef();
    }
    public void DoForEachNeighbor(Action<PolyTri, PolyTri> action)
    {
        
    }
}
