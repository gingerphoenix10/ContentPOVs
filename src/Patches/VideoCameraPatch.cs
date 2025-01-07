using HarmonyLib;
using Steamworks;
    
namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(VideoCamera))]
internal static class VideoCameraPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("get_HasFilmLeft")]
    static bool HasFilmLeft(VideoCamera __instance, ref bool __result)
    {
        if (!POVPlugin.HostDeadRecord && POVPlugin.HostOwnerPickup && __instance.m_instanceData.TryGetEntry<POVCamera>(out POVCamera povCamera))
        {
            if (povCamera.plrID != "-1" && povCamera.plrID != "-2" && povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString())
            {
                __result = false;
                return false;
            }
        }
        return true;
    }    
}