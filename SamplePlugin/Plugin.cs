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
using Lumina.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MountPlugin;

public sealed class MountPlugin : IDalamudPlugin
{
    public string Name => "Mount Plugin";

    private DalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private IClientState ClientState { get; init; }
    private ITargetManager TargetManager { get; init; }
    private IObjectTable ObjectTable { get; init; }
    private IGameGui GameGui { get; init; }
    private GameData GameData { get; init; }
    private bool showWindow;

    // Dictionary to cache mount icons
    private Dictionary<uint, IntPtr> mountIconCache = new Dictionary<uint, IntPtr>();
    private const int MaxCacheSize = 100;
    private bool previousTargetBarVisibleState = false;

    public MountPlugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] IClientState clientState,
        [RequiredVersion("1.0")] ICommandManager commandManager,
        [RequiredVersion("1.0")] ITargetManager targetManager,
        [RequiredVersion("1.0")] IObjectTable objectTable,
        [RequiredVersion("1.0")] IGameGui gameGui) {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        TargetManager = targetManager;
        ObjectTable = objectTable;
        GameGui = gameGui;

        var sqPackPath = "D:\\Games\\Steam\\steamapps\\common\\FINAL FANTASY XIV ONLINE\\game\\sqpack";
        if (string.IsNullOrEmpty(sqPackPath))
        {
            throw new InvalidOperationException("Unable to find sqpack path for FFXIV");
        }
        GameData = new GameData(sqPackPath);

        CommandManager.AddHandler("/mountinfo", new CommandInfo(OnCommand)
        {
            HelpMessage = "Show mount information of nearby players"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawUI;
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler("/mountinfo");
        PluginInterface.UiBuilder.Draw -= DrawUI;
        GameData?.Dispose();
        // Free cached textures
        foreach (var icon in mountIconCache.Values)
        {
            ImGui.MemFree(icon);
        }
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
            var isFocused = GetTargetHealthBarFocused(playerCharacter);

            // Only show the mount icon when the target bar is visible
            // Only render state changes
            if (!isFocused)
            {
                previousTargetBarVisibleState = false;
                return;
            }
            if (previousTargetBarVisibleState && isFocused)
            {
                return;
            }
            previousTargetBarVisibleState = true;

            var mountID = GetMountID(playerCharacter);
            if (mountID > 0)
            {
                var mountName = GetMountNameById(mountID);
                var mountIconTexture = GetMountIconTexture(mountID);

                if (mountIconTexture == IntPtr.Zero)
                {
                    ImGui.Text("Mount icon not found");
                    return;
                }

                var nameplatePosition = GetTargetHealthBarPosition(playerCharacter);
                if (nameplatePosition.HasValue)
                {
                    try
                    {
                        var iconSize = new Vector2(32, 32); // Adjust icon size as needed
                        var iconPosition = nameplatePosition.Value + new Vector2(0, 20); // Adjust position as needed

                        ImGui.SetNextWindowPos(iconPosition, ImGuiCond.Always);
                        if (ImGui.Begin("MountIcon", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
                        {
                            ImGui.Image(mountIconTexture, iconSize);

                            //// Show tooltip with mount name on hover
                            //if (ImGui.IsItemHovered())
                            //{
                            //    ImGui.SetTooltip(mountName);
                            //}

                            ImGui.End();
                        }
                        else
                        {
                            Console.WriteLine("Failed to create window");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"THE THING BROKE DUDE: {ex}");
                    }
                }
            }
        }
    }

    private IntPtr GetMountIconTexture(uint mountObjectID)
    {
        if (!mountIconCache.TryGetValue(mountObjectID, out var texture))
        {
            var mountRow = GameData.GetExcelSheet<Mount>()?.GetRow(mountObjectID);
            if (mountRow != null)
            {
                var icon = GameData.GetIcon(mountRow.Icon);
                if (icon != null)
                {
                    texture = PluginInterface.UiBuilder.LoadImageRaw(icon.Data, icon.Header.Width, icon.Header.Height, 4).ImGuiHandle;
                    mountIconCache[mountObjectID] = texture;

                    if(mountIconCache.Count > MaxCacheSize)
                    {
                        // Free the oldest texture
                        var oldestKey = mountIconCache.Keys.First();
                        var oldestTexture = mountIconCache[oldestKey];
                        ImGui.MemFree(oldestTexture);
                        mountIconCache.Remove(oldestKey);
                    }
                }
            }
        }
        return texture;
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
