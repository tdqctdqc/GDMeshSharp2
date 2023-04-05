
public class RegimeAI
{
    public Regime Regime { get; private set; }
    public ConstructionAI Construction { get; private set; }
    public RegimeAI(Regime regime, Data data)
    {
        Regime = regime;
        Construction = new ConstructionAI(data, regime);
    }
    
}
