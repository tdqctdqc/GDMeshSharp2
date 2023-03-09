
using Godot;

public struct CylinderPosition
{
    public float Y { get; private set; }
    public float X { get; private set; }
    public float Circum { get; private set; }

    public CylinderPosition(float y, float x, float circum)
    {
        Circum = circum;
        Y = y;
        X = x % Circum;
    }

    public Vector2 GetV2()
    {
        return new Vector2(X, Y);
    }

    public CylinderPosition GetCylPoint(Vector2 p)
    {
        return new CylinderPosition(p.x, p.y, Circum);
    }
}
