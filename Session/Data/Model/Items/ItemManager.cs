
using System.Collections.Generic;

public class ItemManager : IModelManager<Item>
{
    public static Food Food { get; private set; } = new Food();
    public static Recruits Recruits { get; private set; } = new Recruits();
    public static Iron Iron { get; private set; } = new Iron();
    public static Oil Oil { get; private set; } = new Oil();
    public Dictionary<string, Item> Models { get; private set; }
        = new Dictionary<string, Item>
        {
            {Food.Name, Food},
            {Recruits.Name, Recruits},
            {Iron.Name, Iron},
            {Oil.Name, Oil},
        };
}
