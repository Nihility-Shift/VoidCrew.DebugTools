using BepInEx;
using CG.Game.Player;
using CG.Input;
using CG.Network;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ResourceAssets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static DebugTools.Common;
using static UnityEngine.GUILayout;
using static VoidManager.Utilities.GUITools;

namespace DebugTools
{
    public class DTGUI : MonoBehaviour, IShowCursorSource, IInputActionMapRequest
    {
        public static DTGUI Instance { get; private set; }

        public void Start()
        {
            Instance = this;

            // Create canvas for GUI display.
            DTCanvas = new GameObject("DTCanvas", new Type[] { typeof(Canvas) });
            Canvas canvasComponent = DTCanvas.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComponent.sortingOrder = 1000;
            canvasComponent.transform.SetAsLastSibling();
            DontDestroyOnLoad(DTCanvas);

            // Background image to block mouse clicks passing IMGUI
            Background = new GameObject("DTGUIBG", new Type[] { typeof(GraphicRaycaster) });
            Image = Background.AddComponent<Image>();
            Image.color = Color.clear;
            Background.transform.SetParent(DTCanvas.transform);
            Background.SetActive(false);

            // Load Default settings
            ShowDebugDev = Configs.SpawnFiltersDebug.Value;
            ShowVanilla = Configs.SpawnFiltersVanilla.Value;
            ShowCarryables = Configs.SpawnFiltersCarryables.Value;
            ShowWeaponModules = Configs.SpawnFiltersWeaponBoxes.Value;
            ShowRuntime = Configs.SpawnFiltersModded.Value;
        }

        /// <summary>
        /// Update open on keybind.
        /// </summary>
        public void Update()
        {
            if (VoidManagerPlugin.Enabled && UnityInput.Current.GetKeyDown(Configs.OpenMenu.Value))
            {
                GUIActive = !GUIActive;
            }
        }

        internal Rect WindowRect = new Rect(0, 0, 450, 800);
        Vector2 SpawnlistScroll = new();

        GameObject Background;
        GameObject DTCanvas;
        Image Image;


        private bool _GUIActive = false;
        public bool GUIActive
        {
            get => _GUIActive;
            set
            {
                _GUIActive = value;
                if (value)
                    GUIOpen();
                else
                    GUIClose();
            }
        }

        //Resizing values
        bool isResizing = false;
        Rect windowResizeStart = new Rect();
        Vector2 minWindowSize = new Vector2(75, 50);
        static GUIContent gcDrag = new GUIContent("/", "drag to resize");

        public void OnGUI()
        {
            if (!VoidManagerPlugin.Enabled || !GUIActive) return;
            GUI.skin = VoidManager.CustomGUI.GUIMain.ChangeSkin();

            WindowRect = Window(98122, WindowRect, WindowFunc, "Spawn Menu");

            Image.rectTransform.position = new Vector3(WindowRect.center.x, (WindowRect.center.y * -1) + Screen.height, 0);
            Image.rectTransform.sizeDelta = WindowRect.size;
        }

        bool firstOpen = true;

        public void GUIOpen()
        {
            GUIToggleCursor(true);
            Background.SetActive(true);

            // Cache late after objects have loaded.
            if (firstOpen)
            {
                firstOpen = false;
                CacheSpawnables();
            }
        }

        public void GUIClose()
        {
            GUIToggleCursor(false);
            Background.SetActive(false);
        }

        string DistanceOffsetString = "1";
        float DistanceOffset = 1f;
        bool ShowCarryables = true;
        bool ShowRuntime = false;
        bool ShowVanilla = true;
        bool ShowDebugDev = false;
        bool ShowWeaponModules = false;
        GUIContent refreshContent = new GUIContent("%", "Refresh");

        string nameFilter = string.Empty;

