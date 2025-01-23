using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using JetBrains.Annotations;
using WerewolvesCompany.Managers;
using WerewolvesCompany;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;

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

    }
}