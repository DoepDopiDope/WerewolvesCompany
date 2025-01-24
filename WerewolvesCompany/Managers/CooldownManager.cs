using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.PlayerLoop;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Managers
{
    public class CooldownManager : MonoBehaviour
    {
        public CooldownManager Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep it across scenes if needed
            }
            else
            {
                logger.LogInfo("Duplicate detected, delted the just-created CooldownManager");
                Destroy(gameObject); // Prevent duplicate instances
            }
        }

        public void Update()
        {
            RolesManager rolesManagerObject = Utils.GetRolesManager();
            if (rolesManagerObject.myRole == null) return;
            rolesManagerObject.myRole.UpdateCooldowns(Time.deltaTime);
        }

    }
}
