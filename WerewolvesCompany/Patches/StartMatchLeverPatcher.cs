﻿using BepInEx.Logging;
using HarmonyLib;
using WerewolvesCompany.Config;
using WerewolvesCompany.Managers;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;

        static private RoleHUD roleHUD = Plugin.Instance.roleHUD;
        static private RolesManager rolesManager => Plugin.Instance.rolesManager;
        static private QuotaManager quotaManager => Plugin.Instance.quotaManager;
        static private ConfigManager configManager => Plugin.Instance.configManager;

        static public string defaultDisabledHoverTip = "";

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void GetDefaultDisabledHoverTip(StartMatchLever __instance)
        {
            defaultDisabledHoverTip = __instance.triggerScript.disabledHoverTip;

            rolesManager.hasAlreadyDistributedRolesThisRound = false;
        }


        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void HoverLeverCheckForQuotaRequirement(StartMatchLever __instance)
        {


            if (StartOfRound.Instance.shipIsLeaving)
            {
                __instance.triggerScript.disabledHoverTip = defaultDisabledHoverTip;
                return;
            }

            if (!configManager.useQuota.Value)
            {
                __instance.triggerScript.interactable = true;
                return;
            }


            if (!rolesManager.hasAlreadyDistributedRolesThisRound || !StartOfRound.Instance.shipHasLanded) return;

            // If there are no villagers left, ignore the quota check
            if (rolesManager.onlyWerewolvesALive)
            {
                __instance.triggerScript.interactable = true;
                return;
            }


            if (__instance.triggerScript.hoverTip.Contains("Start ship :"))
            {
                if (!quotaManager.isQuotaMet)
                {
                    __instance.triggerScript.interactable = false;
                    __instance.triggerScript.disabledHoverTip = $"[Daily quota not met {quotaManager.currentScrapValue}/{quotaManager.requiredDailyQuota}]";
                }
                else
                {
                    __instance.triggerScript.interactable = true;
                }
            }
        }
    }
}
