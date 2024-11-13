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
using System.Reflection;
using ContentSettings.API.Settings;
using ContentSettings.API.Attributes;
using MyceliumNetworking;

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
        POVPlugin.UpdateConfig();
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
        POVPlugin.UpdateConfig();
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
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "Match camera color to player's visor color";

    public override bool GetDefaultValue() => true;
}

[SettingRegister("ContentPOVs")]
public class CameraNameable : BoolSetting, ICustomSetting
{
    public override void ApplyValue()
    {
        POVPlugin.nameable = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "Show user's name while hovering over camera";

    public override bool GetDefaultValue() => true;
}

[SettingRegister("ContentPOVs")]
public class CameraNameDisplay : BoolSetting, ICustomSetting
{
    public override void ApplyValue()
    {
        POVPlugin.nameDisplay = Value;
        POVPlugin.UpdateConfig();
    }

    public string GetDisplayName() => "Display the camera's owner at the bottom right of recordings";

    public override bool GetDefaultValue() => true;
}

[ContentWarningPlugin("com.gingerphoenix10.povs", "ContentPOVs", true)]
[BepInPlugin("com.gingerphoenix10.povs", "ContentPOVs", "1.0.5")]
[BepInDependency("hyydsz-ShopUtils")]
[BepInDependency("RugbugRedfern.MyceliumNetworking")]
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
    internal static bool host_ownerPickup = true;
    internal static bool host_ownerPickupBroken = false;
    internal static bool host_colorable = true;
    internal static bool host_nameable = true;
    internal static bool host_nameDisplay = true;
    internal static void UpdateConfig()
    {
        if (!MyceliumNetwork.IsHost) return;
        MyceliumNetwork.SetLobbyData("ownerPickup", ownerPickup);
        MyceliumNetwork.SetLobbyData("ownerPickupBroken", ownerPickupBroken);
        MyceliumNetwork.SetLobbyData("colorable", colorable);
        MyceliumNetwork.SetLobbyData("nameable", nameable);
        MyceliumNetwork.SetLobbyData("nameDisplay", nameDisplay);
        LoadConfig();
    }
    internal static void LoadConfig()
    {
        host_ownerPickup = MyceliumNetwork.GetLobbyData<bool>("ownerPickup");
        host_ownerPickupBroken = MyceliumNetwork.GetLobbyData<bool>("ownerPickupBroken");
        host_colorable = MyceliumNetwork.GetLobbyData<bool>("colorable");
        host_nameable = MyceliumNetwork.GetLobbyData<bool>("nameable");
        host_nameDisplay = MyceliumNetwork.GetLobbyData<bool>("nameDisplay");
    }
    private void Awake()
    {
        Logger = base.Logger;
        Patcher.PatchAll();
        Entries.RegisterEntry(typeof(POVCamera));
        MyceliumNetwork.RegisterLobbyDataKey("ownerPickup");
        MyceliumNetwork.RegisterLobbyDataKey("ownerPickupBroken");
        MyceliumNetwork.RegisterLobbyDataKey("colorable");
        MyceliumNetwork.RegisterLobbyDataKey("nameable");
        MyceliumNetwork.RegisterLobbyDataKey("nameDisplay");
        MyceliumNetwork.LobbyEntered += delegate
        {
            if (MyceliumNetwork.IsHost)
            {
                UpdateConfig();
            }
            else
            {
                LoadConfig();
            }
        };
        MyceliumNetwork.LobbyDataUpdated += delegate
        {
            if (!MyceliumNetwork.IsHost) LoadConfig();
        };
    }
    private void Update()
    {
        if (MyceliumNetwork.IsHost)
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
                if (povCamera.plrID == "-1")
                {
                    hasPov = true;
                    break;
                }
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
                if (host_colorable)
                {
                    Transform objects = cam.transform.Find("VideoCam");
                    Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                    cubeRenderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cubeRenderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;

                    Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                    cube2Renderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cube2Renderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                }
                else
                {
                    Transform objects = cam.transform.Find("VideoCam");
                    Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                    cubeRenderer.materials[0].color = Color.black;
                    cubeRenderer.materials[1].color = Color.black;

                    Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                    cube2Renderer.materials[0].color = Color.black;
                    cube2Renderer.materials[1].color = Color.black;
                }

                if (cam.transform.parent && cam.transform.parent.GetComponent<Pickup>() != null && host_nameable)
                {
                    cam.transform.parent.GetComponent<Pickup>().hoverText = matched.GetComponent<PhotonView>().Owner.NickName + "'s Camera";
                }
                else if (cam.transform.parent && cam.transform.parent.GetComponent<Pickup>() != null)
                {
                    cam.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Camera";
                }
                Canvas cameraUI = (Canvas)typeof(VideoCamera).GetField("m_cameraUI", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(cam);
                Transform canvas = cameraUI.transform;
                Transform filmGroup = canvas.Find("POVsText");
                TextMeshProUGUI userText;
                if (!filmGroup)
                {
                    filmGroup = new GameObject("POVsText").AddComponent<CanvasGroup>().transform;
                    filmGroup.SetParent(canvas, false);
                    filmGroup.localPosition = new Vector3(250, -400, 0);
                    filmGroup.localScale = Vector3.one * 1.5f;

                    userText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
                    userText.enableWordWrapping = false;
                    userText.alignment = TextAlignmentOptions.BottomRight;
                    userText.transform.SetParent(filmGroup.transform, false);
                }
                userText = filmGroup.Find("Text").GetComponent<TextMeshProUGUI>();
                if (host_nameDisplay) userText.text = matched.GetComponent<PhotonView>().Owner.NickName;
                else userText.text = "";
                hasPov = true;
                break;
            }
            if (!hasPov)
            {
                POVCamera globalCamera = new();
                globalCamera.plrID = "-1";
                cam.GetComponent<ItemInstance>().instanceData.m_dataEntries.Add(globalCamera);
            }
        }
        foreach (ItemInstance item in UnityEngine.Object.FindObjectsOfType<ItemInstance>())
        {
            if (item.item.id != 2) continue;
            string hasPov = "-2";
            HashSet<ItemDataEntry> entries = item.instanceData.m_dataEntries;
            foreach (ItemDataEntry entry in entries)
            {
                if (entry is not POVCamera povCamera) continue;
                if (povCamera.plrID == "-1")
                {
                    hasPov = "-1";
                    break;
                }
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
                hasPov = povCamera.plrID;
                if (!matched)
                {
                    hasPov = "-2";
                    cubeRenderer.materials[0].color = Color.black;
                    cubeRenderer.materials[1].color = Color.black;
                    if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && host_nameable)
                    {
                        item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "?'s Broken Camera";
                    }
                    else
                    {
                        item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
                    }
                    break;
                }

                if (host_colorable)
                {
                    cubeRenderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cubeRenderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;

                    cube2Renderer.materials[0].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                    cube2Renderer.materials[1].color = matched.GetComponent<PlayerVisor>().visorColor.Value;
                }
                else
                {
                    cubeRenderer.materials[0].color = Color.black;
                    cubeRenderer.materials[1].color = Color.black;

                    cube2Renderer.materials[0].color = Color.black;
                    cube2Renderer.materials[1].color = Color.black;
                }

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && host_nameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = matched.GetComponent<PhotonView>().Owner.NickName + "'s Broken Camera";
                }
                else
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
                }
                break;
            }
            if (hasPov == "-2")
            {

                Transform objects = item.gameObject.transform.Find("VideoCam");
                Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                cubeRenderer.materials[0].color = Color.black;
                cubeRenderer.materials[1].color = Color.black;

                cube2Renderer.materials[0].color = Color.black;
                cube2Renderer.materials[1].color = Color.black;

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && host_nameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "?'s Broken Camera";
                }
                else
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
                }
            } else if (hasPov == "-1")
            {
                Transform objects = item.gameObject.transform.Find("VideoCam");
                Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                cubeRenderer.materials[0].color = Color.black;
                cubeRenderer.materials[1].color = Color.black;

                cube2Renderer.materials[0].color = Color.black;
                cube2Renderer.materials[1].color = Color.black;

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && host_nameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
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
            if (MyceliumNetwork.IsHost && PhotonNetwork.InRoom)
            {
                Logger.LogInfo("Called SpawnOnStartRun");
                SpawnCams();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnSlept")]
        internal static void SpawnOnNewDay()
        {
            if (MyceliumNetwork.IsHost)
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
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && host_ownerPickup && povCamera.plrID != "-1" && povCamera.plrID != "-2")
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
                    if (povCamera.plrID != PhotonNetwork.GetPhotonView(photonView).Owner.CustomProperties["SteamID"] as string && host_ownerPickupBroken && povCamera.plrID != "-1" && povCamera.plrID != "-2")
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
                    if (povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString() && host_ownerPickup && povCamera.plrID != "-1" && povCamera.plrID != "-2") return false;
                    break;
                }
            } else if (__instance.itemInstance.item.id == 2) {
                HashSet<ItemDataEntry> entries = __instance.itemInstance.instanceData.m_dataEntries;
                foreach (ItemDataEntry entry in entries)
                {
                    if (entry is not POVCamera povCamera) continue;
                    if (povCamera.plrID != SteamUser.GetSteamID().m_SteamID.ToString() && host_ownerPickupBroken && povCamera.plrID != "-1" && povCamera.plrID != "-2") return false;
                    break;
                }
            }
            return true;
        }
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
}
