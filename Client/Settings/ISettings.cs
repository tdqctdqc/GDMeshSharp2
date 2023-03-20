using System;
using System.Collections.Generic;
using System.Linq;
using gdMeshSharp2.Client.Settings;

public interface ISettings
{
    IReadOnlyList<ISettingsOption> Options { get; }
}
