using System.Reflection;
using HarmonyLib;
using Steamworks;
    
namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(VideoCamera))]
internal static class VideoCameraPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("HasFilmLeft", MethodType.Getter)]
    static bool HasFilmLeft(VideoCamera __instance, ref bool __result)
    {
        ItemInstanceData m_instanceData = (ItemInstanceData)typeof(VideoCamera).GetField("m_instanceData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
        if (!POVPlugin.HostDeadRecord && POVPlugin.HostOwnerPickup && m_instanceData.TryGetEntry<POVCamera>(out POVCamera povCamera))
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