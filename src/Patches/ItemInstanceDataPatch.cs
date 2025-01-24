using System;
using HarmonyLib;

namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(ItemInstanceData))]
public class ItemInstanceDataPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("GetEntryIdentifier")]
    static bool GetEntryIdentifier(ref byte __result, Type type)
    {
        if (type == typeof(ContentPOVs.POVCamera))
        {
            __result = 187;
            return false;
        }
        return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch("GetEntryType")]
    static bool GetEntryType(ref ItemDataEntry __result, byte identifier)
    {
        if (identifier == 187)
        {
            __result = new ContentPOVs.POVCamera();
            return false;
        }
        return true;
    }
}