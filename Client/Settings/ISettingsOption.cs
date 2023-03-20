using System;
using Godot;

namespace gdMeshSharp2.Client.Settings
{
    public interface ISettingsOption
    {
        string Name { get; }
        RefAction SettingChanged { get; }
        Control GetControlInterface();
    }
}