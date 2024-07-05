using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace MountInfo;

[Serializable]
public class Configuration : IPluginConfiguration
{
    private IDalamudPluginInterface pluginInterface;
    public int Version { get; set; } = 0;


    public float xOffset { get; set; } = -45.0f;
    public float yOffset { get; set; } = 25.0f;
    public float scale { get; set; } = 30.0f;
    public bool enabled { get; set; } = true;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public static Configuration Get(IDalamudPluginInterface pluginInterface)
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
