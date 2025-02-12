
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.Managers;
using System.Collections.Generic;
using GameNetcodeStuff;
using BepInEx.Logging;
using System;
using System.Collections;
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
                GameObject go = GameObject.Instantiate(Plugin.Instance.netManagerPrefab);
                go.GetComponent<NetworkObject>().Spawn();
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch("StartGame")]
        static void SendPlayersTheirRole(StartOfRound __instance)
        {

            quotaManager.currentScrapValue = 0;

            // Only host can send roles
            if (!(__instance.IsHost || __instance.IsServer))
            {
                return;
            }
            
            logger.LogInfo("Providing roles");
            rolesManager.BuildAndSendRoles();
        }


    }
}