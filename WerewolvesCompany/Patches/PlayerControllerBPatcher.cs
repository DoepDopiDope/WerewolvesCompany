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

        [HarmonyPostfix]
        [HarmonyPatch("Crouch")]
        static void DisplayMyRolePopUp()
        {
            RolesManager roleManagerObject = RolesManager.FindObjectOfType<RolesManager>();
            roleManagerObject.DisplayMyRolePopUp();

        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void InitiateRole()
        {
            Role role = new Werewolf();
        }

        
        [HarmonyPostfix]
        [HarmonyPatch("LateUpdate")]
        static void LateUpdate(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner) return;

            RolesManager roleManagerObject = RolesManager.FindObjectOfType<RolesManager>();
#nullable enable
            PlayerControllerB? hitPlayer = roleManagerObject.CheckForPlayerInRange(__instance.NetworkObjectId, logupdate);
#nullable disable
            if (hitPlayer == null)
            {
                roleManagerObject.myRole.targetInRangeId = null;
                roleManagerObject.myRole.targetInRangeName = null;
            }
            else
            {
                roleManagerObject.myRole.targetInRangeId = hitPlayer.actualClientId;
                roleManagerObject.myRole.targetInRangeName = hitPlayer.playerUsername;
            }

            //__instance.cursorTip.text = "Coucou";
            //Utils.GetLocalPlayerControllerB().cursorTip.text = "Coucou";
            RoleHUD roleHUD = Plugin.FindObjectOfType<RoleHUD>();
            roleHUD.UpdateToolTip();
        }

        [HarmonyPostfix]
        [HarmonyPatch("KillPlayer")]
        static void OnDeathNotifyServerOfDeath(PlayerControllerB __instance)
        {
            Utils.GetRolesManager().OnSomebodyDeathServerRpc(__instance.actualClientId);
        }

    }
}