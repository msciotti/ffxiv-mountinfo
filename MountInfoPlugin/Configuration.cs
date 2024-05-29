using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace MountInfo;

[Serializable]
public class Configuration : IPluginConfiguration
{
    private DalamudPluginInterface pluginInterface;
    public int Version { get; set; } = 0;


    public float xOffset { get; set; } = -45.0f;
    public float yOffset { get; set; } = 25.0f;
    public float scale { get; set; } = 30.0f;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? PluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public static Configuration Get(DalamudPluginInterface pluginInterface)
    {
        var config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        config.pluginInterface = pluginInterface;
        return config;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
    }
}
