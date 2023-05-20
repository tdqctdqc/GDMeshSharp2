using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RiverPolyTriGen
{
    public void DoRivers(GenWriteKey key)
    {
        var rd = TempRiverData.Construct(key);

        key.Data.Planet.PolygonAux.LandSea.Landmasses.ForEach(lm => DoLandmass(lm, key));
        key.Data.Notices.SetPolyShapes?.Invoke();
        rd.GenerateInfos(key);
        key.Data.Notices.SetPolyShapes?.Invoke();
    }
    private void DoLandmass(HashSet<MapPolygon> lm, GenWriteKey key)
    {
        var riverNexi = key.Data.Planet.PolyNexi.Entities
            .Where(n => n.IncidentPolys.Any(p => lm.Contains(p)))
            .Where(n => n.IncidentEdges.Any(e => e.IsRiver()));
        var riverPolys = riverNexi.SelectMany(n => n.IncidentPolys).Distinct();
        MakeInners(riverNexi, key);
        MakePivots(riverNexi, key);
    }

    private void MakeInners(IEnumerable<MapPolyNexus> rNexi, GenWriteKey key)
    {
        var rd = key.Data.Planet.GetRegister<TempRiverData>().Entities.First();
 
        foreach (var nexus in rNexi)
        {
            var rEdges = nexus.IncidentEdges.Where(e => e.IsRiver());
            if(rEdges.Count() == 0) continue;
            var width = rEdges.Select(re => River.GetWidthFromFlow(re.MoistureFlow)).Average();
            
            foreach (var poly in nexus.IncidentPolys)
            {
                var offset = poly.GetOffsetTo(nexus.Point, key.Data);
                if (width > offset.Length()) throw new Exception();
                var inner = offset.Normalized() * (offset.Length() - width);
                // inner = inner.Intify();
                rd.Inners.Add(new PolyCornerKey(nexus, poly), inner);
            }
        }
    }

    private void MakePivots(IEnumerable<MapPolyNexus> rNexi, GenWriteKey key)
    {
        var rd = key.Data.Planet.GetRegister<TempRiverData>().Entities.First();
        foreach (var nexus in rNexi)
        {
            var edges = nexus.IncidentEdges;
            foreach (var edge in edges)
            {
                var cornerKey = new EdgeEndKey(nexus, edge);
                var k = new PolyCornerKey(nexus, edge.HighPoly.Entity());
                var hiPolyInner = rd.Inners[k];
                var loPolyInner = rd.Inners[new PolyCornerKey(nexus, edge.LowPoly.Entity())];
                var loInnerRelToHi = loPolyInner + edge.HighPoly.Entity().GetOffsetTo(edge.LowPoly.Entity(), key.Data);
                var pivot = (hiPolyInner + loInnerRelToHi) / 2f;
                // pivot = pivot.Intify();
                
                rd.HiPivots.Add(new EdgeEndKey(nexus, edge), pivot);

                var oldHiSegs = edge.HighSegsRel().Segments.Select(s => new LineSegment(s.From, s.To)).ToList();
                var first = oldHiSegs.First().From;
                var newHiSegs = new List<LineSegment>();
                for (var i = 0; i < oldHiSegs.Count; i++)
                {
                    var seg = oldHiSegs[i];
                    var fromAngle = first.GetClockwiseAngleTo(seg.From);
                    var pivotAngle = first.GetClockwiseAngleTo(pivot);
                    var toAngle = first.GetClockwiseAngleTo(seg.To);
                    if (
                        pivotAngle >= fromAngle && pivotAngle <= toAngle
                        || 
                        pivotAngle <= fromAngle && pivotAngle >= toAngle
                        )
                    {
                        newHiSegs.Add(new LineSegment(seg.From, pivot));
                        newHiSegs.Add(new LineSegment(pivot, seg.To));
                    }
                    else
                    {
                        newHiSegs.Add(new LineSegment(seg.From, seg.To));
                    }
                }
                // newHiSegs.CorrectSegmentsToClockwise(Vector2.Zero);
                // newHiSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);
                var newAbsSegs = newHiSegs.Select(s => s.Translate(edge.HighPoly.Entity().Center)).ToList();
                edge.ReplacePoints(newAbsSegs, key);
                newHiSegs = edge.HighSegsRel().Segments;

                var hiNexusP = edge.HiNexus.Entity().Point;
                var hiNexusRel = edge.HighPoly.Entity().GetOffsetTo(hiNexusP, key.GenData);
                var loNexusP = edge.LoNexus.Entity().Point;
                var loNexusRel = edge.HighPoly.Entity().GetOffsetTo(loNexusP, key.GenData);

                float epsilon = .1f;
                var hiFromDist = hiNexusRel.DistanceTo(newHiSegs.First().From);
                var hiToDist = hiNexusRel.DistanceTo(newHiSegs.Last().To);
                if (hiFromDist > epsilon
                    && hiToDist > epsilon)
                {
                    GD.Print("hi nexus rel to From " + hiFromDist);
                    GD.Print("hi nexus rel to To " + hiToDist);
                    GD.Print("pivot " + pivot);
                    GD.Print("OLD");
                    for (var i = 0; i < oldHiSegs.Count; i++)
                    {
                        GD.Print(oldHiSegs[i].ToString());
                    }
                    GD.Print("NEW");
                    for (var i = 0; i < newHiSegs.Count; i++)
                    {
                        GD.Print(newHiSegs[i].ToString());
                    }
                    throw new Exception();
                }
                
                var loFromDist = loNexusRel.DistanceTo(newHiSegs.First().From);
                var loToDist = loNexusRel.DistanceTo(newHiSegs.Last().To);
                if (loFromDist > epsilon
                    && loToDist > epsilon)
                {
                    GD.Print("lo nexus rel to From " + loFromDist);
                    GD.Print("lo nexus rel to To " + loToDist);
                    GD.Print("pivot " + pivot);

                    GD.Print("OLD");
                    for (var i = 0; i < oldHiSegs.Count; i++)
                    {
                        GD.Print(oldHiSegs[i].ToString());
                    }
                    GD.Print("NEW");
                    for (var i = 0; i < newHiSegs.Count; i++)
                    {
                        GD.Print(newHiSegs[i].ToString());
                    }
                    throw new Exception();
                }

                if (newHiSegs.IsContinuous() == false)
                {
                    GD.Print("not continuous");
                    GD.Print("pivot " + pivot);
                    GD.Print("OLD");
                    for (var i = 0; i < oldHiSegs.Count; i++)
                    {
                        GD.Print(oldHiSegs[i].ToString());
                    }
                    GD.Print("NEW");
                    for (var i = 0; i < newHiSegs.Count; i++)
                    {
                        GD.Print(newHiSegs[i].ToString());
                    }

                    throw new Exception();
                }

                
            }
        }
    }
}