
    using Godot;

    public class Hill : Landform, IDecaledTerrain
    {
        public Hill() : 
            base("Hill", .4f, .3f, Colors.Brown, .2f)
        {
        }

        public void GetDecal(MeshBuilder mb, PolyTri pt, Vector2 offset)
        {
            var color = pt.Vegetation == VegetationManager.Barren
                ? Colors.Gray
                : pt.Vegetation.Color.Darkened(.4f);
            
            var size = 20f;
            offset += Vector2.Down * size / 2f;
            var p = pt.GetCentroid();
            var t = new Triangle(
                p + Vector2.Left * size + offset,
                p + Vector2.Right * size + offset,
                p + Vector2.Up * size + offset);
            mb.AddTri(t, color);
        }
    }