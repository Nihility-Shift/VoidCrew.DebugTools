using UnityEngine;
using VoidManager.CustomGUI;
using static UnityEngine.GUILayout;
using static VoidManager.Utilities.GUITools;

namespace DebugTools
{
    public class DTSettings : ModSettingsMenu
    {
        public const string LockedDevDebugLabel = "Locked/Dev Items";
        public const string VanillaLabel = "Vanilla";
        public const string ModdedContentLabel = "Modded";
        public const string WeaponBoxesLabel = "WeaponBuildboxes";
        public const string CarryablesLabel = "Carryables";

        public override void Draw()
        {
            BeginHorizontal();
            if (Button("Toggle Spawn Menu")) DTGUI.Instance.GUIActive = !DTGUI.Instance.GUIActive;
            if (Button("Reset Position")) DTGUI.Instance.WindowRect.position = new Vector2(0, 0);
            DrawChangeKeybindButton("Keybind", ref Configs.OpenMenu);
            DrawCheckbox("Menu Unlocks Cursor", ref Configs.MenuUnlockCursor);
            EndHorizontal();

            Label("Default Filters");
            DrawCheckbox(LockedDevDebugLabel, ref Configs.SpawnFiltersDebug);
            DrawCheckbox(VanillaLabel, ref Configs.SpawnFiltersDebug);
            DrawCheckbox(ModdedContentLabel, ref Configs.SpawnFiltersModded);
            DrawCheckbox(WeaponBoxesLabel, ref Configs.SpawnFiltersWeaponBoxes);
            DrawCheckbox(CarryablesLabel, ref Configs.SpawnFiltersCarryables);
        }

        public override string Name()
        {
            return MyPluginInfo.USERS_PLUGIN_NAME;
        }
    }
}
