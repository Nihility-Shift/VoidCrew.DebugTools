using BepInEx.Configuration;
using UnityEngine;

namespace DebugTools
{
    internal static class Configs
    {
        internal static ConfigEntry<KeyCode> OpenMenu;
        internal static ConfigEntry<bool> MenuUnlockCursor;
        internal static ConfigEntry<bool> SpawnFiltersDebug;
        internal static ConfigEntry<bool> SpawnFiltersVanilla;
        internal static ConfigEntry<bool> SpawnFiltersModded;
        internal static ConfigEntry<bool> SpawnFiltersWeaponBoxes;
        internal static ConfigEntry<bool> SpawnFiltersCarryables;

        internal static void Load(BepinPlugin plugin)
        {
            MenuUnlockCursor = plugin.Config.Bind("Menu", "MenuUnlockCursor", true);
            OpenMenu = plugin.Config.Bind("SpawnMenu", "SpawnMenuKeybind", KeyCode.F10);


            SpawnFiltersDebug = plugin.Config.Bind("SpawnMenu", "ShowLockedDevItems", false, "Displays locked/dev items by default");
            SpawnFiltersVanilla = plugin.Config.Bind("SpawnMenu", "ShowVanilla", true);
            SpawnFiltersModded = plugin.Config.Bind("SpawnMenu", "ShowModded", true);
            SpawnFiltersWeaponBoxes = plugin.Config.Bind("SpawnMenu", "ShowWeaponBuildBoxes", false);
            SpawnFiltersCarryables = plugin.Config.Bind("SpawnMenu", "ShowCarryables", true);
        }
    }
}
