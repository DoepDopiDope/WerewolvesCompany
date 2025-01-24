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

        internal static InputsClass InputActionsInstance;

        private readonly Harmony harmony = new Harmony(GUID);

        public static Plugin Instance;

        public GameObject netManagerPrefab;

        public ManualLogSource logger;
        public ManualLogSource logdebug;
        public ManualLogSource logupdate;

        public System.Random rng;



        public static ConfigEntry<float>
            // Default parameters
            config_DefaultInteractRange,
            config_DefaultActionCoolDown,

            // Werewolf parameters
            config_WerewolfInteractRange,
            config_WerewolfActionCoolDown,

            // Villager parameters
            config_VillagerInteractRange,
            config_VillagerActionCoolDown,

            // Witch parameters
            config_WitchInteractRange,
            config_WitchActionCoolDown,

            // Seer parameters
            config_SeerInteractRange,
            config_SeerActionCoolDown,

            // Wild Boy parameters
            config_WildBoyInteractRange,
            config_WildBoyActionCoolDown
            ;

        private void ConfigSetup()
        {
            // Default parameters
            config_DefaultInteractRange = Config.Bind("Default Interact Range", "Value", 1.0f, "How far the player can use his Action on another player.");
            config_DefaultActionCoolDown = Config.Bind("Default Role Action Cooldown", "Value", 60f, "How often can a player use his action (in seconds).");

            // Werewolf parameters
            config_WerewolfInteractRange = Config.Bind("Werewolf Kill Range", "Value", 1.0f, "How far a Werewolf can kill another player.");
            config_WerewolfActionCoolDown = Config.Bind("Werewolf Kill Cooldown", "Value", 60f, "How often a Werewolf can kill another player (in seconds).");

            // Villager parameters
            config_VillagerInteractRange = Config.Bind("Villager Interact Range", "Value", 1.0f, "Unused -- How far the player can use his Action on another player");
            config_VillagerActionCoolDown = Config.Bind("Villager Action Cooldown", "Value", 0f, "Unused -- How often can a Villager use his action (in seconds).");

            // Witch parameters
            config_WitchInteractRange = Config.Bind("Witch Interact Range", "Value", 1.0f, "How far a Witch can use a potion on another.");
            config_WitchActionCoolDown = Config.Bind("Witch Potion Cooldown", "Value", 9999f, "How often a Witch can use each potion (in seconds).");

            // Seer parameters
            config_SeerInteractRange = Config.Bind("Seer Seer Range", "Value", 10f, "How far the Seer can seer another player role.");
            config_SeerActionCoolDown = Config.Bind("Seer Seer Cooldown", "Value", 120f, "How often the Seer can seer another player role (in seconds).");

            // Wild Boy parameters
            config_WildBoyInteractRange = Config.Bind("Wild Boy Idolize Range", "Value", 30f, "How far the Wild Boy can idolize another player.");
            config_WildBoyActionCoolDown = Config.Bind("Wild Boy Idolize Cooldown", "Value", 9999f, "How often the Wild Boy can idolize another player (in seconds).");
        }


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
            InputActionsInstance = new InputsClass();
            
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
            //InitializeRolesManager();
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
            //InitializeRolesManager();
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



