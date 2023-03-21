using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{

    public static ClientSettings Load()
    {
        return new ClientSettings();
    }

    private ClientSettings() : base("Client")
    {
        _options.Add(PolyHighlightMode);
    }
    
    public EnumSettingsOption<PolyHighlighter.Modes> PolyHighlightMode =
        new EnumSettingsOption<PolyHighlighter.Modes>("Poly Highlight Mode", PolyHighlighter.Modes.Simple);
}
