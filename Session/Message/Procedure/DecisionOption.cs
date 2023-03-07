
using System;

public class DecisionOption
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Func<RegimeAI, float> GetAiScore { get; private set; }
    public Action<HostWriteKey> Enact { get; private set; }

    public DecisionOption(string name, string description, Func<RegimeAI, float> getAiScore, Action<HostWriteKey> enact)
    {
        Name = name;
        Description = description;
        GetAiScore = getAiScore;
        Enact = enact;
    }
}
