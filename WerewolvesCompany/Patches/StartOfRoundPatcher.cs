
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.Managers;
using BepInEx.Logging;
using WerewolvesCompany.UI;


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
                GameObject rolesManagerObject = GameObject.Instantiate(Plugin.Instance.rolesManagerPrefab);
                rolesManagerObject.GetComponent<NetworkObject>().Spawn();

                GameObject configManagerObject = GameObject.Instantiate(Plugin.Instance.configManagerPrefab);
                configManagerObject.GetComponent<NetworkObject>().Spawn();

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("ReviveDeadPlayers")]
        static void ResetRolesToNullOnRoundEnd(StartOfRound __instance)
        {
            logdebug.LogInfo("Resetting my role to null");
            rolesManager.myRole = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch("ResetPlayersLoadedValueClientRpc")]
        static void ResetRolesToNullOnRoundStart(StartOfRound __instance)
        {
            logdebug.LogInfo("Resetting my role to null");
            rolesManager.myRole = null;
            roleHUD.roleTextContainer.SetActive(false);
        }


    }
}