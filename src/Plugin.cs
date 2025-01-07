using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

namespace ContentPOVs;

[ContentWarningPlugin("ContentPOVs", "1.3.2", false)]
public class POVPlugin
{
    internal static bool OwnerPickup = true;
    internal static bool OwnerPickupBroken = false;
    internal static bool Colorable = true;
    internal static bool Nameable = true;
    internal static bool NameDisplay = true;
    internal static bool ScoreDivision = true;
    internal static bool DeadCameras;
    internal static bool DeadRecord;
    internal static bool HostOwnerPickup = true;
    internal static bool HostOwnerPickupBroken = false;
    internal static bool HostColorable = true;
    internal static bool HostNameable = true;
    internal static bool HostNameDisplay = true;
    internal static bool HostScoreDivision = true;
    internal static bool HostDeadCameras = false;
    internal static bool HostDeadRecord = false;

    static POVPlugin()
    {
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
        settings.Add("DeadCameras", DeadCameras);
        settings.Add("DeadRecord", DeadRecord);
        PhotonNetwork.CurrentRoom.SetCustomProperties(settings);
    }
    private static bool TryLoadConfig(string optionName, bool fallback)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties[optionName] == null) return fallback;
        else return (bool)PhotonNetwork.CurrentRoom.CustomProperties[optionName];
    }
    internal static void LoadConfig()
    {
        HostOwnerPickup = TryLoadConfig("ownerPickup", OwnerPickup);
        HostOwnerPickupBroken = TryLoadConfig("ownerPickupBroken", OwnerPickupBroken);
        HostColorable = TryLoadConfig("colorable", Colorable);
        HostNameable = TryLoadConfig("nameable", Nameable);
        HostNameDisplay = TryLoadConfig("nameDisplay", NameDisplay);
        HostScoreDivision = TryLoadConfig("scoreDivision", ScoreDivision);
        HostDeadCameras = TryLoadConfig("DeadCameras", DeadCameras);
        HostDeadRecord = TryLoadConfig("DeadRecord", DeadRecord);
    }
    public static void SpawnCams()
    {
        for (int i = 0; i < UnityEngine.Object.FindObjectsOfType<Player>().Length + 0; i++)
        {
            UpdateScript.awaitingCamera.Add(UnityEngine.Object.FindObjectsOfType<Player>()[i].GetComponent<PhotonView>().Owner);
        }
    }
}