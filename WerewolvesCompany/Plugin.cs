using BepInEx;
using HarmonyLib;
using WerewolvesCompany.Managers;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using WerewolvesCompany.UI;
using HarmonyLib.Tools;
using UnityEngine.SceneManagement;
using BepInEx.Configuration;

using LethalCompanyInputUtils.Api;

namespace WerewolvesCompany
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        const string GUID = "doep.WerewolvesCompany";
        const string NAME = "WerewolvesCompany";
        const string VERSION = "0.0.1";

        internal static MyExampleInputClass InputActionsInstance;

        private readonly Harmony harmony = new Harmony(GUID);

        public static Plugin Instance;

        public GameObject netManagerPrefab;

        public ManualLogSource logger;
        public ManualLogSource logdebug;
        public ManualLogSource logupdate;

        public System.Random rng;


        public static ConfigEntry<float>
            config_InteractRange,
            config_RoleActionCoolDown;



        void Awake()
        {
            // Setup logging
            logger = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- main");
            logdebug = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- debug");
            logupdate = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- update");
            logger.LogInfo("Plugin is initializing...");


            BepInEx.Logging.Logger.Sources.Remove(logupdate);



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

            // Assign the plugin Instance
            Instance = this;

            // Initiate config
            ConfigSetup();

            // Initiate the Inputs class
            InputActionsInstance = new MyExampleInputClass();
            
            // Patch the game using Harmony
            harmony.PatchAll();

            // Initialize the random number generator
            rng = new System.Random();

            // Load asset bundle for RolesManager
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcodemod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/WerewolvesCompany/RolesManager.prefab");
            netManagerPrefab.AddComponent<RolesManager>();

            // Create a persistent ModManager to handle game initialization
            logger.LogInfo("Plugin.Awake() is creating ModManager.");
            GameObject modManagerObject = new GameObject("ModManager");
            modManagerObject.AddComponent<ModManager>();
            DontDestroyOnLoad(modManagerObject);
            Plugin.Instance.logger.LogInfo("ModManager GameObject created.");   
        }

        private void ConfigSetup()
        {
            config_InteractRange = Config.Bind("Interact Range", "Value", 3.0f, "How far the player can use his Action on another player");
            config_RoleActionCoolDown = Config.Bind("Role Action Cooldown", "Value", 60f, "How often can a player use his action.");
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
            // Add the HUDInitializer
            InitializeRolesManager();
            InitializeHUD();
            //GameObject hudInitializerObject = new GameObject("HUDInitializer");
            //hudInitializerObject.AddComponent<HUDInitializer>();
            //logger.LogInfo("HUDInitializer has been added to the scene.");

            // Example: Initialize some roles for testing
            //Role werewolf = new Werewolf();
            //Role villager = new Villager();
            //Role witch = new Witch();
            //Role seer = new Seer();

            //werewolf.PerformRoleAction();
            //villager.PerformRoleAction();
            //witch.PerformRoleAction();
            //seer.PerformRoleAction();

        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.Instance.logger.LogInfo($"Scene loaded: {scene.name}. Reinitializing HUD components...");
            InitializeHUD();
            InitializeRolesManager();
        }

        private void InitializeRolesManager()
        {
            if (FindObjectOfType<RolesManager>() == null)
            {
                GameObject rolesManagerObject = new GameObject("RolesManager");
                rolesManagerObject.AddComponent<RolesManager>();
                Plugin.Instance.logger.LogInfo("RolesManager has been recreated.");
            }

        }

        private void InitializeHUD()
        {
            if (FindObjectOfType<RoleHUD>() == null)
            {
                GameObject roleHUDObject = new GameObject("RoleHUD");
                roleHUDObject.AddComponent<RoleHUD>();
                logger.LogInfo("RoleHUD has been recreated.");
            }
        }


    }
}



