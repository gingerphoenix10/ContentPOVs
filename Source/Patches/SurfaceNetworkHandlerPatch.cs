using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using System.Reflection;

namespace ContentPOVs.Patches;

    [HarmonyPatch(typeof(SurfaceNetworkHandler))]
    internal static class SurfaceNetworkHandlerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RPCM_StartGame")]
        internal static void SpawnOnStartRun()
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
            {
                Debug.Log("Called SpawnOnStartRun");
                POVPlugin.SpawnCams();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnSlept")]
        internal static void SpawnOnNewDay()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("Called SpawnOnNewDay");
                POVPlugin.SpawnCams();
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch("PreCheckHeadToUnderWorld")]
        internal static bool ToUnderworld(SurfaceNetworkHandler __instance, ref bool __result)
        {
            bool m_Started = (bool)typeof(SurfaceNetworkHandler).GetField("m_Started", BindingFlags.NonPublic | BindingFlags.Static).GetValue(__instance);
            PhotonView m_View = (PhotonView)typeof(SurfaceNetworkHandler).GetField("m_View", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            MethodInfo CheckIfCameraIsPresent = typeof(SurfaceNetworkHandler).GetMethod("CheckIfCameraIsPresent", BindingFlags.NonPublic | BindingFlags.Instance);
            bool m_HeadingToUnderWorld = (bool)typeof(SurfaceNetworkHandler).GetField("m_HeadingToUnderWorld", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (!m_Started)
            {
                Debug.LogError("Cant head to underworld before started game");
                __result = false;
                return false;
            }
            if (!(bool)CheckIfCameraIsPresent.Invoke(__instance, new object[] { false }))
            {
                m_View.RPC("RPCA_HelmetText", RpcTarget.All, 64, -1);
                __result = false;
                return false;
            }

            if (m_HeadingToUnderWorld)
            {
                __result = false;
                return false;
            }

            typeof(SurfaceNetworkHandler).GetField("m_HeadingToUnderWorld", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, true);
            __result = true;
            return false;
        }
        [HarmonyPrefix]
        [HarmonyPatch("OnJoinedRoom")]
        internal static void OnJoinedRoom()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                POVPlugin.UpdateConfig();
            }
            else
            {
                POVPlugin.LoadConfig();
            }
        }
        [HarmonyPrefix]
        [HarmonyPatch("OnRoomPropertiesUpdate")]
        internal static void OnRoomPropertiesUpdate()
        {
            POVPlugin.LoadConfig();
        }
    }