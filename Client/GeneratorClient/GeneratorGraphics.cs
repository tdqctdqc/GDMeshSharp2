using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;

public class GeneratorGraphics : GameGraphics
{

    public override void _Ready()
    {
    }

    public void SetupGenerator(WorldData data, GeneratorClient client)
    {
        
        
        // var faultLineGraphics = data.GenAuxData.FaultLines.Select(f => new FaultLineGraphic(f, data)).ToList();
        //
        // var faultLineNode = new GraphicsSegmenter<FaultLineGraphic>();
        // _segmenters.Add(faultLineNode);
        // faultLineNode.Setup(faultLineGraphics, 10, m => m.Origin, data);
        // AddChild(faultLineNode);
    }

}