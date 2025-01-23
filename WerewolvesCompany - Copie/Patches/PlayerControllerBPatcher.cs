using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using WerewolvesCompany.Managers;
using WerewolvesCompany;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;

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


        //[HarmonyPostfix]
        //[HarmonyPatch("Crouch")]
        //static void SwapShipDoors(PlayerControllerB __instance)
        //{
        //    if (__instance.IsHost || __instance.IsServer)
        //    {
        //        //NetworkManagerWerewolvesCompany.Instance.SwapShipDoorsClientRpc();
        //        StartOfRound.Instance.SetDoorsClosedClientRpc(StartOfRound.Instance.hangarDoorsClosed);
        //    }
        //    else
        //    {
        //        //NetworkManagerWerewolvesCompany.Instance.SwapShipDoorsServerRpc();
        //        StartOfRound.Instance.SetDoorsClosedServerRpc(StartOfRound.Instance.hangarDoorsClosed);
        //    }

        //}

        [HarmonyPostfix]
        [HarmonyPatch("Crouch")]
        static void DisplayRoleToolTip()
        {
            //RolesManager rolesManager = new RolesManager();
            //rolesManager.DisplayRoleToolTip();
            RolesManager roleManagerObject = RolesManager.FindObjectOfType<RolesManager>();
            roleManagerObject.DisplayRoleToolTip();

        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void InitiateRole()
        {
            Role role = new Werewolf();
        }

    }
}