        private void WindowFunc(int windowID)
        {
            // Display selection
            BeginHorizontal();
            if (DrawButtonSelected(DTSettings.CarryablesLabel, ShowCarryables)) ShowCarryables = !ShowCarryables;
            if (DrawButtonSelected(DTSettings.WeaponBoxesLabel, ShowWeaponModules)) ShowWeaponModules = !ShowWeaponModules;
            if (DrawButtonSelected(DTSettings.ModdedContentLabel, ShowRuntime)) ShowRuntime = !ShowRuntime;
            if (DrawButtonSelected(DTSettings.VanillaLabel, ShowVanilla)) ShowVanilla = !ShowVanilla;
            if (DrawButtonSelected(DTSettings.LockedDevDebugLabel, ShowDebugDev)) ShowDebugDev = !ShowDebugDev;
            if (Button(refreshContent, ButtonMinSizeStyle)) CacheSpawnables();
            EndHorizontal();
            if (DrawTextField("DistanceOffset", ref DistanceOffsetString))
            {
                float.TryParse(DistanceOffsetString, out DistanceOffset);
            }

            // Filter by text
            if (DrawTextField("Filter", ref nameFilter))
            {
                // Cache filter value once on filter change.
                foreach(SpawnEntry spawnData in Spawnables)
                {
                    // Filters header/name, description, and filename.
                    spawnData.StringFiltered = !spawnData.Context.HeaderText.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase)
                        && !spawnData.Context.BodyText.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase)
                        && !spawnData.FileName.Contains(nameFilter, StringComparison.CurrentCultureIgnoreCase);
                }
            }

            // Display spawnables
            BeginVertical();
            SpawnlistScroll = BeginScrollView(SpawnlistScroll);
            foreach (var spawnItem in Spawnables)
            {
                SpawnItemButton(spawnItem);
            }
            EndScrollView();
            EndVertical();


            
            BeginHorizontal();

            // Show Tips
            Label(GUI.tooltip);

            // Resize doesn't work q.q
            //WindowRect = ResizeWindow(WindowRect, ref isResizing, ref windowResizeStart, minWindowSize);

