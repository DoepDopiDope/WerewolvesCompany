using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using WerewolvesCompany.Managers;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {
        static public ManualLogSource logger = Plugin.instance.logger;
        static public ManualLogSource logdebug = Plugin.instance.logdebug;

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

        [HarmonyPostfix]
        [HarmonyPatch("Crouch")]
        static void SimpleTipDisplay(PlayerControllerB __instance)
        {
            logdebug.LogInfo("Calling for a Simple Tip display");
            if (__instance.IsHost || __instance.IsServer)
            {
                NetworkManagerWerewolvesCompany.Instance.SimpleTipDisplayServerRpc();
            }
            else
            {
                NetworkManagerWerewolvesCompany.Instance.SimpleTipDisplayClientRpc();
            }
        }

    }
}