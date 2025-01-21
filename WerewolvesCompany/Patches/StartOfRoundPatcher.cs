
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.Managers;
using System.Collections.Generic;
using GameNetcodeStuff;
using BepInEx.Logging;
using System;
using System.Collections;


namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatcher
    {
        static public ManualLogSource logger = Plugin.instance.logger;
        //static public RolesManager rolesManager = new RolesManager();



        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void spawnNetManager(StartOfRound __instance)
        {
            if (__instance.IsHost)
            {
                GameObject go = GameObject.Instantiate(Plugin.instance.netManagerPrefab);
                go.GetComponent<NetworkObject>().Spawn();
            }
        }



        [HarmonyPostfix]
        [HarmonyPatch("SetShipDoorsClosed")]
        static void SendPlayersTheirRole(StartOfRound __instance)
        {
            // Verify that this is the host, so that it does not send roles multiple times
            if (!(__instance.IsHost || __instance.IsServer))
            {
                return;
            }

            logger.LogInfo(" ==== Roles generation has started ====");
            // Build roles
            Dictionary<ulong, Role> finalRoles;

            RolesManager rolesManager = new RolesManager();
            finalRoles = rolesManager.BuildFinalRolesFromScratch();
            logger.LogInfo("==== Roles generation has finished ==== ");


            logger.LogInfo("==== Starting to send roles to each player ==== ");
            // Send the role to each player
            foreach (var item in finalRoles)
            {
                NetworkManagerWerewolvesCompany.Instance.SendRoleClientRpc(item.Key, item.Value);
            }
            logger.LogInfo("==== Finished sending roles to each player ==== ");


            //if (__instance.IsHost || __instance.IsServer)
            //{
            //    NetworkManagerWerewolvesCompany.Instance.SendRoleServerRpc();
            //}
            //else
            //{
            //    NetworkManagerWerewolvesCompany.Instance.SendRoleClientRpc();
            //}
        }
    }
}