            EndHorizontal();
            GUI.DragWindow();
        }

        public static List<SpawnEntry> Spawnables = new();

        /// <summary>
        /// Displays spawnable button if matches filters.
        /// </summary>
        /// <param name="spawnData"></param>
        void SpawnItemButton(SpawnEntry spawnData)
        {
            // Filter logic
            if (!ShowDebugDev && spawnData.LockedDevItem) return;
            if (!ShowVanilla && !spawnData.Runtime) return;
            if (!ShowRuntime && spawnData.Runtime) return;
            if (!ShowCarryables && spawnData.SpawnType == SpawnType.Carryable) return;
            if (!ShowWeaponModules && spawnData.SpawnType == SpawnType.CompositeWeaponBox) return;
            if (spawnData.StringFiltered) return;

            if (Button(spawnData.SEContent))
            {
                if (LocalPlayer.Instance == null || !VoidManagerPlugin.Enabled) return;
                Transform camera = Camera.main.gameObject.transform;
                Vector3 Spawnpos = camera.position + camera.forward * DistanceOffset; // Vector3.up keeps it from being feet level.

                if (spawnData.Runtime)
                {
                    ObjectFactory.InstantiateRuntimeObject(spawnData.GUID, Spawnpos, default);
                }
                else if (spawnData.SpawnType == SpawnType.Carryable)
                {
                    ObjectFactory.InstantiateSpaceObjectByGUID(spawnData.GUID, Spawnpos, default);
                }
                else
                {
                    CompositeWeaponBuildBox CWBB = DefaultAssetTable.Instance.BuildBoxCompositeWeapon;
                    ObjectFactory.InstantiateSpaceObjectByGUID(CWBB.assetGuid, Spawnpos, default, new Dictionary<byte, object> { { 0, JObject.FromObject(spawnData.Ref).ToString(Formatting.None, Array.Empty<JsonConverter>()) } });
                }
            }
        }

        public static void CacheSpawnables()
        {
            Spawnables.Clear();

            foreach (var AssetGUIDPair in CarryableContainer.Instance.RuntimeDescriptions)
            {
                Spawnables.Add(new SpawnEntry(
                    AssetGUIDPair.ContextInfo,
                    AssetGUIDPair.Ref,
                    SpawnType.Carryable)
                );
            }

            foreach (var AssetGUIDPair in CompositeWeaponDataContainer.Instance.RuntimeDescriptions)
            {
                Spawnables.Add(new SpawnEntry(
                    AssetGUIDPair.ContextInfo,
                    AssetGUIDPair.Ref,
                    SpawnType.CompositeWeaponBox)
                );
            }
        }

        bool ShowingCursor = false;

        void GUIToggleCursor(bool enable)
        {
            if (!Configs.MenuUnlockCursor.Value && !(!enable && ShowingCursor))
            {
                return; // Stop early if unlocking cursor is disabled, but allow passthrough if cursor is enabled and is getting set to disabled.
            }

            ShowingCursor = enable;
            CursorUtility.ShowCursor(this, enable);

            if (ShowingCursor)
            {
                InputActionMapRequests.AddOrChangeRequest(this, InputStateRequestType.UI);
            }
            else
            {
                InputActionMapRequests.RemoveRequest(this);
            }
        }

        public class SpawnEntry
        {
            public SpawnEntry() { }
            public SpawnEntry(IResourceAssetContextInfo contextInfo, ResourceAssetRef assetRef, SpawnType spawnType)
            {
                Context = contextInfo;
                Ref = assetRef;
                FileName = assetRef.Filename;
                GUID = assetRef.AssetGuid;
                LockedDevItem = IsItemLocked(GUID);
                Runtime = assetRef.IsRuntime;
                SpawnType = spawnType;

                SEContent = new GUIContent($"{Context.HeaderText}\n{FileName}", Context.BodyText);
            }

            public GUIContent SEContent;
            public IResourceAssetContextInfo Context;
            public ResourceAssetRef Ref;
            public string FileName;
            public GUIDUnion GUID;
            public bool LockedDevItem;
            public bool Runtime;
            public SpawnType SpawnType;

            // Cached string filtered to save frames.
            public bool StringFiltered;
        }

        public enum SpawnType : byte
        {
            Carryable,
            CompositeWeaponBox,
        }

        /// <summary>
        /// Draws a window resizer in the lower right corner of an IMGUI window. Credit to Molix
        /// https://discussions.unity.com/t/gui-window-resize-window/3841/3
        /// </summary>
        /// <param name="windowRect"></param>
        /// <param name="isResizing"></param>
        /// <param name="resizeStart"></param>
        /// <param name="minWindowSize"></param>
        /// <returns></returns>
        public static Rect ResizeWindow(Rect windowRect, ref bool isResizing, ref Rect resizeStart, Vector2 minWindowSize)
        {
            // Default style does not exist.
            /*if (styleWindowResize == null)
            {
                // this is a custom style that looks like a // in the lower corner
                styleWindowResize = UnityEngine.GUI.skin.GetStyle("WindowResizer");
            }*/

            Vector2 mousePos = Mouse.current.position.value;
            Rect r = GUILayoutUtility.GetRect(gcDrag, ButtonMinSizeStyle);

            if (Mouse.current.leftButton.wasPressedThisFrame && r.Contains(mousePos))
            {
                isResizing = true;
                resizeStart = new Rect(mousePos.x, mousePos.y, windowRect.width, windowRect.height);
                //Event.current.Use();  // the GUI.Button below will eat the event, and this way it will show its active state
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && isResizing)
            {
                isResizing = false;
            }
            else if (!Mouse.current.leftButton.isPressed)
            {
                // if the mouse is over some other window we won't get an event, this just kind of circumvents that by checking the button state directly
                isResizing = false;
            }
            else if (isResizing)
            {
                windowRect.width = Mathf.Max(minWindowSize.x, resizeStart.width + (mousePos.x - resizeStart.x));
                windowRect.height = Mathf.Max(minWindowSize.y, resizeStart.height + (mousePos.y - resizeStart.y));
                windowRect.xMax = Mathf.Min(Screen.width, windowRect.xMax);  // modifying xMax affects width, not x
                windowRect.yMax = Mathf.Min(Screen.height, windowRect.yMax);  // modifying yMax affects height, not y
            }

            Button(gcDrag, ButtonMinSizeStyle);

            return windowRect;
        }
    }
}
