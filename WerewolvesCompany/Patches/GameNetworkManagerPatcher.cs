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
            NetworkManager networkManager = __instance.GetComponent<NetworkManager>();
            if (networkManager == null) return;
            networkManager.AddNetworkPrefab(Plugin.Instance.rolesManagerPrefab);
            networkManager.AddNetworkPrefab(Plugin.Instance.configManagerPrefab);
        }


        [HarmonyPostfix]
        [HarmonyPatch("Disconnect")]
        static void DisableHUD()
        {
            if (Plugin.Instance.roleHUD == null) return;
            Plugin.Instance.roleHUD.roleTextContainer.SetActive(false);
            Plugin.Instance.roleHUD.voteWindowContainer.SetActive(false);
        }
    }
}
