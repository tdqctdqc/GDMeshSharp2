using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class MapPolygonExt
{
    
    public static bool PointInPoly(this MapPolygon poly, Vector2 posAbs, Data data)
    {
        return data.Cache.PolyRelWheelTris[poly].Any(t => t.ContainsPoint(poly.GetOffsetTo(posAbs, data)));
    }
    
    public static Vector2 GetOffsetTo(this MapPolygon poly, MapPolygon p, Data data)
    {
        var off1 = p.Center - poly.Center;
        var off2 = (off1 + Vector2.Right * data.Planet.Width);
        var off3 = (off1 + Vector2.Left * data.Planet.Width);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }
    public static Vector2 GetOffsetTo(this MapPolygon poly, Vector2 p, Data data)
    {
        var off1 = p - poly.Center;
        var off2 = (off1 + Vector2.Right * data.Planet.Width);
        var off3 = (off1 + Vector2.Left * data.Planet.Width);
        if (off1.Length() < off2.Length() && off1.Length() < off3.Length()) return off1;
        if (off2.Length() < off1.Length() && off2.Length() < off3.Length()) return off2;
        return off3;
    }

    public static int GetNumPeeps(this MapPolygon poly, Data data)
    {
        if (data.Planet.Polygons.PeepsInPoly[poly] is HashSet<Peep> h)
        {
            return h.Count;
        }
        return 0;
    }

    public static float GetArea(this MapPolygon poly, Data data)
    {
        return data.Cache.PolyRelWheelTris[poly].Sum(t => t.GetArea());
    }
    
    
    public static float GetScore(this MapPolygon poly, MapPolygon closest, MapPolygon secondClosest, 
        Vector2 pRel, Data data, Func<MapPolygon, float> getScore)
    {
        var l = pRel.Length();
        var closeL = (poly.GetOffsetTo(closest, data) - pRel).Length();
        var secondCloseL = (poly.GetOffsetTo(secondClosest, data) - pRel).Length();
        var totalDist = l + closeL + secondCloseL;
        var closeInt = .5f * Mathf.Lerp(getScore(poly), getScore(closest), closeL / (l + closeL));
        var secondInt = .5f * Mathf.Lerp(getScore(poly), getScore(secondClosest), secondCloseL / (l + secondCloseL));

        return closeInt + secondInt;
    }
    
}