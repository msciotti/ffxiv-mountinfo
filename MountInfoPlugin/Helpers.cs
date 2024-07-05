using Dalamud.Game.ClientState.Objects.SubKinds;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using System.Numerics;

namespace MountInfo
{
    public static class Helpers
    {
        public static unsafe uint GetMountID(IPlayerCharacter playerCharacter)
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

            var mountRow = Service.DataManager.GetExcelSheet<Mount>()?.GetRow(mountObjectID);
            if (mountRow != null)
            {
                return mountRow.Singular;
            }
            
            return "Unknown Mount";
        }

        public static uint GetMountIconID(uint mountID)
        {
            var mountRow = Service.DataManager.GetExcelSheet<Mount>()?.GetRow(mountID);
            if (mountRow == null) return 0;
            return mountRow.Icon;
        }

        public static unsafe Vector2? GetTargetHealthBarPosition(IPlayerCharacter playerCharacter)
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

        public static unsafe bool GetTargetHealthBarFocused(IPlayerCharacter playerCharacter)
        {
            var targetInfoHud = (AtkUnitBase*)Service.GameGui.GetAddonByName("_TargetInfo");
            if (targetInfoHud == null) return false;

            var healthBarNode = targetInfoHud->RootNode->ChildNode;
            if (healthBarNode == null) return false;

            return healthBarNode->IsVisible();
        }
    }
}
