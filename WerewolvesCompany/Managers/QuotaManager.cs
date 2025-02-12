using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using GameNetcodeStuff;
using UnityEngine;
using WerewolvesCompany.Managers;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Managers
{
    internal class QuotaManager : MonoBehaviour
    {
        public QuotaManager Instance;

        public RolesManager rolesManager => Plugin.Instance.rolesManager;
        public RoleHUD roleHUD => Plugin.Instance.roleHUD;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;
        public ManualLogSource logupdate = Plugin.Instance.logupdate;

        public int currentScrapValue = 0;
        public int requiredDailyQuota = 100;

        public bool isQuotaMet => (currentScrapValue >= requiredDailyQuota);

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep it across scenes if needed
            }
            else
            {
                //logger.LogInfo("Duplicate detected, delted the just-created RoleHUD");
                Destroy(gameObject); // Prevent duplicate instances
            }
        }


        void Update()
        {
            
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
            logger.LogInfo($"Set new daily quota to {quotaValue}");
            requiredDailyQuota = quotaValue;
        }

        public int ComputeDailyQuota()
        {
            // Quota from Infected Company, will require testing
            int levelTotalValue = Mathf.RoundToInt(RoundManager.Instance.totalScrapValueInLevel);
            int Nplayers = StartOfRound.Instance.connectedPlayersAmount;
            float playersWeightedQuota = (float)Mathf.Max(0, Nplayers - 3) * 0.05f;
            float playersAndLevelWeightedQuota = (float)levelTotalValue * (0.25f + playersWeightedQuota);
            float finalQuota = Mathf.Min(playersAndLevelWeightedQuota, (float)levelTotalValue * 0.5f);

            return (int)finalQuota;
        }

        public void ComputeAndSetNewDailyQuota()
        {
            logger.LogInfo("Computing the new daily Quota");
            rolesManager.SetNewDailyQuotaServerRpc(ComputeDailyQuota());
        }
    }
}
