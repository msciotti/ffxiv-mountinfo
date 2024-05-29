using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MountInfo;

internal class Service
{
    [PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] internal static ICommandManager CommandManager { get; private set; }
    [PluginService] internal static IClientState ClientState { get; private set; }
    [PluginService] internal static ITargetManager TargetManager { get; private set; }
    [PluginService] internal static IObjectTable ObjectTable { get; private set; }
    [PluginService] internal static IGameGui GameGui { get; private set; }
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; }

    internal static void Initialize(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
    }
}
