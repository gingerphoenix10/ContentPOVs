using HarmonyLib;
using Portningsbolaget.Utilities;
#if BEPIN
using BepInEx;
#endif

namespace ContentPOVs;

[ContentWarningPlugin("ContentPOVs", "2.0.0", vanillaCompatible: false )]
#if BEPIN
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class ContentPOVs : BaseUnityPlugin {
    private static readonly Harmony Patcher = new(MyPluginInfo.PLUGIN_GUID);
    private void Awake()
    {
        Patcher.PatchAll();
    }
}
#else
public class ContentPOVs {}
#endif