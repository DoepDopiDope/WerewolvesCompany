using BepInEx.Logging;
using UnityEngine;
using System.Collections.Generic;
using WerewolvesCompany.Config;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Managers
{
    internal class QuotaManager : MonoBehaviour
    {
        public static QuotaManager Instance { get; private set; }

        public RolesManager rolesManager => Plugin.Instance.rolesManager;
        public ConfigManager configManager => Plugin.Instance.configManager;
        public RoleHUD roleHUD => Plugin.Instance.roleHUD;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public int currentScrapValue = 0;
        public int requiredDailyQuota = 100;
        private readonly HashSet<int> countedScrapObjects = new HashSet<int>();

        public bool isQuotaMet => (currentScrapValue >= requiredDailyQuota);

        public bool useQuota => configManager.useQuota.Value;





        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep it across scenes if needed
                Plugin.Instance.quotaManager = this;
            }
            else
            {
                //logger.LogInfo("Duplicate detected, delted the just-created RoleHUD");
                Destroy(gameObject); // Prevent duplicate instances
            }
        }


        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (Plugin.Instance != null && Plugin.Instance.quotaManager == this)
                Plugin.Instance.quotaManager = null;
        }

        public bool TryRegisterScrap(GrabbableObject scrap)
        {
            return scrap != null && countedScrapObjects.Add(scrap.GetInstanceID());
        }

        public void ResetTrackedScrap()
        {
            countedScrapObjects.Clear();
        }

        public void AddScrapValue(int scrapValue)
        {
            logdebug.LogInfo($"Added quota value : {scrapValue}");
            currentScrapValue += scrapValue;
        }

        public void ResetScrapValue()
        {
            logger.LogInfo("Quota value has been reset");
            currentScrapValue = 0;
        }

        public void CheatValue()
        {
            logger.LogInfo("Quota value has been cheated to meet the daily quota");
            currentScrapValue = requiredDailyQuota;
        }

        public void SetNewDailyQuota(int quotaValue)
        {
            //if (!useQuota)
            //{
            //    quotaValue = 0;
            //}

            logger.LogInfo($"Set new daily quota to {quotaValue}");
            requiredDailyQuota = quotaValue;
        }

        public int ComputeDailyQuota()
        {
            // Quota from Infected Company, will require testing
            int levelTotalValue = Mathf.RoundToInt(RoundManager.Instance.totalScrapValueInLevel);
            int Nplayers = rolesManager.GetActivePlayers().Count;
            float playersWeightedQuota = (float)Mathf.Max(0, Nplayers - configManager.quotaNplayersOffset.Value) * configManager.quotaPlayersWeight.Value;
            float playersAndLevelWeightedQuota = (float)levelTotalValue * (configManager.quotaMinMultiplier.Value + playersWeightedQuota);
            float finalQuota = Mathf.Min(playersAndLevelWeightedQuota, (float)levelTotalValue * configManager.quotaMaxMultiplier.Value);

            return (int)finalQuota;
        }

        public void ComputeAndSetNewDailyQuota()
        {
            logger.LogInfo("Computing the new daily Quota");
            rolesManager.SetNewDailyQuotaServerRpc(ComputeDailyQuota());
        }
    }
}
