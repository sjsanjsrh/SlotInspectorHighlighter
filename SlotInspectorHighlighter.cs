using FrooxEngine;
using FrooxEngine.UIX;
using Elements.Core;
using ResoniteModLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SlotInspectorHighlighter
{
    public class SlotInspectorHighlighter : ResoniteMod
    {
        public override string Name => "SlotInspectorHighlighter";
        public override string Author => "Sinduy";
        public override string Version => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
        public override string Link => "https://github.com/sjsanjsrh/SlotInspectorHighlighter";
        public static readonly string DOMAIN_NAME = "com.Sinduy.SlotInspectorHighlighter";

        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<bool> enabled =
            new ModConfigurationKey<bool>("enabled", "Should the mod be enabled", () => true);
        [AutoRegisterConfigKey]
        private static readonly ModConfigurationKey<colorX> hightlightColor =
            new ModConfigurationKey<colorX>("highlightColor", "Color of the highlighter", () => new colorX(1, 1, 1, 0.01f));

        private static ModConfiguration Config;

        public static bool Enabled => Config.GetValue(enabled);
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
            if (!SlotInspectorHighlighter.Enabled) { return; }
            Slot textSlot = ____slotNameText.Target.Slot;
            Slot hlSlot = textSlot.FindChild(SlotInspectorHighlighter.HIGHLITER_SLOT_NAME);
            if (hlSlot == null)
            {
                UIBuilder builder = new UIBuilder(textSlot);
                var hlImg = builder.Image(null, SlotInspectorHighlighter.HighlightColor);
                hlImg.Slot.Name = SlotInspectorHighlighter.HIGHLITER_SLOT_NAME;
                hlSlot = hlImg.Slot;
            }
            hlSlot.ActiveSelf = ____selectionReference.Target?.Target == ____setupRoot;
        }
    }
}
