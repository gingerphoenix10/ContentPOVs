using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace ContentPOVs;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, false)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class POVPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static readonly Harmony Patcher = new(MyPluginInfo.PLUGIN_GUID);
    internal static bool OwnerPickup = true;
    internal static bool OwnerPickupBroken = false;
    internal static bool Colorable = true;
    internal static bool Nameable = true;
    internal static bool NameDisplay = true;
    internal static bool ScoreDivision = true;
    internal static float ScoreFactor = 100;
    internal static bool DeadCameras = false;
    internal static bool DeadRecord = false;
    
    internal static bool HostOwnerPickup = true;
    internal static bool HostOwnerPickupBroken = false;
    internal static bool HostColorable = true;
    internal static bool HostNameable = true;
    internal static bool HostNameDisplay = true;
    internal static bool HostScoreDivision = true;
    internal static float HostScoreFactor = 100;
    internal static bool HostDeadCameras = false;
    internal static bool HostDeadRecord = false;
    internal static void UpdateConfig()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Hashtable settings = new();
        settings.Add("ownerPickup", OwnerPickup);
        settings.Add("ownerPickupBroken", OwnerPickupBroken);
        settings.Add("colorable", Colorable);
        settings.Add("nameable", Nameable);
        settings.Add("nameDisplay", NameDisplay);
        settings.Add("scoreDivision", ScoreDivision);
        settings.Add("scoreFactor", ScoreFactor);
        settings.Add("deadCameras", DeadCameras);
        settings.Add("deadRecord", DeadRecord);
        PhotonNetwork.CurrentRoom.SetCustomProperties(settings);
    }
    private static object TryLoadConfig(string optionName, object fallback)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[optionName] == null) return fallback;
        else return PhotonNetwork.CurrentRoom.CustomProperties[optionName];
    }
    internal static void LoadConfig()
    {
        HostOwnerPickup = (bool)TryLoadConfig("ownerPickup", OwnerPickup);
        HostOwnerPickupBroken = (bool)TryLoadConfig("ownerPickupBroken", OwnerPickupBroken);
        HostColorable = (bool)TryLoadConfig("colorable", Colorable);
        HostNameable = (bool)TryLoadConfig("nameable", Nameable);
        HostNameDisplay = (bool)TryLoadConfig("nameDisplay", NameDisplay);
        HostScoreDivision = (bool)TryLoadConfig("scoreDivision", ScoreDivision);
        HostScoreFactor = (float)TryLoadConfig("scoreFactor", ScoreFactor);
        HostDeadCameras = (bool)TryLoadConfig("deadCameras", DeadCameras);
        HostDeadRecord = (bool)TryLoadConfig("deadRecord", DeadRecord);
    }
    private void Awake()
    {
        Logger = base.Logger;
        Patcher.PatchAll();
    }
    
    public static void SpawnCams()
    {
        for (int i = 0; i < UnityEngine.Object.FindObjectsOfType<Player>().Length + 0; i++)
        {
            UpdateScript.awaitingCamera.Add(UnityEngine.Object.FindObjectsOfType<Player>()[i].GetComponent<PhotonView>().Owner);
            Logger.LogInfo("Spawning camera for player");
        }
    }
    internal static Harmony Harmony { get; private set; }
}