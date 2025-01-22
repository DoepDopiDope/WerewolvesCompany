using BepInEx.Logging;
using UnityEngine;

namespace WerewolvesCompany.UI
{
    public class HUDInitializer : MonoBehaviour
    {
        public ManualLogSource logger = Plugin.instance.logger;
        public ManualLogSource logdebug = Plugin.instance.logdebug;

        void Start()
        {
            // Add the RoleHUD to the scene
            logger.LogInfo("Starting the HUDInitializer");
            GameObject roleHUDObject = new GameObject("RoleHUD");
            RoleHUD roleHUD = roleHUDObject.AddComponent<RoleHUD>();
            logger.LogInfo($"HUDInitializer has create the roleHUDObject");

            logdebug.LogInfo("roleHUDObject has the following properties:");
            logdebug.LogInfo($"-- roleHUDObject.activeSelf = {roleHUDObject.activeSelf}");
        }
    }
}
