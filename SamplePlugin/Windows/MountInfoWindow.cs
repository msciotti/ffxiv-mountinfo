using System;
using System.Numerics;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;


namespace SamplePlugin.Windows;

public class MountInfoWindow : Window, IDisposable
{
    private readonly Plugin plugin;

    public MountInfoWindow(Plugin plugin) : base("Mount Plugin Main", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoBackground)
    {
        this.plugin = plugin;
        Size = new Vector2(275, 275);
    }

    public void Dispose() { }

    public override void PreDraw() 
    {
        this.Position = DrawHealthBarPosition();
    }  
    
    public Vector2? DrawHealthBarPosition()
    {
        var target = Service.TargetManager.Target;
        if (target != null && target is PlayerCharacter playerCharacter)
        {
            return Helpers.GetTargetHealthBarPosition(playerCharacter) + new Vector2(plugin.Configuration.xOffset, plugin.Configuration.yOffset);
        }
        return Vector2.Zero;
    }

    public override void Draw()
    {
        var target = Service.TargetManager.Target;
        if (target != null && target is PlayerCharacter playerCharacter)
        {
            var mountID = Helpers.GetMountID(playerCharacter);
            if (mountID > 0)
            {
                var mountName = Helpers.GetMountNameById(mountID);
                var mountIconID = Helpers.GetMountIconID(mountID);

                if (Service.TextureProvider.GetIcon(mountIconID) is { ImGuiHandle: var mountIconTextureHandle } unknownTexture)
                {
                    ImGui.Image(mountIconTextureHandle, ImGuiHelpers.ScaledVector2(plugin.Configuration.scale, plugin.Configuration.scale));
                    //// Show tooltip with mount name on hover
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip($"Riding: {mountName}");
                    }
                }
            }
        }
    }
}
