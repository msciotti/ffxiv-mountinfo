using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace MountInfo.UI;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(MountInfoPlugin plugin) : base("MountInfoPlugin###Config", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)
    {
        Size = new Vector2(500, 232);
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw() { }

    public override void Draw()
    {
        var xOffset = Configuration.xOffset;
        if (ImGui.SliderFloat("X Offset", ref xOffset, -100, 200))
        {
            Configuration.xOffset = (int)xOffset;
            Configuration.Save();
        }

        var yOffset = Configuration.yOffset;
        if (ImGui.SliderFloat("Y Offset", ref yOffset, -100, 200))
        {
            Configuration.yOffset = (int)yOffset;
            Configuration.Save();
        }

        var scale = Configuration.scale;
        if (ImGui.SliderFloat("Scale", ref scale, 20.0f, 50.0f))
        {
            Configuration.scale = scale;
            Configuration.Save();
        }
    }
}
