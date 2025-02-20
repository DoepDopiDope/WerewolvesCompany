using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib.Tools;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Managers
{
    public class NetworkManagerWerewolvesCompany : NetworkBehaviour
    {
        public static NetworkManagerWerewolvesCompany Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        void Awake()
        {
            Instance = this;
        }
    }
}