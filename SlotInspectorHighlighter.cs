using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SlotInspectorHighlighter
{
    public class SlotInspectorHighlighter : ResoniteMod
    {
        internal const string DOMAIN_NAME = "com.Sinduy.SlotInspectorHighlighter";
        internal const string VERSION = "1.0.5";
        public override string Name => "SlotInspectorHighlighter";
        public override string Author => "Sinduy";
        public override string Version => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        public override string Link => "https://github.com/sjsanjsrh/SlotInspectorHighlighter";

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enabled =
            new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> localOnly =
            new ModConfigurationKey<bool>("localOnly", "Highlight only inspector element allocated by the local user", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> hightlightColor =
            new ModConfigurationKey<colorX>("highlightColor", "Color of the highlighter", () => new colorX(0, 0.85f, 0.95f, 0.1f));

        private static ModConfiguration Config;

        public static bool Enabled => Config.GetValue(enabled);
        public static bool LocalOnly => Config.GetValue(localOnly);
        public static colorX HighlightColor => Config.GetValue(hightlightColor);
        public static readonly String HIGHLITER_SLOT_NAME = "Highlighter";


        public override void OnEngineInit()
        {
            Config = GetConfiguration();

            new Harmony(DOMAIN_NAME).PatchAll();
        }
    }

    [HarmonyPatch(typeof(SlotInspector))]
    class SlotInspectorHighlighterPatch
    {
        [HarmonyPatch("UpdateText")]
        [HarmonyPostfix]
        static void UpdateTextPostfix(SlotInspector __instance, Slot ____setupRoot, RelayRef<SyncRef<Slot>> ____selectionReference, SyncRef<Text> ____slotNameText)
        {
            bool isLocalUserFlag = true;
            if (SlotInspectorHighlighter.LocalOnly)
            {
                SceneInspector sceneInspector = __instance.Slot.GetComponentInParents<SceneInspector>();
                User allocatedUser = GetAllocatingUser(sceneInspector);
                isLocalUserFlag = allocatedUser == __instance.LocalUser;
            }

            // Get or create highlighter slot
            Slot selectedSlot = ____selectionReference.Target?.Target;
            Slot textSlot = ____slotNameText.Target.Slot;
            Slot hlSlot = textSlot.FindChild(SlotInspectorHighlighter.HIGHLITER_SLOT_NAME);
            if (!(SlotInspectorHighlighter.Enabled && isLocalUserFlag))
            {
                if (hlSlot != null)
                { hlSlot.Destroy(); }
                return;
            }
            
            if (hlSlot == null)
            {
                UIBuilder builder = new UIBuilder(textSlot);
                var hlImg = builder.Image(null, SlotInspectorHighlighter.HighlightColor);
                hlImg.Slot.Name = SlotInspectorHighlighter.HIGHLITER_SLOT_NAME;
                hlSlot = hlImg.Slot;
            }
            hlSlot.ActiveSelf = selectedSlot == ____setupRoot;
        }

        private static User GetAllocatingUser(IWorldElement worldElement)
        {
            if (worldElement == null)
                return null;

            World world = worldElement.World;
            if (world == null)
                return null;

            ulong position;
            byte userId;
            worldElement.ReferenceID.ExtractIDs(out position, out userId);

            User userByAllocationId = world.GetUserByAllocationID(userId);
            if (userByAllocationId == null)
                return null;

            return position < userByAllocationId.AllocationIDStart ? null : userByAllocationId;
        }
    }
}
