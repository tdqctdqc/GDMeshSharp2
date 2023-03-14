
using Godot;

public struct CylinderPos
{
    public float Y { get; private set; }
    public float X { get; private set; }
    public float Circum { get; private set; }
    
    public CylinderPos(float x, float y, float circum)
    {
        Circum = circum;
        Y = y;
        X = x % Circum;
    }

    public Vector2 GetV2()
    {
        return new Vector2(X, Y);
    }

    public CylinderPos GetCylPoint(Vector2 p)
    {
        return new CylinderPos(p.x, p.y, Circum);
    }
}
