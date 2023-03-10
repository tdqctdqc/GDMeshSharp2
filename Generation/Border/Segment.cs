
public class Segment<TPrim> : ISegment<TPrim>
{
    public TPrim From { get; }
    public TPrim To { get; }
    public ISegment<TPrim> ReverseGeneric()
    {
        throw new System.NotImplementedException();
    }

    public bool PointsTo(ISegment<TPrim> s)
    {
        throw new System.NotImplementedException();
    }

    public bool ComesFrom(ISegment<TPrim> s)
    {
        throw new System.NotImplementedException();
    }
}
