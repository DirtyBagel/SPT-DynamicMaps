using System;
using System.Reflection;
using SPT.Reflection.Patching;
using DynamicMaps.Config;
using DynamicMaps.Utils;
using EFT.UI.Map;
using HarmonyLib;

namespace DynamicMaps.Patches
{
    internal class MapScreenShowPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MapScreen), nameof(MapScreen.Show));
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            try
            {
                if (!Settings.ReplaceMapScreen.Value || !GameUtils.ShouldShowMapInRaid())
                {
                    // mod is disabled
                    DynamicMapsPlugin.Instance.Map?.ViewModeController?.EndMapScreen();
                    return true;
                }

                // show instead
                DynamicMapsPlugin.Instance.Map?.ViewModeController?.ShowMapScreen();
                return false;
            }
            catch(Exception e)
            {
                DynamicMapsPlugin.Log.LogError($"Caught error while trying to show map");
                DynamicMapsPlugin.Log.LogError($"{e.Message}");
                DynamicMapsPlugin.Log.LogError($"{e.StackTrace}");

                return true;
            }
        }
    }

    internal class MapScreenClosePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(MapScreen), nameof(MapScreen.Close));
        }

        [PatchPrefix]
        public static bool PatchPrefix()
        {
            try
            {
                if (!Settings.ReplaceMapScreen.Value || !GameUtils.ShouldShowMapInRaid())
                {
                    // mod is disabled
                    return true;
                }

                // close instead
                DynamicMapsPlugin.Instance.Map?.ViewModeController?.EndMapScreen();
                return false;
            }
            catch(Exception e)
            {
                DynamicMapsPlugin.Log.LogError($"Caught error while trying to close map");
                DynamicMapsPlugin.Log.LogError($"{e.Message}");
                DynamicMapsPlugin.Log.LogError($"{e.StackTrace}");

                return true;
            }
        }
    }
}
