using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class PolyBuildingSlots
{
    public Dictionary<BuildingType, LinkedList<PolyTriPosition>> AvailableSlots { get; private set; }
    public int this[BuildingType type] => AvailableSlots.ContainsKey(type) ? AvailableSlots[type].Count : 0;

    public static PolyBuildingSlots Construct()
    {
        return new PolyBuildingSlots(new Dictionary<BuildingType, LinkedList<PolyTriPosition>>());
    }
    [SerializationConstructor] private PolyBuildingSlots(Dictionary<BuildingType, LinkedList<PolyTriPosition>> availableSlots)
    {
        AvailableSlots = availableSlots;
    }
    
    public void RemoveSlot(BuildingType type, PolyTriPosition pos)
    {
        var removed = AvailableSlots[type].Remove(pos);
        if (removed == false) throw new Exception();
    }
    public void SetSlotNumbers(MapPolygon poly, StrongWriteKey key)
    {
        var agSlots = poly.Tris.Tris
            .Where(t =>
                t.Landform.IsLand
                && t.Landform.MinRoughness <= LandformManager.Hill.MinRoughness
                && t.Vegetation.MinMoisture >= VegetationManager.Arid.MinMoisture
                && float.IsNaN(t.GetArea()) == false
            )
            .Sum(t => t.GetArea() * t.Landform.FertilityMod * t.Vegetation.FertilityMod)
            / 2500f;
        
        
        var grazeSlots = poly.Tris.Tris
              .Where(t =>
                  t.Landform.IsLand
                  && t.Landform.MinRoughness <= LandformManager.Hill.MinRoughness
                  && t.Vegetation.MinMoisture < VegetationManager.Grassland.MinMoisture
                  && float.IsNaN(t.GetArea()) == false
              )
              .Sum(t => t.GetArea() 
                        * t.Landform.FertilityMod 
                        * ShapingFunctions.ProjectToRange(t.Vegetation.FertilityMod, 1f, .5f, 1f))
            / 5000f;

        var industrySlots = 5;
        var govSlots = 1;
        var extrSlots = 5;
        var totalSlots = grazeSlots + agSlots + industrySlots + govSlots + extrSlots;
        
        AvailableSlots.Clear();
        var tris = poly.Tris.Tris.Where(t => t.Landform.IsLand)
            .Select(t => t.Index)
            .OrderBy(t => Game.I.Random.Randi())
            .ToHashSet();
        
        if (totalSlots > tris.Count)
        {
            return;
            throw new Exception($"{totalSlots} slots {tris.Count} tris");
        }
        
        AddSlots(BuildingType.Industry, poly, tris, 5);
        AddSlots(BuildingType.Government, poly, tris, 1);
        AddSlots(BuildingType.Extraction, poly, tris, 5);
        AddSlots(BuildingType.Agriculture, poly, tris, Mathf.FloorToInt(agSlots));
        AddSlots(BuildingType.Grazing, poly, tris, Mathf.FloorToInt(grazeSlots));
        if (AvailableSlots[BuildingType.Agriculture].Any(i => AvailableSlots[BuildingType.Industry].Contains(i)))
        {
            throw new Exception();
        }
    }
    private void AddSlots(BuildingType type, MapPolygon poly, HashSet<byte> availTriIds, int num)
    {
        AvailableSlots.Add(type, new LinkedList<PolyTriPosition>());
        for (var i = 0; i < num; i++)
        {
            if (availTriIds.Count == 0) throw new Exception();
            var id = availTriIds.First();
            availTriIds.Remove(id);
            AvailableSlots[type].AddLast(new PolyTriPosition(poly.Id, id));
        }
    }
}
