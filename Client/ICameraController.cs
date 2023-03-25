using Godot;

public interface ICameraController 
{
    Vector2 GetMousePosInMapSpace();
    Vector2 GetMapPosInGlobalSpace(Vector2 mapPos);
    Vector2 GetGlobalMousePosition();
    float XScrollRatio { get; }

}