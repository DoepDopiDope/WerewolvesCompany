using BepInEx;
using HarmonyLib;
using WerewolvesCompany.Managers;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using WerewolvesCompany.UI;

namespace WerewolvesCompany
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "doep.WerewolvesCompany";
        const string NAME = "WerewolvesCompany";
        const string VERSION = "0.0.1";

        private readonly Harmony harmony = new Harmony(GUID);

        public static Plugin Instance;

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

            Logger.LogInfo("Plugin is initializing...");

            // Assign the plugin Instance
            Instance = this;

            // Setup logging
            logger = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- main");
            logdebug = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- debug");

            // Patch the game using Harmony
            harmony.PatchAll();

            // Initialize the random number generator
            rng = new System.Random();

            // Load asset bundle for NetworkManagerWerewolvesCompany
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcodemod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/WerewolvesCompany/NetworkManagerWerewolvesCompany.prefab");
            netManagerPrefab.AddComponent<NetworkManagerWerewolvesCompany>();

            // Create a persistent ModManager to handle game initialization
            GameObject modManagerObject = new GameObject("ModManager");
            modManagerObject.AddComponent<ModManager>();
            DontDestroyOnLoad(modManagerObject);

            Logger.LogInfo("ModManager has been initialized.");
        }
    }

    public class ModManager : MonoBehaviour
    {
        public static ModManager Instance { get; private set; }

        void Awake()
        {
            // Singleton pattern to ensure only one Instance of ModManager exists
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize the RolesManager
            if (RolesManager.Instance == null)
            {
                Plugin.Instance.logdebug.LogInfo("RolesManager is null, creating it.");
                GameObject rolesManagerObject = new GameObject("RolesManager");
                rolesManagerObject.AddComponent<RolesManager>();
                Plugin.Instance.logger.LogInfo("RolesManager has been created.");
            }

            Plugin.Instance.logger.LogInfo("ModManager: Awake() called. Initialization started.");
        }

        void Start()
        {
            Plugin.Instance.logger.LogInfo("ModManager is setting up the game...");



            // Add the HUDInitializer
            GameObject hudInitializerObject = new GameObject("HUDInitializer");
            hudInitializerObject.AddComponent<HUDInitializer>();
            Plugin.Instance.logger.LogInfo("HUDInitializer has been added to the scene.");

            // Example: Initialize some roles for testing
            Role werewolf = new Werewolf();
            Role villager = new Villager();
            Role witch = new Witch();
            Role seer = new Seer();

            werewolf.PerformRoleAction();
            villager.PerformRoleAction();
            witch.PerformRoleAction();
            seer.PerformRoleAction();

            Plugin.Instance.logger.LogInfo("ModManager setup is complete.");
        }
    }
}



