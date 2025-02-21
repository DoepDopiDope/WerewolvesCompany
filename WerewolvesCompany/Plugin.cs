using BepInEx;
using HarmonyLib;
using WerewolvesCompany.Managers;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using WerewolvesCompany.UI;
using UnityEngine.SceneManagement;

using WerewolvesCompany.Inputs;
using WerewolvesCompany.Config;
using System;



namespace WerewolvesCompany
{

    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    class Plugin : BaseUnityPlugin
    {
        const string GUID = "doep.WerewolvesCompany";
        const string NAME = "WerewolvesCompany";
        const string VERSION = "0.5.4";

        internal static InputsKeybinds InputActionsInstance;

        private readonly Harmony harmony = new Harmony(GUID);

        public static Plugin Instance;

        public GameObject rolesManagerPrefab;
        public GameObject configManagerPrefab;

        public ManualLogSource logger;
        public ManualLogSource logdebug;

        public System.Random rng;
        
        public ModManager modManager;
        public RolesManager rolesManager;
        public ConfigManager configManager;
        public RoleHUD roleHUD;
        public CooldownManager cooldownManager;
        public QuotaManager quotaManager;
        

        private void InitializeNetCodeStuff()
        {
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
        }

        void Awake()
        {
            

            // Setup logging
            logger = BepInEx.Logging.Logger.CreateLogSource($"{GUID}");
            logdebug = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- debug");
            logger.LogInfo("Plugin is initializing...");

            //BepInEx.Logging.Logger.Sources.Remove(logdebug);


            // Does stuff for the netcode stuff
            InitializeNetCodeStuff();

            // Assign the plugin Instance
            Instance = this;

            // Initiate config
            logdebug.LogInfo("Setting up the config");
            ConfigParameters.ConfigSetup();

            // Harmony Patches
            logdebug.LogInfo("Harmony patching");
            harmony.PatchAll();

            // Initialize the random number generator
            logdebug.LogInfo("Initiate the random generator");
            rng = new System.Random();


            // Load asset bundle for RolesManager
            logdebug.LogInfo("Creating the roles manager");
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcodemod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            rolesManagerPrefab = bundle.LoadAsset<GameObject>("Assets/WerewolvesCompany/RolesManager.prefab");
            rolesManagerPrefab.AddComponent<RolesManager>();


            // Load asset bundle for ParametersManager
            logdebug.LogInfo("Creating the config manager");
            configManagerPrefab = bundle.LoadAsset<GameObject>("Assets/WerewolvesCompany/ConfigManager.prefab");
            configManagerPrefab.AddComponent<ConfigManager>();


            // Create a persistent ModManager to handle game initialization
            logger.LogInfo("Plugin.Awake() is creating ModManager.");
            GameObject modManagerObject = new GameObject("ModManager");
            modManagerObject.AddComponent<ModManager>();
            DontDestroyOnLoad(modManagerObject);
            logger.LogInfo("ModManager GameObject created.");


            // Create the ModManager Instance
            this.modManager = modManagerObject.GetComponent<ModManager>();
            // Run checks
            RunChecks();

        }

        private void RunChecks()
        {
            logdebug.LogInfo("============= Checks =============");
            logdebug.LogInfo("Checking for duplicate refInts");
            try
            {
                References.CheckIndividualRefInt();
                logdebug.LogInfo("--> OK");
            }
            catch (Exception e)
            {
                logdebug.LogError("There are duplicate roles refInts");
                logdebug.LogError(e);
            }
        }

        public void InitiateInputsSystem()
        {
            InputActionsInstance = new InputsKeybinds();
        }

    }

    public class ModManager : MonoBehaviour
    {
        public static ModManager Instance { get; private set; }
        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;


        void Awake()
        {
            // Singleton pattern to ensure only one Instance of ModManager exists
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded; // Subscribe to scene
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            
            logger.LogInfo("ModManager: Awake() called. Initialization started.");

            // Run the Start() manually, I cannot figure out why it won't Start() on its own
            logger.LogInfo("Manually initializing the ModManager");
            Start();
            logger.LogInfo("ModManager setup is complete.");
        }


        void Start()
        {
            InitializeRolesManager();
            InitializeConfigManager();
            InitializeHUD();
            //InitializeCooldownManager();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            logdebug.LogInfo($"Scene loaded: {scene.name}. Reinitializing HUD components...");
            InitializeHUD();
            //InitializeCooldownManager();
            InitializeQuotaManager();
            //InitializeRolesManager();
        }

        private void InitializeRolesManager()
        {
            if (FindObjectOfType<RolesManager>() == null)
            {
                GameObject rolesManagerObject = new GameObject("RolesManager");
                rolesManagerObject.AddComponent<RolesManager>();
                logdebug.LogWarning("RolesManager has been recreated.");
                Plugin.Instance.rolesManager = rolesManagerObject.GetComponent<RolesManager>();
            }
        }

        private void InitializeConfigManager()
        {
            if (FindObjectOfType<ConfigManager>() == null)
            {
                GameObject rolesManagerObject = new GameObject("ConfigManager");
                rolesManagerObject.AddComponent<ConfigManager>();
                logdebug.LogWarning("ConfigManager has been recreated.");
                Plugin.Instance.configManager = rolesManagerObject.GetComponent<ConfigManager>();
            }
        }



        private void InitializeHUD()
        {
            if (FindObjectOfType<RoleHUD>() == null)
            {
                GameObject roleHUDObject = new GameObject("RoleHUD");
                roleHUDObject.AddComponent<RoleHUD>();
                logdebug.LogWarning("RoleHUD has been recreated.");
                Plugin.Instance.roleHUD = roleHUDObject.GetComponent<RoleHUD>();
            }
        }
        
        //private void InitializeCooldownManager()
        //{
        //    if (FindObjectOfType<CooldownManager>() == null)
        //    {
        //        GameObject cooldownManagerObject = new GameObject("CooldownManager");
        //        cooldownManagerObject.AddComponent<CooldownManager>();
        //        logdebug.LogWarning("CooldownManager has been recreated.");
        //        Plugin.Instance.cooldownManager = cooldownManagerObject.GetComponent<CooldownManager>();
        //    }
        //}

        private void InitializeQuotaManager()
        {
            if (FindObjectOfType<QuotaManager>() == null)
            {
                GameObject quotaManagerObject = new GameObject("QuotaManager");
                quotaManagerObject.AddComponent<QuotaManager>();
                logdebug.LogWarning("QuotaManager has been recreated.");
                Plugin.Instance.quotaManager = quotaManagerObject.GetComponent<QuotaManager>();
            }
        }


    }
}



