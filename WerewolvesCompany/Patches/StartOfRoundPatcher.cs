﻿
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
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;

        //static public RolesManager rolesManager = new RolesManager();



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
        [HarmonyPatch("SetShipDoorsClosed")]
        static void SendPlayersTheirRole(StartOfRound __instance)
        {
            
            // Verify that this is the host, so that it does not send roles multiple times
            if (!(__instance.IsHost || __instance.IsServer))
            {
                return;
            }
    
            logger.LogInfo("Roles generation has started");


            // Build roles
            RolesManager rolesManager =  Plugin.FindObjectOfType<RolesManager>();
            Dictionary<ulong, Role> finalRoles;
            finalRoles = rolesManager.BuildFinalRolesFromScratch();
            logger.LogInfo("Roles generation has finished");

            logdebug.LogInfo($"{finalRoles}");
            logger.LogInfo("Sending roles to each player");
            // Send the role to each player
            foreach (var item in finalRoles)
            {
                logdebug.LogInfo($"Trying to send role {item.Value} to player id {item.Key}");

                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { item.Key }
                    }
                };
                logdebug.LogInfo($"Using ClientRpcParams: {clientRpcParams}");
                

                Utils.PrintDictionary<ulong, Role>(finalRoles);
                logdebug.LogInfo($"{item.Value.refInt}");

                logdebug.LogInfo("Invoking the SendRoleClientRpc method");
                rolesManager.SendRoleClientRpc(item.Value.refInt, clientRpcParams);
            }

            logger.LogInfo("Finished sending roles to each player");

            rolesManager.allRoles = finalRoles;
            logdebug.LogInfo("Stored all roles in RolesManager.");
        }
    }
}