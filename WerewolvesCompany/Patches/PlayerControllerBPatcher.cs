using GameNetcodeStuff;
using HarmonyLib;
using WerewolvesCompany.Managers;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {

        [HarmonyPrefix]
        [HarmonyPatch("KillPlayer")]
        static void NotifyPlayersOfDeath(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner)
            {
                return;
            }
            if (__instance.isPlayerDead)
            {
                return;
            }
            if (!__instance.AllowPlayerDeath())
            {
                return;
            }
            if (__instance.IsHost || __instance.IsServer)
            {
                NetworkManagerWerewolvesCompany.Instance.DeathNotificationClientRpc(__instance.playerClientId);
            }
            else
            {
                NetworkManagerWerewolvesCompany.Instance.RequestDeathNotificationServerRpc(__instance.playerClientId);
            }
        }
    }
}