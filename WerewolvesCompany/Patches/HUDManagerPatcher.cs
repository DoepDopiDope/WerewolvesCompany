using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using WerewolvesCompany.Managers;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Patches
{
    //[HarmonyPatch(typeof(HUDManager))]
    [HarmonyPatch]
    internal class HUDManagerPatcher
    {
        static public ManualLogSource logger => Plugin.Instance.logger;
        static public ManualLogSource logdebug => Plugin.Instance.logdebug;
        static private RoleHUD roleHUD => Plugin.Instance.roleHUD;
        static private QuotaManager quotaManager => Plugin.Instance.quotaManager;
        static private RolesManager rolesManager => Plugin.Instance.rolesManager;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(HUDManager), "SetSpectatingTextToPlayer")]
        static void DisplaySpectatedPlayerRole(PlayerControllerB playerScript, HUDManager __instance)
        {
            logdebug.LogInfo("Called SetSpectatingTextToPlayer");
            RolesManager rolesManager = Utils.GetRolesManager();
            rolesManager.QueryAllRolesServerRpc();
            string displayText = $"(Spectating: {playerScript.playerUsername} - <b>{rolesManager.allRoles[playerScript.OwnerClientId].roleNameColored}</b>)";
            logdebug.LogInfo($"Displaying Spectating text: {displayText}");
            __instance.spectatingPlayerText.text = displayText;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "AddNewScrapFoundToDisplay")]
        static bool PreventTooltipOnDropBodyInShip(GrabbableObject GObject)
        {
            // Check if object is a ragdoll, and therefore do not display the tooltip
            if (Utils.GetRolesManager().DisableTooltipWhenBodyDroppedInShip.Value && GObject.name.ToLower().Contains("ragdoll"))
            {
                logdebug.LogInfo($"Skipped tooltip for item: {GObject.name}");
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "AddNewScrapFoundToDisplay")]
        static void AddScrapValueToQuota(GrabbableObject GObject)
        {
            //quotaManager.AddScrapValue(GObject.scrapValue);
            logdebug.LogInfo($"Found scrap: {GObject.name} of value {GObject.scrapValue}");
            //rolesManager.AddQuotaValueServerRpc(GObject.scrapValue);
            quotaManager.AddScrapValue(GObject.scrapValue);
        }
    }
}
