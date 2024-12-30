using HarmonyLib;

namespace ContentPOVs.Patches;

[HarmonyPatch(typeof(ExtractVideoMachine))]
internal static class ExtractorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("RPC_Failed")]
    internal static void RPC_Failed(ExtractVideoMachine __instance)
    {
        __instance.StateMachine.SwitchState<ExtractMachineIdleState>();
    }
}