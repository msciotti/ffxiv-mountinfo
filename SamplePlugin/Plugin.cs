using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using System;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System.Numerics;
using System.Collections.Generic;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using SamplePlugin.Windows;
using Dalamud.Interface.Windowing;
using Lumina.Extensions;
using System.Linq;

namespace SamplePlugin;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Mount Plugin";

    private DalamudPluginInterface PluginInterface { get; init; }
    public Configuration Configuration { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private ITargetManager TargetManager { get; init; }
    private IObjectTable ObjectTable { get; init; }
    private IGameGui GameGui { get; init; }
    private ITextureProvider TextureProvider { get; init; }
    private GameData GameData { get; init; }
    public readonly WindowSystem WindowSystem = new("SamePlugin");
    private ConfigWindow ConfigWindow { get; init; }
    private bool showWindow;

    // Dictionary to cache mount icons
    private Dictionary<uint, IDalamudTextureWrap> mountIconCache = new Dictionary<uint, IDalamudTextureWrap>();
    private const int MaxCacheSize = 100;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] IClientState clientState,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] ITargetManager targetManager,
        [RequiredVersion("1.0")] IObjectTable objectTable,
        [RequiredVersion("1.0")] IGameGui gameGui,
        [RequiredVersion("1.0")] ITextureProvider textureProvider) {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        TargetManager = targetManager;
        ObjectTable = objectTable;
        GameGui = gameGui;
        TextureProvider = textureProvider;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        var sqPackPath = "D:\\Games\\Steam\\steamapps\\common\\FINAL FANTASY XIV ONLINE\\game\\sqpack";
        if (string.IsNullOrEmpty(sqPackPath))
        {
            throw new InvalidOperationException("Unable to find sqpack path for FFXIV");
        }
        GameData = new GameData(sqPackPath);

        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler("/mountinfo", new CommandInfo(OnCommand)
        {
            HelpMessage = "Show mount information of nearby players"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += () => ConfigWindow.Toggle();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        CommandManager.RemoveHandler("/mountinfo");
        PluginInterface.UiBuilder.Draw -= DrawUI;
        GameData?.Dispose();
        // Free cached textures
        mountIconCache.Clear();
    }

    private void OnCommand(string command, string arguments)
    {
        showWindow = !showWindow;
    }

    private unsafe void DrawUI()
    {
        if (!showWindow) return;

        var target = TargetManager.Target;
        if (target != null && target is PlayerCharacter playerCharacter)
        {
            var mountID = GetMountID(playerCharacter);
            if (mountID > 0)
            {
                var mountName = GetMountNameById(mountID);
                var mountIconID = GetMountIconID(mountID);

                var nameplatePosition = GetTargetHealthBarPosition(playerCharacter);
                if (nameplatePosition.HasValue)
                {
                    
                    var iconPosition = nameplatePosition.Value + new Vector2(Configuration.xOffset, Configuration.yOffset); // Adjust position as needed
                    ImGui.SetNextWindowPos(iconPosition, ImGuiCond.Always);
                    if (ImGui.Begin("MountIcon", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
                    {
                        if (TextureProvider.GetIcon(mountIconID) is { ImGuiHandle: var mountIconTextureHandle } unknownTexture)
                        {

                            ImGui.Image(mountIconTextureHandle, ImGuiHelpers.ScaledVector2(Configuration.scale, Configuration.scale));
                            //// Show tooltip with mount name on hover
                            if (ImGui.IsItemHovered())
                            {
                                ImGui.SetTooltip($"Riding: {mountName}");
                            }
                        }
                         
                    }
                    ImGui.End();
                    
                }
            }
        }
    }

    private unsafe uint GetMountID(PlayerCharacter playerCharacter)
    {
        var characterPtr = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)playerCharacter.Address;
        if (characterPtr == null) return 0;
        var mountContainer = characterPtr->Mount;

        var mountObjectID = mountContainer.MountId;
        if (mountObjectID == 0) return 0;

        return mountObjectID;
    }

    private unsafe string GetMountNameById(uint mountObjectID)
    {
        if (GameData != null)
        {
            var mountRow = GameData.GetExcelSheet<Mount>().GetRow(mountObjectID);
            if (mountRow != null)
            {
                return mountRow.Singular;
            }
        }
        return "Unknown Mount";
    }
    
    private uint GetMountIconID(uint mountID)
    {
        var mountRow = GameData.GetExcelSheet<Mount>().GetRow(mountID);
        if (mountRow == null) return 0;
        return mountRow.Icon;
    }

    private unsafe Vector2? GetTargetHealthBarPosition(PlayerCharacter playerCharacter)
    {        
        var targetInfoHud = (AtkUnitBase*)GameGui.GetAddonByName("_TargetInfo");
        if (targetInfoHud == null) return null;

        var healthBarNode = (AtkResNode*)targetInfoHud->RootNode->ChildNode;
        if (healthBarNode == null) return null;

        // Get screen coordinates from the node
        var x = healthBarNode->ScreenX;
        var y = healthBarNode->ScreenY;
        return new Vector2(x, y);
    }

    private unsafe bool GetTargetHealthBarFocused(PlayerCharacter playerCharacter)
    {
        var targetInfoHud = (AtkUnitBase*)GameGui.GetAddonByName("_TargetInfo");
        if (targetInfoHud == null) return false;

        var healthBarNode = (AtkResNode*)targetInfoHud->RootNode->ChildNode;
        if (healthBarNode == null) return false;

        return healthBarNode->IsVisible;
    }
}
