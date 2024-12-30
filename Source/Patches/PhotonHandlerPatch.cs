using HarmonyLib;
using UnityEngine;
using Photon.Pun;

namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(PhotonHandler))]
public class PhotonHandlerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    static void Awake()
    {
        GameObject updateScript = new GameObject();
        updateScript.name = "ContentPOVs";
        updateScript.AddComponent<UpdateScript>();
        UnityEngine.Object.DontDestroyOnLoad(updateScript);
    }
}