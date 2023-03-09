using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class EdgeDisturber
{
    private static GenWriteKey _key;
    
    public static void SplitEdges(IReadOnlyCollection<MapPolygon> polys, GenWriteKey key, float minLength)
    {
        _key = key;
        var edges = new HashSet<MapPolygonEdge>();

        void AddEdge(MapPolygon p1, MapPolygon p2)
        {
            edges.Add(p1.GetEdge(p2, key.Data));
        }

        int iter = 0;
        foreach (var poly in polys)
        {
            foreach (var n in poly.Neighbors.Refs())
            {
                var edge = poly.GetEdge(n, key.Data);
                if (edges.Contains(edge)) continue;
                edges.Add(edge);

                if (edge.HighSegsRel().Segments.Any(s => s.Length() > minLength * 2f))
                {
                    iter++;
                    edge.SplitToMinLength(minLength, _key);
                }
            }
        }
        GD.Print($"altered {iter} / {edges.Count}" );
    }
    
    public static void DisturbEdges(IReadOnlyCollection<MapPolygon> polys, Vector2 dimensions, GenWriteKey key)
    {
        _key = key;
        var noise = new OpenSimplexNoise();
        noise.Period = dimensions.x;
        noise.Octaves = 2;
        var disturbedEdges = new HashSet<Vector2>();
        for (var i = 0; i < polys.Count; i++)
        {
            var poly = polys.ElementAt(i);
            for (var j = 0; j < poly.Neighbors.Count(); j++)
            {
                var nPoly = poly.Neighbors.Refs().ElementAt(j);
                if (poly.Id > nPoly.Id)
                {
                    DisturbEdge(_key.Data, poly, nPoly, noise);
                }
            }
        }
    }

    private static void DisturbEdge(Data data, MapPolygon highId, MapPolygon lowId, OpenSimplexNoise noise)
    {
        // var border = highId.GetBorder(lowId, _key.Data);
        // var minSegLength = Constants.PreferredMinPolyBorderSegLength;
        // if (border.HighSegsRel.Count != 1) return;
        //
        // var hiSeg = border.HighSegsRel[0];
        // var loSeg = border.LowSegsRel[0];
        // var numSplitPoints = Mathf.FloorToInt(hiSeg.Length() / minSegLength) - 1;
        // if (numSplitPoints < 1) return;
        // var splitLength = hiSeg.Length() / numSplitPoints;
        //
        // var axisHiToLo = border.GetOffsetToOtherPoly(highId);
        //
        // var borderAxisHi = (hiSeg.To - hiSeg.From).Normalized();
        //
        //
        // var newPoints = new List<Vector2> {hiSeg.From};
        // for (int i = 1; i <= numSplitPoints; i++)
        // {
        //     newPoints.Add(hiSeg.From + splitLength * i * borderAxisHi);
        // }
        // newPoints.Add(hiSeg.To);
        // var avgShiftMag = 0f;
        // for (int i = 1; i < newPoints.Count - 1; i++)
        // {
        //     var segBeforeLength = newPoints[i].DistanceTo(newPoints[i - 1]);
        //     var segBeforeFlex = segBeforeLength - minSegLength;
        //     var segAfterLength = newPoints[i].DistanceTo(newPoints[i + 1]);
        //     var segAfterFlex = segAfterLength - minSegLength;
        //
        //     var borderAxisDeviation = Game.I.Random.RandfRange(-segBeforeFlex, segAfterFlex);
        //     newPoints[i] += borderAxisHi * borderAxisDeviation;
        //
        //     var axisFromHi = newPoints[i];
        //     var axisToLo = axisHiToLo - newPoints[i];
        //     var sample = Mathf.Abs(noise.GetNoise2dv(newPoints[i] + highId.Center));
        //     var pushToHi = Game.I.Random.RandfRange(0f, 1f) < .5f;
        //     var shift = pushToHi ? -axisFromHi * sample : axisToLo * sample;
        //     avgShiftMag += shift.Length();
        //     newPoints[i] += shift;
        // }
        // var newSegsHi = new List<LineSegment>();
        // var newSegsLow = new List<LineSegment>();
        //
        // for (var i = 0; i < newPoints.Count - 1; i++)
        // {
        //     var newHiSeg = new LineSegment(newPoints[i], newPoints[i + 1]);
        //     newSegsHi.Add(newHiSeg);
        // }
        // border.ReplacePoints(newSegsHi, _key);
    }
}
