using System;
using System.Collections.Generic;
using System.Linq;

public class ClientSettings : Settings
{
    private static ClientSettings _settings;
    public static ClientSettings Load()
    {
        if(_settings == null) _settings = new ClientSettings();
        return _settings;
    }

    private ClientSettings() : base("Client")
    {
        _options.Add(PolyHighlightMode);
    }
    
    public EnumSettingsOption<PolyHighlighter.Modes> PolyHighlightMode =
        new EnumSettingsOption<PolyHighlighter.Modes>("Poly Highlight Mode", PolyHighlighter.Modes.Simple);
}
