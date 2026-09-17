using BepInEx.Logging;
using HarmonyLib;
using WerewolvesCompany.Managers;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatcher
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;

        //static public RolesManager rolesManager = new RolesManager();
        static private RolesManager rolesManager => Plugin.Instance.rolesManager;
        static private RoleHUD roleHUD => Plugin.Instance.roleHUD;
        static private QuotaManager quotaManager => Plugin.Instance.quotaManager;




        [HarmonyPostfix]
        [HarmonyPatch("SyncScrapValuesClientRpc")]
        static void SendPlayersTheirRoleAndSetNewQuota(RoundManager __instance)
        {
            if (rolesManager == null || quotaManager == null) return;
            if (rolesManager.hasAlreadyDistributedRolesThisRound) return;

            // Only host can send roles
            if (!(__instance.IsHost || __instance.IsServer))
            {
                return;
            }

            quotaManager.ResetTrackedScrap();
            rolesManager.ResetCurrentQuotaValueServerRpc();

            if (RoundManager.Instance.currentLevel.name == "CompanyBuildingLevel")
            {
                rolesManager.CheatQuotaServerRpc();
                return;
            }

            logger.LogInfo("Providing roles");
            rolesManager.BuildAndSendRoles();

            quotaManager.ComputeAndSetNewDailyQuota();


        }
    }
}
