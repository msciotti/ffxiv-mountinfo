using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;


namespace SamplePlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Mount Plugin Config")
    {
        Size = new Vector2(232, 75);
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        var xOffset = Configuration.xOffset;
        if (ImGui.SliderFloat("X Offset", ref xOffset, -100, 100))
        {
            Configuration.xOffset = (int)xOffset;
            Configuration.Save();
        }

        var yOffset = Configuration.yOffset;
        if (ImGui.SliderFloat("Y Offset", ref yOffset, -100, 100))
        {
            Configuration.yOffset = (int)yOffset;
            Configuration.Save();
        }

        var scale = Configuration.scale;
        if (ImGui.SliderFloat("Scale", ref scale, 0.1f, 2.0f))
        {
            Configuration.scale = scale;
            Configuration.Save();
        }

        var movable = Configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            Configuration.IsConfigWindowMovable = movable;
            Configuration.Save();
        }
    }
}
