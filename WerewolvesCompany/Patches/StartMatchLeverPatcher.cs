using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using TMPro;
using WerewolvesCompany.Managers;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;
        static public ManualLogSource logupdate = Plugin.Instance.logupdate;

        static private RoleHUD roleHUD = Plugin.Instance.roleHUD;
        static private RolesManager rolesManager => Plugin.Instance.rolesManager;
        static private QuotaManager quotaManager => Plugin.Instance.quotaManager;

        static public string defaultDisabledHoverTip = "";

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void GetDefaultDisabledHoverTip(StartMatchLever __instance)
        {
            defaultDisabledHoverTip = __instance.triggerScript.disabledHoverTip;
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

            if (!StartOfRound.Instance.shipHasLanded) return;

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
