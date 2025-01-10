using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Pun;
using ExitGames.Client.Photon;
using System.Collections.Generic;

namespace ContentPOVs;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, true)]
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
    internal static bool DeadCameras = false;
    internal static bool DeadRecord = false;
    
    internal static bool HostOwnerPickup = true;
    internal static bool HostOwnerPickupBroken = false;
    internal static bool HostColorable = true;
    internal static bool HostNameable = true;
    internal static bool HostNameDisplay = true;
    internal static bool HostScoreDivision = true;
    internal static bool HostDeadCameras = false;
    internal static bool HostDeadRecord = false;
    internal static void UpdateConfig()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        Hashtable settings = new();
        settings.Add("OwnerPickup", OwnerPickup);
        settings.Add("OwnerPickupBroken", OwnerPickupBroken);
        settings.Add("Colorable", Colorable);
        settings.Add("Nameable", Nameable);
        settings.Add("NameDisplay", NameDisplay);
        settings.Add("ScoreDivision", ScoreDivision);
        settings.Add("DeadCameras", DeadCameras);
        settings.Add("DeadRecord", DeadRecord);
        PhotonNetwork.CurrentRoom.SetCustomProperties(settings);
    }
    internal static bool TryLoadConfig(string optionName, bool fallback)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[optionName] == null) return fallback;
        else return (bool)PhotonNetwork.CurrentRoom.CustomProperties[optionName];
    }
    internal static void LoadConfig()
    {
        HostOwnerPickup = TryLoadConfig("OwnerPickup", OwnerPickup);
        HostOwnerPickupBroken = TryLoadConfig("OwnerPickupBroken", OwnerPickupBroken);
        HostColorable = TryLoadConfig("Colorable", Colorable);
        HostNameable = TryLoadConfig("Nameable", Nameable);
        HostNameDisplay = TryLoadConfig("NameDisplay", NameDisplay);
        HostScoreDivision = TryLoadConfig("ScoreDivision", ScoreDivision);
        HostDeadCameras = TryLoadConfig("DeadCameras", DeadCameras);
        HostDeadRecord = TryLoadConfig("DeadRecord", DeadRecord);
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