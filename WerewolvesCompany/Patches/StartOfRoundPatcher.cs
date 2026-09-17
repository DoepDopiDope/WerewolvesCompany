
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.Managers;
using BepInEx.Logging;
using WerewolvesCompany.UI;
using GameNetcodeStuff;


namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;

        //static public RolesManager rolesManager = new RolesManager();
        static private RolesManager rolesManager => Plugin.Instance.rolesManager;
        static private RoleHUD roleHUD => Plugin.Instance.roleHUD;
        static private QuotaManager quotaManager => Plugin.Instance.quotaManager;


        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void spawnNetManager(StartOfRound __instance)
        {
            if (__instance.IsHost)
            {
                // Roles read synchronized settings while they initialize, so the
                // configuration object must be spawned first on every peer.
                if (Config.ConfigManager.Instance == null)
                {
                    GameObject configManagerObject = GameObject.Instantiate(Plugin.Instance.configManagerPrefab);
                    configManagerObject.GetComponent<NetworkObject>().Spawn();
                }

                if (RolesManager.Instance == null)
                {
                    GameObject rolesManagerObject = GameObject.Instantiate(Plugin.Instance.rolesManagerPrefab);
                    rolesManagerObject.GetComponent<NetworkObject>().Spawn();
                }
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("ReviveDeadPlayers")]
        //static void ResetRolesToNullOnRoundEnd(StartOfRound __instance)
        //{
        //    logdebug.LogInfo("Resetting my role to null");
        //    rolesManager.myRole = null;
        //    roleHUD.roleTextContainer.SetActive(false);
        //}

        [HarmonyPrefix]
        [HarmonyPatch("ResetPlayersLoadedValueClientRpc")]
        static void OnRoundStart()
        {
            if (rolesManager == null || roleHUD == null) return;
            logdebug.LogInfo("Resetting my role to null");
            rolesManager.myRole = null;
            rolesManager.hasAlreadyDistributedRolesThisRound = false;
            roleHUD.roleTextContainer.SetActive(false);
        }
        

        [HarmonyPrefix]
        [HarmonyPatch("EndOfGameClientRpc")]
        static void EndGameLogic()
        {
            if (rolesManager == null || roleHUD == null) return;
            // Display winning team
            rolesManager.DisplayWinningTeam();

            // Reset roleHUD
            logdebug.LogInfo("Resetting my role to null");
            rolesManager.myRole = null;
            roleHUD.roleTextContainer.SetActive(false);
        }


    }
}
