using HarmonyLib;

namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(PhotonGameLobbyHandler))]
internal static class PhotonGameLobbyHandlerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("OnPlayerEnteredRoom")]
    internal static void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (SurfaceNetworkHandler.HasStarted) UpdateScript.awaitingCamera.Add(newPlayer);
    }
}