using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Steamworks;
using TMPro;
using Zorro.Core.Serizalization;
using System.Text;
using ShopUtils;
using UnityEngine.EventSystems;
using System.ComponentModel;
using System.Reflection;
using ContentSettings.API.Settings;
using ContentSettings.API.Attributes;

namespace ContentPOVs;

public class POVCamera : ItemDataEntry
{
    public string plrID;

    public override void Serialize(BinarySerializer binarySerializer)
    {
        binarySerializer.WriteString(plrID, Encoding.UTF8);
    }

    public override void Deserialize(BinaryDeserializer binaryDeserializer)
    {
        plrID = binaryDeserializer.ReadString(Encoding.UTF8);
    }
}

[SettingRegister("ContentPOVs")]
public class OnlyOwnerPickup : BoolSetting, ICustomSetting
{
    public override void ApplyValue()
    {
        POVPlugin.ownerPickup = Value;
    }

    public string GetDisplayName() => "Only owner can pickup camera";

    public override bool GetDefaultValue() => true;
}

[SettingRegister("ContentPOVs")]
public class OnlyOwnerPickupBroken : BoolSetting, ICustomSetting
{
    public override void ApplyValue()
    {
        POVPlugin.ownerPickupBroken = Value;
    }

    public string GetDisplayName() => "Only owner can pickup broken camera";

    public override bool GetDefaultValue() => false;
}
[SettingRegister("ContentPOVs")]
public class CameraColorable : BoolSetting, ICustomSetting
{
    public override void ApplyValue()
    {
        POVPlugin.colorable = Value;
    }

    public string GetDisplayName() => "Match camera color to player's visor color (doesn't sync)";

    public override bool GetDefaultValue() => true;
}

[SettingRegister("ContentPOVs")]
public class CameraNameable : BoolSetting, ICustomSetting
{
    public override void ApplyValue()
    {
        POVPlugin.nameable = Value;
    }

    public string GetDisplayName() => "Show user's name while hovering over camera (doesn't sync)";

    public override bool GetDefaultValue() => true;
}

[SettingRegister("ContentPOVs")]
public class CameraNameDisplay : BoolSetting, ICustomSetting
{
    public override void ApplyValue()
    {
        POVPlugin.nameDisplay = Value;
    }

    public string GetDisplayName() => "Display the camera's owner at the bottom right of recordings (doesn't sync)";

    public override bool GetDefaultValue() => true;
}

