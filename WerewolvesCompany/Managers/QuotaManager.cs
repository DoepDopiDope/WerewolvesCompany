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
        public int requiredScrapValue = 100;

        public bool isQuotaMet => (currentScrapValue >= requiredScrapValue);

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
    }
}
