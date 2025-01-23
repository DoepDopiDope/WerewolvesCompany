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

        [HarmonyPostfix]
        [HarmonyPatch("Crouch")]
        static void DisplayRoleToolTip()
        {
            RolesManager roleManagerObject = RolesManager.FindObjectOfType<RolesManager>();
            roleManagerObject.DisplayRoleToolTip();

        }

        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void InitiateRole()
        {
            Role role = new Werewolf();
        }



        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void Update(PlayerControllerB __instance)
        {
            if (!__instance.IsOwner) return;

            RolesManager roleManagerObject = RolesManager.FindObjectOfType<RolesManager>();
            ulong? hitPlayer = roleManagerObject.CheckForPlayerInRange(__instance.NetworkObjectId, logupdate);

            roleManagerObject.myRole.targetInRange = hitPlayer;

            RoleHUD roleHUD = Plugin.FindObjectOfType<RoleHUD>();
            roleHUD.UpdateToolTip();

        }
    }
}