using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Steamworks;
using Zorro.Core.CLI;
using System;

namespace ContentPOVs;

[ContentWarningPlugin("ContentPOVs", "1.3.6", false)]
public class POVPlugin
{
    internal static bool OwnerPickup = true;
    internal static bool OwnerPickupBroken = false;
    internal static bool Colorable = true;
    internal static bool Nameable = true;
    internal static bool NameDisplay = true;
    internal static bool ScoreDivision = true;
    internal static float ScoreFactor = 100;
    internal static bool DeadCameras;
    internal static bool DeadRecord;
    
    internal static bool HostOwnerPickup = true;
    internal static bool HostOwnerPickupBroken = false;
    internal static bool HostColorable = true;
    internal static bool HostNameable = true;
    internal static bool HostNameDisplay = true;
    internal static bool HostScoreDivision = true;
    internal static float HostScoreFactor = 100;
    internal static bool HostDeadCameras = false;
    internal static bool HostDeadRecord = false;

    static POVPlugin()
    {
        /*if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.gingerphoenix10.povs"))
        {
            Modal.Show("ContentPOVs Duplicate Install", "ContentPOVs has been installed via both BepInEx (probably Thunderstore), AND the Steam Workshop. This will probably break the mod, so try and uninstall one of these versions :)", new ModalOption[]
            {
                new ModalOption("OK", null)
            }, null);
        } THIS BREAKS SHIT IF YOU DON'T HAVE BEPINEX */
        GameObject gameManager = new GameObject();
        gameManager.name = "ContentPOVs";
        gameManager.AddComponent<UpdateScript>();
        UnityEngine.Object.DontDestroyOnLoad(gameManager);
    }

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
    public static void SpawnCams()
    {
        for (int i = 0; i < UnityEngine.Object.FindObjectsOfType<Player>().Length + 0; i++)
        {
            UpdateScript.awaitingCamera.Add(UnityEngine.Object.FindObjectsOfType<Player>()[i].GetComponent<PhotonView>().Owner);
        }
    }
    
    [ConsoleCommand]
    public static void TrySpawn(string SteamID)
    {
        try
        {
            CSteamID id = new(Convert.ToUInt64(SteamID));
            Debug.Log("Attempting to summon a camera for " + SteamFriends.GetFriendPersonaName(id));
        }
        catch
        {
            Debug.Log("Attempting to summon a camera for ?");
        }

        Pickup cam = PickupHandler.CreatePickup((byte)1, new ItemInstanceData(Guid.NewGuid()), MainCamera.instance.GetDebugItemSpawnPos(), Quaternion.Euler(0f, 0f, 0f));
        ItemInstance itemInstance = cam.itemInstance;

        POVCamera camera = new POVCamera();
        camera.plrID = SteamID;
        itemInstance.instanceData.m_dataEntries.Add(camera);
    }
}