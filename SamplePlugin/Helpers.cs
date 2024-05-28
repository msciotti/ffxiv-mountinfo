using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;

namespace SamplePlugin
{
    public static class Helpers
    {
        private static string SqPackPath = "D:\\Games\\Steam\\steamapps\\common\\FINAL FANTASY XIV ONLINE\\game\\sqpack";
        public static GameData GameData = new(SqPackPath);

        public static unsafe uint GetMountID(PlayerCharacter playerCharacter)
        {
            var characterPtr = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)playerCharacter.Address;
            if (characterPtr == null) return 0;
            var mountContainer = characterPtr->Mount;

            var mountObjectID = mountContainer.MountId;
            if (mountObjectID == 0) return 0;

            return mountObjectID;
        }

        public static unsafe string GetMountNameById(uint mountObjectID)
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

        public static uint GetMountIconID(uint mountID)
        {
            var mountRow = GameData.GetExcelSheet<Mount>().GetRow(mountID);
            if (mountRow == null) return 0;
            return mountRow.Icon;
        }

        public static unsafe Vector2? GetTargetHealthBarPosition(PlayerCharacter playerCharacter)
        {
            var targetInfoHud = (AtkUnitBase*)Service.GameGui.GetAddonByName("_TargetInfo");
            if (targetInfoHud == null) return null;

            var healthBarNode = (AtkResNode*)targetInfoHud->RootNode->ChildNode;
            if (healthBarNode == null) return null;

            // Get screen coordinates from the node
            var x = healthBarNode->ScreenX;
            var y = healthBarNode->ScreenY;
            return new Vector2(x, y);
        }

        public static unsafe bool GetTargetHealthBarFocused(PlayerCharacter playerCharacter)
        {
            var targetInfoHud = (AtkUnitBase*)Service.GameGui.GetAddonByName("_TargetInfo");
            if (targetInfoHud == null) return false;

            var healthBarNode = (AtkResNode*)targetInfoHud->RootNode->ChildNode;
            if (healthBarNode == null) return false;

            return healthBarNode->IsVisible;
        }
    }
}
