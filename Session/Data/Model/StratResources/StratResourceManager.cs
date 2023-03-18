
using System.Collections.Generic;

public class StratResourceManager : IModelManager<StratResource>
{
    public static Food Food { get; private set; } = new Food();
    public static Recruits Recruits { get; private set; } = new Recruits();
    public static Iron Iron { get; private set; } = new Iron();
    public static Fuel Fuel { get; private set; } = new Fuel();
    public Dictionary<string, StratResource> Models { get; private set; }
        = new Dictionary<string, StratResource>
        {
            {Food.Name, Food},
            {Recruits.Name, Recruits},
            {Iron.Name, Iron},
            {Fuel.Name, Fuel},
        };
}
