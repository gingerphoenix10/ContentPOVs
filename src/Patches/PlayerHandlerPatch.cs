using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(PlayerHandler))]
public class PlayerHandlerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(PlayerHandler.AddPlayer))]
    public static void AddPlayer(Player player)
    {
        if (SurfaceNetworkHandler.HasStarted && TimeOfDayHandler.TimeOfDay == TimeOfDay.Morning) UpdateScript.awaitingCamera.Add(player.refs.view.Owner);
    }
}