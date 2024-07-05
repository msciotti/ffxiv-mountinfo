using Dalamud.Game.ClientState.Objects;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace MountInfo;

internal class Service
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; }
    [PluginService] internal static ICommandManager CommandManager { get; private set; }
    [PluginService] internal static IClientState ClientState { get; private set; }
    [PluginService] internal static ITargetManager TargetManager { get; private set; }
    [PluginService] internal static IObjectTable ObjectTable { get; private set; }
    [PluginService] internal static IGameGui GameGui { get; private set; }
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; }
    [PluginService] internal static IDataManager DataManager { get; private set; }

    internal static void Initialize(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
    }
}
