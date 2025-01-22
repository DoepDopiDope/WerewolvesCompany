

﻿using BepInEx;
using HarmonyLib;
using WerewolvesCompany.Managers;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;

namespace WerewolvesCompany
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "doep.WerewolvesCompany";
        const string NAME = "WerewolvesCompany";
        const string VERSION = "0.0.1";

        private readonly Harmony harmony = new Harmony(GUID);

        public static Plugin instance;

        public GameObject netManagerPrefab;

        public ManualLogSource logger;
        public ManualLogSource logdebug;

        public System.Random rng;

        void Awake()
        {
            // Does stuff for the netcode stuff
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }

            instance = this;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcodemod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/WerewolvesCompany/NetworkManagerWerewolvesCompany.prefab");
            netManagerPrefab.AddComponent<NetworkManagerWerewolvesCompany>();

            harmony.PatchAll();

            // Initiate the logger
            logger = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- main");
            logdebug = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- debug");

            //BepInEx.Logging.Logger.Sources.Remove(logdebug);
            

            // Initiate the random number generator
            rng = new System.Random();

            // Initiate the Role Manager object
            if (RolesManager.Instance == null)
            {
                logdebug.LogInfo("RolesManager is null, therefore making it");
                var rolesManagerGameObject = new GameObject("RolesManager");
                rolesManagerGameObject.AddComponent<RolesManager>();
                logger.LogInfo("RolesManager dynamically created in Start()");
            }


            // Iniate roles for testing
            Role werewolf = new Werewolf();
            Role villager = new Villager();
            Role witch = new Witch();
            Role seer = new Seer();

            werewolf.PerformRoleAction();
            villager.PerformRoleAction();
            witch.PerformRoleAction();
            seer.PerformRoleAction();

        }

        void Start()
        {
            Role werewolf = new Werewolf();
            Role villager = new Villager();

            werewolf.PerformRoleAction();
            villager.PerformRoleAction();
        }
    }
}
