using System;
using System.Collections.Generic;
using System.Linq;
using gdMeshSharp2.Client.Settings;

public class ClientSettings : ISettings
{
    public IReadOnlyList<ISettingsOption> Options { get; }

    public static ClientSettings Load()
    {
        return new ClientSettings();
    }

    private ClientSettings()
    {
        Options = new ISettingsOption[]
        {
            PolyHighlightMode
        };
    }
    
    public EnumSettingsOption<PolyHighlighter.Modes> PolyHighlightMode =
        new EnumSettingsOption<PolyHighlighter.Modes>("Poly Highlight Mode", PolyHighlighter.Modes.Simple);
}
