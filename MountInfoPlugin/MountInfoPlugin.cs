using Dalamud.Game.Command;
using Dalamud.Plugin;
using MountInfo.UI;
using Dalamud.Interface.Windowing;

namespace MountInfo;

public sealed class MountInfoPlugin : IDalamudPlugin
{
    public Configuration Configuration { get; }
    public WindowSystem WindowSystem { get; }
    private ConfigWindow ConfigWindow { get; }
    public MountInfoWindow MountInfoWindow { get; }

    public MountInfoPlugin(DalamudPluginInterface pluginInterface) {
        Service.Initialize(pluginInterface);
        Configuration = Configuration.Get(pluginInterface);
        ConfigWindow = new ConfigWindow(this);
        MountInfoWindow = new MountInfoWindow(this);
        WindowSystem = new WindowSystem("MountInfoPlugin");

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MountInfoWindow);

        pluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        pluginInterface.UiBuilder.OpenMainUi += MountInfoWindow.Toggle;
        pluginInterface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;

        var commandInfo = new CommandInfo((_, _) => MountInfoWindow.Toggle()) { HelpMessage = "Show mount information of target player" };
        Service.CommandManager.AddHandler("/mountinfo", commandInfo);
    }

    public void Dispose()
    {
       Service.CommandManager.RemoveHandler("/mountinfo");
    }
}
