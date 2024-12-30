using System.Collections.Generic;
using HarmonyLib;
using Photon.Pun;
using Steamworks;

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
                HashSet<ItemDataEntry> entries = __instance.itemInstance.instanceData.m_dataEntries;
                foreach (ItemDataEntry entry in entries)
                {
                    if (entry is not POVCamera povCamera) continue;
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && POVPlugin.HostOwnerPickup && povCamera.plrID != "-1" && povCamera.plrID != "-2")
                    {
                        __instance.m_photonView.RPC("RPC_FailedToPickup", PhotonNetwork.GetPhotonView(photonView).GetComponent<Player>().refs.view.Owner);
                        return false;
                    }
                    break;
                }
            }
            else if (__instance.itemInstance.item.id == 2)
            {
                HashSet<ItemDataEntry> entries = __instance.itemInstance.instanceData.m_dataEntries;
                foreach (ItemDataEntry entry in entries)
                {
                    if (entry is not POVCamera povCamera) continue;
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && POVPlugin.HostOwnerPickupBroken && povCamera.plrID != "-1" && povCamera.plrID != "-2")
                    {
                        __instance.m_photonView.RPC("RPC_FailedToPickup", PhotonNetwork.GetPhotonView(photonView).GetComponent<Player>().refs.view.Owner);
                        return false;
                    }
                    break;
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

        [HarmonyPrefix]
        [HarmonyPatch("Interact")]
        internal static bool Interact(Player player, Pickup __instance)
        {
            if (__instance.itemInstance.item.id == 1)
            {
                HashSet<ItemDataEntry> entries = __instance.itemInstance.instanceData.m_dataEntries;
                foreach (ItemDataEntry entry in entries)
                {
                    if (entry is not POVCamera povCamera) continue;
                    if (povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString() && POVPlugin.HostOwnerPickup && povCamera.plrID != "-1" && povCamera.plrID != "-2") return false;
                    break;
                }
            }
            else if (__instance.itemInstance.item.id == 2)
            {
                HashSet<ItemDataEntry> entries = __instance.itemInstance.instanceData.m_dataEntries;
                foreach (ItemDataEntry entry in entries)
                {
                    if (entry is not POVCamera povCamera) continue;
                    if (povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString() && POVPlugin.HostOwnerPickupBroken && povCamera.plrID != "-1" && povCamera.plrID != "-2") return false;
                    break;
                }
            }
            return true;
        }
    }