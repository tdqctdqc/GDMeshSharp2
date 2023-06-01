
using System;
using Godot;

public class PolyTri : Triangle
{
    public byte Index { get; private set; }
    public int NeighborStartIndex { get; private set; }
    public int NeighborCount { get; private set; }
    public Landform Landform => LandformModel.Model();
    public Vegetation Vegetation => VegetationModel.Model();
    public ModelRef<Landform> LandformModel { get; private set; }
    public ModelRef<Vegetation> VegetationModel { get; private set; }

    public static PolyTri Construct(Vector2 a, Vector2 b, Vector2 c, ModelRef<Landform> landformModel, 
        ModelRef<Vegetation> vegetationModel)
    {
        return new PolyTri(a, b, c, landformModel, vegetationModel,
            (byte) 255, -1, 0);
    }
    public PolyTri(Vector2 a, Vector2 b, Vector2 c, ModelRef<Landform> landformModel, 
        ModelRef<Vegetation> vegetationModel, byte index, int neighborStartIndex, int neighborCount)
    : base(a,b,c)
    {
        Index = index;
        LandformModel = landformModel;
        VegetationModel = vegetationModel;
        NeighborCount = neighborCount;
        NeighborStartIndex = neighborStartIndex;
    }

    public void ForEachNeighbor(MapPolygon poly, Action<PolyTri> func)
    {
        for (var i = 0; i < NeighborCount; i++)
        {
            var n = poly.Tris.TriNativeNeighbors[i + NeighborStartIndex];
            var nTri = poly.Tris.Tris[n];
            func(nTri);
        }
    }
    public bool AnyNeighbor(MapPolygon poly, Func<PolyTri, bool> func)
    {
        for (var i = 0; i < NeighborCount; i++)
        {
            var n = poly.Tris.TriNativeNeighbors[i + NeighborStartIndex];
            var nTri = poly.Tris.Tris[n];
            if(func(nTri)) return true;
        }

        return false;
    }
    public void SetLandform(Landform lf, GenWriteKey key)
    {
        LandformModel = lf.MakeRef();
    }
    public void SetVegetation(Vegetation v, GenWriteKey key)
    {
        VegetationModel = v.MakeRef();
    }

    public void SetNeighborStart(int start, GenWriteKey key)
    {
        NeighborStartIndex = start;
    }
    public void SetNeighborCount(int count, GenWriteKey key)
    {
        NeighborCount = count;
    }
    public void SetIndex(byte index, GenWriteKey key)
    {
        Index = index;
    }
    public PolyTri Transpose(Vector2 offset)
    {
        return new PolyTri(A + offset, B + offset, C + offset, 
            LandformModel.Copy(), VegetationModel.Copy(), Index,
            NeighborStartIndex, NeighborCount);
    }
}
