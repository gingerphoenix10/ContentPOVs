using HarmonyLib;
using Photon.Pun;
using Steamworks;
using UnityEngine;

namespace ContentPOVs.Patches;

    [HarmonyPatch(typeof(Pickup))]
    internal static class PickupPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("RPC_RequestPickup")]
        internal static bool RequestPickup(int photonView, Pickup __instance)
        {
            if (__instance.itemInstance.item.id == 1)
            {
                ItemInstanceData data = __instance.itemInstance.instanceData;
                if (data.TryGetEntry<POVCamera>(out POVCamera povCamera))
                {
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && POVPlugin.HostOwnerPickup && povCamera.plrID != "-1" && povCamera.plrID != "-2")
                    {
                        if (POVPlugin.HostDeadCameras)
                        {
                            foreach (Player playerInstance in UnityEngine.Object.FindObjectsOfType<Player>())
                            {
                                if (playerInstance.ai) continue;
                                if (playerInstance.photonView.Owner.CustomProperties["SteamID"] as string == povCamera.plrID && playerInstance.data.dead)
                                {
                                    return true;
                                }
                            }
                        }
                        __instance.m_photonView.RPC("RPC_FailedToPickup", PhotonNetwork.GetPhotonView(photonView).GetComponent<Player>().refs.view.Owner);
                        return false;
                    }
                }
            }
            else if (__instance.itemInstance.item.id == 2)
            {
                ItemInstanceData data = __instance.itemInstance.instanceData;
                if (data.TryGetEntry<POVCamera>(out POVCamera povCamera))
                {
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && POVPlugin.HostOwnerPickupBroken && povCamera.plrID != "-1" && povCamera.plrID != "-2")
                    {
                        if (POVPlugin.HostDeadCameras)
                        {
                            foreach (Player playerInstance in UnityEngine.Object.FindObjectsOfType<Player>())
                            {
                                if (playerInstance.ai) continue;
                                if (playerInstance.photonView.Owner.CustomProperties["SteamID"] as string == povCamera.plrID && playerInstance.data.dead)
                                {
                                    return true;
                                }
                            }
                        }
                        __instance.m_photonView.RPC("RPC_FailedToPickup", PhotonNetwork.GetPhotonView(photonView).GetComponent<Player>().refs.view.Owner);
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(PickupSpawner))]
        internal static class SpawnerPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch("SpawnMe")]
            internal static bool Spawn(bool force, PickupSpawner __instance)
            {
                if (__instance.ItemToSpawn.id == 1) return false;
                return true;
            }
        }

        /*[HarmonyPrefix]
        [HarmonyPatch("Interact")]
        internal static bool Interact(Player player, Pickup __instance)
        {
            if (__instance.itemInstance.item.id == 1)
            {
                ItemInstanceData data = __instance.itemInstance.instanceData;
                if (data.TryGetEntry<POVCamera>(out POVCamera povCamera))
                {
                    if (povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString() && POVPlugin.HostOwnerPickup && povCamera.plrID != "-1" && povCamera.plrID != "-2") return false;
                }
            }
            else if (__instance.itemInstance.item.id == 2)
            {
                ItemInstanceData data = __instance.itemInstance.instanceData;
                if (data.TryGetEntry<POVCamera>(out POVCamera povCamera))
                {
                    if (povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString() && POVPlugin.HostOwnerPickupBroken && povCamera.plrID != "-1" && povCamera.plrID != "-2") return false;
                    break;
                }
            }
            return true;
        }*/
    }