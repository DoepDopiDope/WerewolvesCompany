﻿using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using WerewolvesCompany.Managers;
using WerewolvesCompany;
using WerewolvesCompany.UI;
using System.Diagnostics;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;
        static public ManualLogSource logupdate = Plugin.Instance.logupdate;
        
        //static RolesManager rolesManager = Utils.GetRolesManager();
        static private RoleHUD roleHUD = Plugin.FindObjectOfType<RoleHUD>();



        [HarmonyPostfix]
        [HarmonyPatch("LateUpdate")]
        static void LateUpdate(PlayerControllerB __instance)
        {
            if (!(__instance == Utils.GetLocalPlayerControllerB())) return;
            if (!__instance.IsOwner) return;

            //logdebug.LogInfo("Get the rolesManager");
            RolesManager rolesManager = Plugin.Instance.rolesManager;

            //logdebug.LogInfo("Check if my role is null");
            if (rolesManager.myRole == null) return;

            //logdebug.LogInfo("Check for player in range");
#nullable enable
            PlayerControllerB? hitPlayer = rolesManager.CheckForPlayerInRange(__instance.NetworkObjectId);
#nullable disable

            //logdebug.LogInfo("Check if hitplayer is null");
            if (hitPlayer == null)
            {
                //logdebug.LogInfo("set stuff to null");
                rolesManager.myRole.targetInRangeId = null;
                rolesManager.myRole.targetInRangeName = null;
            }
            else
            {
                //logdebug.LogInfo("set stuff to stuff");
                rolesManager.myRole.targetInRangeId = hitPlayer.OwnerClientId;
                rolesManager.myRole.targetInRangeName = hitPlayer.playerUsername;
            }

            //logdebug.LogInfo("Update HUD");
            roleHUD.UpdateRoleDisplay();
            roleHUD.UpdateToolTip();
        }

        [HarmonyPostfix]
        [HarmonyPatch("KillPlayer")]
        static void OnDeathNotifyServerOfDeath(PlayerControllerB __instance)
        {
            Plugin.Instance.rolesManager.OnSomebodyDeathServerRpc(__instance.OwnerClientId);
            Plugin.Instance.rolesManager.QueryAllRolesServerRpc();
        }

    }
}