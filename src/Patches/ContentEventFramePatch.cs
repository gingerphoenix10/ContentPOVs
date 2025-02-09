using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core;

namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(ContentEventFrame))]
internal static class ContentEventFramePatch
{
    [HarmonyPrefix]
    [HarmonyPatch("GetScore")]
    internal static bool GetScore(ContentEventFrame __instance, ref float __result)
    {
        if (!POVPlugin.HostScoreDivision) return true;
        __result = SingletonAsset<BigNumbers>.Instance.percentageToScreenToFactorCurve.Evaluate(__instance.seenAmount) * __instance.contentEvent.GetContentValue() / (1 + POVPlugin.ScoreFactor / 100 * (PhotonNetwork.CurrentRoom.PlayerCount - 1));
        return false;
    }
}