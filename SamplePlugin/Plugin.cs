using Dalamud.Game.Command;
using Dalamud.Plugin;
using Lumina;
using System;
using SamplePlugin.Windows;
using Dalamud.Interface.Windowing;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    public Configuration Configuration { get; }
    public WindowSystem WindowSystem { get; }
    private ConfigWindow ConfigWindow { get; }
    public MountInfoWindow MountInfoWindow { get; }
    public GameData GameData { get; }

    public Plugin(DalamudPluginInterface pluginInterface) {
        Service.Initialize(pluginInterface);
        Configuration = Configuration.Get(pluginInterface);
        ConfigWindow = new ConfigWindow(this);
        MountInfoWindow = new MountInfoWindow(this);
        WindowSystem = new WindowSystem("SamplePlugin");

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MountInfoWindow);

        pluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        pluginInterface.UiBuilder.OpenMainUi += MountInfoWindow.Toggle;
        pluginInterface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;

        var commandInfo = new CommandInfo((_, _) => MountInfoWindow.Toggle()) { HelpMessage = "Show mount information of nearby players" };
        Service.CommandManager.AddHandler("/mountinfo", commandInfo);
    }

    public void Dispose()
    {
       Service.CommandManager.RemoveHandler("/mountinfo");
    }
}
