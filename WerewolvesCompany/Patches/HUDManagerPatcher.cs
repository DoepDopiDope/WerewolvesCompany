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
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;
        static public ManualLogSource logupdate = Plugin.Instance.logupdate;
        static private RoleHUD roleHUD = Plugin.FindObjectOfType<RoleHUD>();

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
            if (Utils.GetRolesManager().DisableTooltipWhenBodyDroppedInShip.Value)
            {
                if (GObject.name.ToLower().Contains("ragdoll"))
                {
                    logdebug.LogInfo($"Skipped tooltip for item: {GObject.name}");
                    return false;
                }
            }
            return true;
        }

    }
}
