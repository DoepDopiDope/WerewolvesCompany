using HarmonyLib;
using Unity.Netcode;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void AddToPrefabs(ref GameNetworkManager __instance)
        {
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(Plugin.Instance.rolesManagerPrefab);
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(Plugin.Instance.configManagerPrefab);
        }


        [HarmonyPostfix]
        [HarmonyPatch("Disconnect")]
        static void DisableHUD()
        {
            Plugin.Instance.roleHUD.roleTextContainer.SetActive(false);
            Plugin.Instance.roleHUD.voteWindowContainer.SetActive(false);
        }
    }
}