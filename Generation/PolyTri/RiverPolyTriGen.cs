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

                var hiSegs = edge.HighSegsRel().Segments;
                var first = hiSegs.First().From;
                var newHiSegs = new List<LineSegment>();
                for (var i = 0; i < hiSegs.Count; i++)
                {
                    var seg = hiSegs[i];
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

                var newAbsSegs = newHiSegs.Select(s => s.Translate(edge.HighPoly.Entity().Center)).ToList();
                // newAbsSegs.CorrectSegmentsToClockwise(Vector2.Zero);
                // newAbsSegs.OrderByClockwise(Vector2.Zero, ls => ls.From);
                edge.ReplacePoints(newAbsSegs, key);
            }
        }
    }

}
