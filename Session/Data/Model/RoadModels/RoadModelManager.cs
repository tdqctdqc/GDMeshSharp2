using System;
using System.Collections.Generic;
using System.Linq;

public class RoadModelManager : IModelManager<RoadModel>
{
    public static PavedRoad PavedRoad { get; private set; } = new PavedRoad();
    public static DirtRoad DirtRoad { get; private set; } = new DirtRoad();
    public static Railroad Railroad { get; private set; } = new Railroad();
    
    
    public Dictionary<string, RoadModel> Models { get; }

    public RoadModelManager()
    {
        Models = new Dictionary<string, RoadModel>
        {
            {nameof(PavedRoad), PavedRoad},
            {nameof(DirtRoad), DirtRoad},
            {nameof(Railroad), Railroad},
        };
        
    }
}
