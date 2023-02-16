
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class FindDistTester
{
    public static void RunTest()
    {
        var points = Enumerable.Range(0, 1000)
            .Select(i =>
                Vector2.Right.Rotated(Game.I.Random.RandfRange(0, Mathf.Pi * 2f)
                                      * Game.I.Random.RandfRange(0f, 100f)))
            .ToList();
        
        
        TestSpeed(DistSin, points, 100, nameof(DistSin));
        TestSpeed(DistLong, points, 100, nameof(DistLong));
        TestSpeed(DistProject, points, 100, nameof(DistProject));
        TestError(points, 100);

    }


    private static void TestError(List<Vector2> points, int iter)
    {
        var projError = 0f;
        var sinError = 0f;
        for (var i = 0; i < iter; i++)
        {
            var a = points.GetRandomElement();
            var b = points.GetRandomElement();
            var c = points.GetRandomElement();
            
            var distLong = DistLong(a, b, c);
            var distProject = DistProject(a, b, c);
            var distSin = DistSin(a, b, c);

            var thisProjError = Mathf.Abs(distLong - distProject);
            projError += thisProjError;
            var thisSinError = Mathf.Abs(distLong - distSin);
            sinError += thisSinError;
            if (thisProjError > 1f)
            {
                GD.Print("failure proj " + thisProjError + " out of " + distLong);
            }
            if (thisSinError > 1f)
            {
                GD.Print("failure sin " + thisSinError + " out of " + distLong);
            }
        }
        GD.Print($"avg proj error {projError / iter}");
        GD.Print($"avg sin error {sinError / iter}");
    }
    private static List<float> TestSpeed(Func<Vector2,Vector2,Vector2, float> test, List<Vector2> points, int iter, string tag)
    {
        var res = new List<float>();
        var sw = new Stopwatch();
        sw.Start();
        for (var i = 0; i < iter; i++)
        {
            var a = points.GetRandomElement();
            var b = points.GetRandomElement();
            var c = points.GetRandomElement();
            var dist = test(a, b, c);
            res.Add(dist);
        }
        sw.Stop();
        
        GD.Print($"{tag} {sw.Elapsed.TotalMilliseconds / iter}ms");
        return res;
    }
    private static float DistProject(Vector2 point, Vector2 start, Vector2 end)
    {
        var h = point.DistanceSquaredTo(start);
        var a = (point - start).Project(end - start).LengthSquared();
        return Mathf.Sqrt(h - a);
    }
    private static float DistSin(Vector2 point, Vector2 start, Vector2 end)
    {
        var theta = Mathf.Abs((point - start).AngleTo(end - start));
        return Mathf.Sin(theta) * point.DistanceTo(start);
    }

    private static float DistLong(Vector2 point, Vector2 start, Vector2 end)
    {
        // vector AB
        var AB = new Vector2();
        AB.x = end.x - start.x;
        AB.y = end.y - start.y;
 
        // vector BP
        var BE = new Vector2();
        BE.x = point.x - end.x;
        BE.y = point.y - end.y;
 
        // vector AP
        var AE = new Vector2();
        AE.x = point.x - start.x;
        AE.y = point.y - start.y;
 
        // Variables to store dot product
        float AB_BE, AB_AE;
 
        // Calculating the dot product
        AB_BE = (AB.x * BE.x + AB.y * BE.y);
        AB_AE = (AB.x * AE.x + AB.y * AE.y);
 
        // Minimum distance from
        // point E to the line segment
        float reqAns = 0;
 
        // Case 1
        if (AB_BE > 0)
        {
 
            // Finding the magnitude
            var y = point.y - end.y;
            var x = point.x - end.x;
            reqAns = Mathf.Sqrt(x * x + y * y);
        }
 
        // Case 2
        else if (AB_AE < 0)
        {
            var y = point.y - start.y;
            var x = point.x - start.x;
            reqAns = Mathf.Sqrt(x * x + y * y);
        }
 
        // Case 3
        else
        {
 
            // Finding the perpendicular distance
            var x1 = AB.x;
            var y1 = AB.y;
            var x2 = AE.x;
            var y2 = AE.y;
            var mod = Mathf.Sqrt(x1 * x1 + y1 * y1);
            reqAns = Mathf.Abs(x1 * y2 - y1 * x2) / mod;
        }
        return reqAns;
    }
}
