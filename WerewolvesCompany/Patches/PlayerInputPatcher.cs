using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.InputSystem;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(PlayerInput))]
    internal class PlayerInputPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;


        [HarmonyPostfix]
        [HarmonyPatch("InitializeActions")]
        static void IntializeCustomKeybinds()
        {
            logger.LogInfo("Setting up keybinds");
            Plugin.Instance.InitiateInputsSystem();
        }
    }
}
