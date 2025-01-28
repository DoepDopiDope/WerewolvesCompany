using BepInEx.Logging;
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
        static RolesManager rolesManager => Utils.GetRolesManager();
        static private RoleHUD roleHUD = Plugin.FindObjectOfType<RoleHUD>();

        //[HarmonyPostfix]
        //[HarmonyPatch("Crouch")]
        //static void DisplayMyRolePopUp()
        //{
        //    RolesManager roleManagerObject = RolesManager.FindObjectOfType<RolesManager>();
        //    roleManagerObject.DisplayMyRolePopUp();

        //}

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

            if (!(__instance == Utils.GetLocalPlayerControllerB())) return;

            
            if (!__instance.IsOwner) return;


            if (rolesManager.myRole == null) return;
#nullable enable
            PlayerControllerB? hitPlayer = rolesManager.CheckForPlayerInRange(__instance.NetworkObjectId, logupdate);
#nullable disable
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

            //__instance.cursorTip.text = "Coucou";
            //Utils.GetLocalPlayerControllerB().cursorTip.text = "Coucou";
            roleHUD.UpdateRoleDisplay();
            roleHUD.UpdateToolTip();
        }

        [HarmonyPostfix]
        [HarmonyPatch("KillPlayer")]
        static void OnDeathNotifyServerOfDeath(PlayerControllerB __instance)
        {
            rolesManager.OnSomebodyDeathServerRpc(__instance.OwnerClientId);
        }

    }
}