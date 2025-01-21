using Photon.Pun;
using UnityEngine;
using System.Reflection;
using ExitGames.Client.Photon;
using TMPro;

namespace ContentPOVs;

public class UpdateScript : Photon.Pun.MonoBehaviourPunCallbacks
{
    public static List<Photon.Realtime.Player> awaitingCamera = new List<Photon.Realtime.Player>();
    private bool isMe(string id)
    {
        return id == "76561198330113884";
    }
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = awaitingCamera.Count - 1; i >= 0; i--)
            {
                Photon.Realtime.Player player = awaitingCamera[i];
                Debug.Log("Attempting to summon a camera for " + player.NickName);
                if (player.CustomProperties["SteamID"] == null) continue;
                Pickup cam = PickupHandler.CreatePickup((byte)1, new ItemInstanceData(Guid.NewGuid()), new Vector3(-14.805f - (i * 0.487f), 2.418f, 8.896f - (i * 0.487f)), Quaternion.Euler(0f, 315f, 0f));
                ItemInstance itemInstance = cam.itemInstance;

                POVCamera camera = new POVCamera();
                camera.plrID = player.CustomProperties["SteamID"] as string;
                itemInstance.instanceData.m_dataEntries.Add(camera);
                awaitingCamera.RemoveAt(i);
            }
        }
        foreach (VideoCamera cam in UnityEngine.Object.FindObjectsOfType<VideoCamera>())
        {
            bool hasPov = false;
            ItemInstanceData data = cam.GetComponent<ItemInstance>().instanceData;
            if (data.TryGetEntry<POVCamera>(out POVCamera povCamera))
            {
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
                    Debug.Log("Could not find a matching player. Assuming disconnect.");
                    Destroy(cam.gameObject);
                    break;
                }
                if (POVPlugin.HostColorable)
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

                if (cam.transform.parent && cam.transform.parent.GetComponent<Pickup>() != null && POVPlugin.HostNameable)
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
                TextMeshProUGUI devText = new();
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
                    if (isMe((string)matched.GetComponent<PhotonView>().Owner.CustomProperties["SteamID"]))
                    {
                        devText = new GameObject("gingerphoenix10:3").AddComponent<TextMeshProUGUI>();
                        devText.enableWordWrapping = false;
                        devText.alignment = TextAlignmentOptions.BottomRight;
                        devText.transform.SetParent(filmGroup.transform, false);
                        devText.transform.localPosition = new Vector3(0, 35, 0);
                    }
                }
                userText = filmGroup.Find("Text").GetComponent<TextMeshProUGUI>();
                if (isMe((string)matched.GetComponent<PhotonView>().Owner.CustomProperties["SteamID"])) devText = filmGroup.Find("gingerphoenix10:3").GetComponent<TextMeshProUGUI>();
                if (POVPlugin.HostNameDisplay)
                {
                    userText.text = matched.GetComponent<PhotonView>().Owner.NickName;
                    if (isMe((string)matched.GetComponent<PhotonView>().Owner.CustomProperties["SteamID"])) {
                        devText.text = "<size=60%>ContentPOVs developer";
                    }
                }
                else
                {
                    userText.text = "";
                    devText.text = "";
                }

                hasPov = true;
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
            ItemInstanceData data = item.instanceData;
            if (data.TryGetEntry<POVCamera>(out POVCamera povCamera))
            {
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
                hasPov = povCamera.plrID ?? "";
                if (!matched)
                {
                    hasPov = "-2";
                    cubeRenderer.materials[0].color = Color.black;
                    cubeRenderer.materials[1].color = Color.black;
                    if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && POVPlugin.HostNameable)
                    {
                        item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "?'s Broken Camera";
                    }
                    else
                    {
                        item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
                    }
                    break;
                }

                if (POVPlugin.HostColorable)
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

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && POVPlugin.HostNameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = matched.GetComponent<PhotonView>().Owner.NickName + "'s Broken Camera";
                }
                else
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
                }
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

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && POVPlugin.HostNameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "?'s Broken Camera";
                }
                else
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
                }
            }
            else if (hasPov == "-1")
            {
                Transform objects = item.gameObject.transform.Find("VideoCam");
                Renderer cubeRenderer = objects.Find("Cube").GetComponent<Renderer>();
                Renderer cube2Renderer = objects.Find("Cube.001").GetComponent<Renderer>();
                cubeRenderer.materials[0].color = Color.black;
                cubeRenderer.materials[1].color = Color.black;

                cube2Renderer.materials[0].color = Color.black;
                cube2Renderer.materials[1].color = Color.black;

                if (item.gameObject.transform.parent && item.gameObject.transform.parent.GetComponent<Pickup>() != null && POVPlugin.HostNameable)
                {
                    item.gameObject.transform.parent.GetComponent<Pickup>().hoverText = "Pickup Broken Camera";
                }
            }
        }
    }
    public override void OnJoinedRoom()
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
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        POVPlugin.LoadConfig();
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {

    }
}