[ContentWarningPlugin("com.gingerphoenix10.povs", "ContentPOVs", true)]
[BepInPlugin("com.gingerphoenix10.povs", "ContentPOVs", "1.0.0")]
[BepInDependency("hyydsz-ShopUtils")]
//[BepInDependency("RedstoneWizard08.ConfigurableWarning")]
public class POVPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static readonly Harmony Patcher = new("com.gingerphoenix10.povs");
    private static List<Photon.Realtime.Player> awaitingCamera = new List<Photon.Realtime.Player>();
    internal static bool ownerPickup = true;
    internal static bool ownerPickupBroken = false;
    internal static bool colorable = true;
    internal static bool nameable = true;
    internal static bool nameDisplay = true;
    private void Awake()
    {
        Logger = base.Logger;
        Patcher.PatchAll();
        Entries.RegisterEntry(typeof(POVCamera));
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = awaitingCamera.Count - 1; i >= 0; i--)
            {
                Photon.Realtime.Player player = awaitingCamera[i];
                Logger.LogInfo("Attempting to summon a camera for " + player.NickName);
                if (player.CustomProperties["SteamID"] == null) continue;
                Pickup cam = PickupHandler.CreatePickup((byte)1, new ItemInstanceData(Guid.NewGuid()), new Vector3(-14.805f - (i * 0.487f), 2.418f, 8.896f - (i * 0.487f)), Quaternion.Euler(0f, 315f, 0f));
                ItemInstance itemInstance = cam.itemInstance;
                POVCamera camera = new POVCamera();
                camera.plrID = player.CustomProperties["SteamID"] as string;
                itemInstance.instanceData.m_dataEntries.Add(camera);
                awaitingCamera.RemoveAt(i);
            }
        }
        foreach (VideoCamera cam in UnityEngine.Object.FindObjectsOfType<VideoCamera>()) {
            bool hasPov = false;
            HashSet<ItemDataEntry> entries = cam.GetComponent<ItemInstance>().instanceData.m_dataEntries;
            foreach (ItemDataEntry entry in entries)
            {
                if (entry is not POVCamera povCamera) continue;
                Player matched = new();
                foreach (PlayerVisor vis in UnityEngine.Object.FindObjectsOfType<PlayerVisor>())
                {
                    Player plr = vis.gameObject.GetComponent<Player>();
                    if (plr.GetComponent<PhotonView>().Owner.CustomProperties["SteamID"] as string == povCamera.plrID)
                    {
                        matched = plr;
                        break;
                    }
                }

                if (!matched)
                {
                    Logger.LogInfo("Could not find a matching player. Assuming disconnect.");
                    Destroy(cam.gameObject);
                    break;
                }
                if (colorable)
                {
                    Transform objects = cam.transform.Find("VideoCam");
                    Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                    cubeRenderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cubeRenderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;

                    Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                    cube2Renderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cube2Renderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                }

                if (cam.transform.parent && cam.transform.parent.GetComponent<Pickup>() != null && nameable)
                {
                    cam.transform.parent.GetComponent<Pickup>().hoverText = matched.GetComponent<PhotonView>().Owner.NickName + "'s Camera";
                }
                Canvas cameraUI = (Canvas)typeof(VideoCamera).GetField("m_cameraUI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cam);
                Transform canvas = cameraUI.transform;
                var filmGroup = canvas.Find("POVsText");
                var userText = filmGroup.Find("Text").GetComponent<TextMeshProUGUI>();
                if (nameDisplay) userText.text = matched.GetComponent<PhotonView>().Owner.NickName;
                else userText.text = "";
                hasPov = true;
                break;
            }
            if (!hasPov)
            {
                Logger.LogInfo("A camera without the ContentPOVs info was found. Assuming it's just the default camera, so deleting");
                Destroy(cam.gameObject);
                //Was easier just to create a ContentPOVs camera on start rather than to convert the original camera to host's
            }
        }
        foreach (ItemInstance item in UnityEngine.Object.FindObjectsOfType<ItemInstance>())
        {
            if (item.item.id != 2) continue;
            bool hasPov = false;
            HashSet<ItemDataEntry> entries = item.instanceData.m_dataEntries;
            foreach (ItemDataEntry entry in entries)
            {
                if (entry is not POVCamera povCamera) continue;
                Player matched = new();
                foreach (PlayerVisor vis in UnityEngine.Object.FindObjectsOfType<PlayerVisor>())
                {
                    Player plr = vis.gameObject.GetComponent<Player>();
                    if (plr.GetComponent<PhotonView>().Owner.CustomProperties["SteamID"] as string == povCamera.plrID)
                    {
                        matched = plr;
                        break;
                    }
                }

                Transform objects = item.gameObject.transform.Find("VideoCam");
                Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                hasPov = true;
                if (!matched)
                {
                    if (colorable)
                    {
                        cubeRenderer.materials[0].color = Color.black;
                        cubeRenderer.materials[1].color = Color.black;

                        cube2Renderer.materials[0].color = Color.black;
                        cube2Renderer.materials[1].color = Color.black;
                    }
                    if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && nameable)
                    {
                        item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "?'s Broken Camera";
                    }
                    break;
                }

                if (colorable)
                {
                    cubeRenderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cubeRenderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;

                    cube2Renderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cube2Renderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                }

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && nameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = matched.GetComponent<PhotonView>().Owner.NickName + "'s Broken Camera";
                }
                break;
            }
            if (!hasPov)
            {

                if (colorable)
                {
                    Transform objects = item.gameObject.transform.Find("VideoCam");
                    Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                    Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                    cubeRenderer.materials[0].color = Color.black;
                    cubeRenderer.materials[1].color = Color.black;

                    cube2Renderer.materials[0].color = Color.black;
                    cube2Renderer.materials[1].color = Color.black;
                }

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && nameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "?'s Broken Camera";
                }
            }
        }
    }
    private static void SpawnCams()
    {
        for (int i = 0; i < UnityEngine.Object.FindObjectsOfType<Player>().Length + 0; i++)
        {
            awaitingCamera.Add(UnityEngine.Object.FindObjectsOfType<Player>()[i].GetComponent<PhotonView>().Owner);
        }
    }
    internal static Harmony Harmony { get; private set; }

    [HarmonyPatch(typeof(SurfaceNetworkHandler))]
    internal static class SurfaceNetworkHandlerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("RPCM_StartGame")]
        internal static void SpawnOnStartRun()
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.InRoom)
            {
                Logger.LogInfo("Called SpawnOnStartRun");
                SpawnCams();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnSlept")]
        internal static void SpawnOnNewDay()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Logger.LogInfo("Called SpawnOnNewDay");
                SpawnCams();
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
    }

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
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && ownerPickup)
                    {
                        __instance.m_photonView.RPC("RPC_FailedToPickup", PhotonNetwork.GetPhotonView(photonView).GetComponent<Player>().refs.view.Owner);
                        return false;
                    }
                    break;
                }
            } else if (__instance.itemInstance.item.id == 2)
            {
                HashSet<ItemDataEntry> entries = __instance.itemInstance.instanceData.m_dataEntries;
                foreach (ItemDataEntry entry in entries)
                {
                    if (entry is not POVCamera povCamera) continue;
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && ownerPickupBroken)
                    {
                        __instance.m_photonView.RPC("RPC_FailedToPickup", PhotonNetwork.GetPhotonView(photonView).GetComponent<Player>().refs.view.Owner);
                        return false;
                    }
                    break;
                }
            }
            return true;
        }
        /*[HarmonyPrefix]
        [HarmonyPatch("Interact")]
        internal static bool Interact(Player player, Pickup __instance)
        {
            if (__instance.itemInstance.item.id == 1)
            {
                HashSet<ItemDataEntry> entries = __instance.itemInstance.instanceData.m_dataEntries;
                foreach (ItemDataEntry entry in entries)
                {
                    if (entry is not POVCamera povCamera) continue;
                    if (povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString()) return false;
                    break;
                }
            }
            return true;
        }
        
        Temporarily disabled. Need to check if the host has pickups enabled or not

         */
    }

    [HarmonyPatch(typeof(PhotonGameLobbyHandler))]
    internal static class ConnectPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("OnPlayerEnteredRoom")]
        internal static void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (SurfaceNetworkHandler.HasStarted) awaitingCamera.Add(newPlayer);
        }
    }
    [HarmonyPatch(typeof(VideoCamera))]
    internal static class CameraPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        internal static void CameraStart(VideoCamera __instance)
        {
            Canvas cameraUI = (Canvas)typeof(VideoCamera).GetField("m_cameraUI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            Transform canvas = cameraUI.transform;
            var filmGroup = new GameObject("POVsText").AddComponent<CanvasGroup>();
            filmGroup.transform.SetParent(canvas, false);
            filmGroup.transform.localPosition = new Vector3(250, -400, 0);
            filmGroup.transform.localScale = Vector3.one * 1.5f;

            var userText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            userText.enableWordWrapping = false;
            userText.alignment = TextAlignmentOptions.BottomRight;
            userText.transform.SetParent(filmGroup.transform, false);
        }
    }
}

