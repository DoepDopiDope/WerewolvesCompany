using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using WerewolvesCompany.Managers;
using WerewolvesCompany;
using WerewolvesCompany.UI;
using System.Diagnostics;
using System.Drawing;
using UnityEngine.Rendering;
using TMPro;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;
        static public ManualLogSource logupdate = Plugin.Instance.logupdate;
        
        static private RoleHUD roleHUD = Plugin.Instance.roleHUD;
        static private RolesManager rolesManager => Plugin.Instance.rolesManager;
        static private QuotaManager quotaManager => Plugin.Instance.quotaManager;

        [HarmonyPostfix]
        [HarmonyPatch("LateUpdate")]
        static void LateUpdate(PlayerControllerB __instance)
        {
            if (!(__instance == Utils.GetLocalPlayerControllerB())) return;
            if (!__instance.IsOwner) return;
            if (rolesManager.myRole == null) return;

            // Check for player in range
#nullable enable
            PlayerControllerB? hitPlayer = rolesManager.CheckForPlayerInRange(__instance.NetworkObjectId);
#nullable disable

            // Check hit player
            if (hitPlayer == null)
            {
                rolesManager.myRole.targetInRangeId = null;
                rolesManager.myRole.targetInRangeName = null;
            }
            else
            {
                rolesManager.myRole.targetInRangeId = hitPlayer.OwnerClientId;
                rolesManager.myRole.targetInRangeName = hitPlayer.playerUsername;
            }

            // Update the HUD, it needs to run after all of the above.
            roleHUD.UpdateHUD();

        }


        [HarmonyPostfix]
        [HarmonyPatch("KillPlayer")]
        static void OnDeathLogic(PlayerControllerB __instance)
        {
            // SKip if not killing the local controller
            if (!(__instance == Utils.GetLocalPlayerControllerB())) return;
            if (!__instance.IsOwner) return;

            // Reset role to its initial state
            Plugin.Instance.rolesManager.myRole = References.GetRoleByName(Plugin.Instance.rolesManager.myRole.roleName);
            logger.LogInfo("Role has been reset due to death");

            // Notify server of death
            Plugin.Instance.rolesManager.OnSomebodyDeathServerRpc(__instance.OwnerClientId);
            Plugin.Instance.rolesManager.QueryAllRolesServerRpc();

            // Reset my vote to no vote
            roleHUD.voteCastedPlayer = null;
            roleHUD.voteWindowContainer.SetActive(false);

            // Set HUD off
            roleHUD.roleTextContainer.SetActive(false);
        }

        [HarmonyPrefix]
        [HarmonyPatch("ShowNameBillboard")]
        static void ShowRoleSpecificColor(PlayerControllerB __instance)
        {
            // Default color
            __instance.usernameBillboardText.color = UnityEngine.Color.white;

            //"<color=pink>lover</color>"
            string loverLine = " <color=#ff00ffff><3</color>";
            __instance.usernameBillboardText.text = __instance.usernameBillboardText.text.Replace(loverLine, "");
            

            // Check if the player is a Werewolf, and should be displayed in red in case I'm also a Werewolf
            if (rolesManager.CanWerewolvesSeeEachOther.Value)
            {
                if (!(rolesManager.myRole == null))
                {
                    if (rolesManager.allRoles.ContainsKey(__instance.OwnerClientId)) // Check that the targetted player does have a role
                    {
                        if ((rolesManager.myRole.roleName == "Werewolf") && (rolesManager.allRoles[__instance.OwnerClientId].roleName == "Werewolf"))
                        {
                            __instance.usernameBillboardText.color = UnityEngine.Color.red;
                        }
                    }
                }
            }

            // Check if the player is a Minion, in which case he can see the werewolves
            if (!(rolesManager.myRole == null))
            {
                if (rolesManager.allRoles.ContainsKey(__instance.OwnerClientId)) // Check that the targetted player does have a role
                {
                    if ((rolesManager.myRole.roleName == "Minion") && (rolesManager.allRoles[__instance.OwnerClientId].roleName == "Werewolf"))
                    {
                        __instance.usernameBillboardText.color = UnityEngine.Color.red;
                    }
                }
            }

            // Check if the player is the one I am in love with
            if (!(rolesManager.myRole == null))
            {
                if (rolesManager.myRole.interactions.isInLoveWith != null)
                {
                    if (__instance.OwnerClientId == rolesManager.myRole.interactions.isInLoveWith.Value)
                    {
                        __instance.usernameBillboardText.text += loverLine;
                    }
                }
                
            }
        }

        

    }
}