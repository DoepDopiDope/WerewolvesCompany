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
        static private RolesManager rolesManager => Plugin.Instance.rolesManager;


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
            roleHUD.UpdateVoteWindowText();
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
                if (rolesManager.myRole.isInLoveWith != null)
                {
                    if (__instance.OwnerClientId == rolesManager.myRole.isInLoveWith.Value)
                    {
                        __instance.usernameBillboardText.text += loverLine;
                    }
                }
                
            }
        }
    }
}