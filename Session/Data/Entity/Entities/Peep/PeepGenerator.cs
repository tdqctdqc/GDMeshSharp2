using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class PeepGenerator
{
    public GenData Data { get; private set; }
    private IDDispenser _id;
    private GenWriteKey _key;

    public PeepGenerator(IDDispenser id, GenWriteKey key, GenData data)
    {
        _id = id;
        _key = key;
        Data = data;
    }

    public void Generate()
    {
        GenerateFarmers();
    }
    private void GenerateFarmers()
    {
        foreach (var poly in Data.Planet.Polygons.Entities)
        {
            var fertility = poly.Moisture * (1f - poly.Roughness);
            var farmerPerFertility = 5;
            var numFarmersInPoly = Mathf.FloorToInt(fertility * farmerPerFertility);
            for (int i = 0; i < numFarmersInPoly; i++)
            {
            }
        }
    }
}