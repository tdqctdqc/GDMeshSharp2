using System;
using System.Collections.Generic;
using System.Linq;

public class PeepClassManager : IModelManager<PeepClass>
{
    public static PeepClass Laborer { get; private set; } = new PeepClass(nameof(Laborer));
    public static PeepClass Professional { get; private set; } = new PeepClass(nameof(Professional));
    public static PeepClass Indigenous { get; private set; } = new PeepClass(nameof(Indigenous));
    public Dictionary<string, PeepClass> Models { get; private set; }

    public PeepClassManager()
    {
        Models = GetType().GetStaticPropertiesOfType<PeepClass>().ToDictionary(pj => pj.Name, pj => pj);
    }
}
