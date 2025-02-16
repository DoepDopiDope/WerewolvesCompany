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
using WerewolvesCompany.Inputs;



namespace WerewolvesCompany
{
    
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    class Plugin : BaseUnityPlugin
    {
        const string GUID = "doep.WerewolvesCompany";
        const string NAME = "WerewolvesCompany";
        const string VERSION = "0.5.3";

        internal static InputsKeybinds InputActionsInstance;

        private readonly Harmony harmony = new Harmony(GUID);

        public static Plugin Instance;

        public GameObject netManagerPrefab;

        public ManualLogSource logger;
        public ManualLogSource logdebug;
        public ManualLogSource logupdate;

        public System.Random rng;
        
        public ModManager modManager;
        public RolesManager rolesManager;
        public RoleHUD roleHUD;
        public CooldownManager cooldownManager;
        public QuotaManager quotaManager;



        // Global parameters
        public static ConfigEntry<bool> config_CanWerewolvesSeeEachOther;
        public static ConfigEntry<bool> config_DisableTooltipWhenBodyDroppedInShip;
        public static ConfigEntry<float> config_VoteCooldown;
        public static ConfigEntry<float> config_VoteAmount;

        // Quota parameters
        public static ConfigEntry<float> config_QuotaMinMultiplier;
        public static ConfigEntry<float> config_QuotaPlayersWeight;
        public static ConfigEntry<int>   config_QuotaNplayersOffset;
        public static ConfigEntry<float> config_QuotaMaxMultiplier;

        // Werewolf Role Parameters
        public static ConfigEntry<float> config_WerewolfInteractRange;
        public static ConfigEntry<float> config_WerewolfActionCoolDown;
        public static ConfigEntry<float> config_WerewolfStartOfRoundActionCoolDown;

        // Villager Role Parameters
        public static ConfigEntry<float> config_VillagerInteractRange;
        public static ConfigEntry<float> config_VillagerActionCoolDown;
        public static ConfigEntry<float> config_VillagerStartOfRoundActionCoolDown;

        // Witch Role Parameters
        public static ConfigEntry<float> config_WitchInteractRange;
        public static ConfigEntry<float> config_WitchActionCoolDown;
        public static ConfigEntry<float> config_WitchStartOfRoundActionCoolDown;

        // Seer Role Parameters
        public static ConfigEntry<float> config_SeerInteractRange;
        public static ConfigEntry<float> config_SeerActionCoolDown;
        public static ConfigEntry<float> config_SeerStartOfRoundActionCoolDown;

        // Wild Boy Role Parameters
        public static ConfigEntry<float> config_WildBoyInteractRange;
        public static ConfigEntry<float> config_WildBoyActionCoolDown;
        public static ConfigEntry<float> config_WildBoyStartOfRoundActionCoolDown;

        // Cupid Role Parameters
        public static ConfigEntry<float> config_CupidInteractRange;
        public static ConfigEntry<float> config_CupidActionCoolDown;
        public static ConfigEntry<float> config_CupidStartOfRoundActionCoolDown;
        
        

        private void ConfigSetup()
        {

            // -----------------------------------------------
            // Global parameters

            // Vote parameters
            config_CanWerewolvesSeeEachOther = Config.Bind("Global Parameters", "Werewolves Know Each Other", true, "Do werewolves know each other?");
            config_DisableTooltipWhenBodyDroppedInShip = Config.Bind("Global Parameters", "Disable Body in Ship tooltip", true, "Prevents the display of the tooltip to all players when a body is dropped in the ship.");
            config_VoteCooldown                        = Config.Bind("Global Parameters", "Vote Kill Cooldown", 120f, "Cooldown for the next vote after someone has been vote-kicked.");
            config_VoteAmount                          = Config.Bind("Global Parameters", "Vote Kill Required Amount", 0.5f, "Which fraction of the alive players are required to vote kick someone.");

            // Quota parameters
            config_QuotaMinMultiplier = Config.Bind("Quota", "minMultiplier", 0.25f, "Minimum multiplier to the total level value.");
            config_QuotaPlayersWeight  = Config.Bind("Quota", "playerWeight", 0.05f, "Multiplier weight of each player to the multiplier.");
            config_QuotaNplayersOffset = Config.Bind("Quota", "NplayersMin", 3, "Number of players at which player Weight is counted positively.");
            config_QuotaMaxMultiplier  = Config.Bind("Quota", "maxMultiplier", 0.5f, "Fraction of the total level value that the quota cannot exceed.");

            // -----------------------------------------------
            // Roles parameters

            // Werewolf parameters
            config_WerewolfInteractRange              = Config.Bind("Role: Werewolf", "Kill Range", 1.5f, "How far a Werewolf can kill another player.");
            config_WerewolfActionCoolDown             = Config.Bind("Role: Werewolf", "Kill Cooldown", 120f, "How often a Werewolf can kill another player (in seconds).");
            config_WerewolfStartOfRoundActionCoolDown = Config.Bind("Role: Werewolf", "Kill Cooldown at start of round", 120f, "How soon after the start of a round a Werewolf can Kill someone (in seconds).");

            // Villager parameters
            config_VillagerInteractRange              = Config.Bind("Role: Villager", "PatPat Range", 1.5f, "Unused -- How far the player can use his Action on another player");
            config_VillagerActionCoolDown             = Config.Bind("Role: Villager", "PatPat Cooldown", 0f, "Unused -- How often can a Villager use his action (in seconds).");
            config_VillagerStartOfRoundActionCoolDown = Config.Bind("Role: Villager", "PatPat Cooldown at start of round", 0f, "How soon after a round has started a Villager can PatPat another villager (in seconds).");

            // Witch parameters
            config_WitchInteractRange              = Config.Bind("Role: Witch", "Potion Range", 1.5f, "How far a Witch can use a potion on another.");
            config_WitchActionCoolDown             = Config.Bind("Role: Witch", "Potion Cooldown", 9999f, "How often a Witch can use each potion (in seconds).");
            config_WitchStartOfRoundActionCoolDown = Config.Bind("Role: Witch", "Potion Cooldown at start of round", 120f, "How soon after a round has started a Witch can use her potions (in seconds).");

            // Seer parameters
            config_SeerInteractRange              = Config.Bind("Role: Seer", "Seer Range", 10f, "How far the Seer can seer another player role.");
            config_SeerActionCoolDown             = Config.Bind("Role: Seer", "Seer Cooldown", 120f, "How often the Seer can seer another player role (in seconds).");
            config_SeerStartOfRoundActionCoolDown = Config.Bind("Role: Seer", "Seer Cooldown at start of round", 120f, "How soon after a round has started a Seer can seer a player role (in seconds).");

            // Wild Boy parameters
            config_WildBoyInteractRange              = Config.Bind("Role: Wild Boy", "Idolize Range", 30f, "How far the Wild Boy can idolize another player.");
            config_WildBoyActionCoolDown             = Config.Bind("Role: Wild Boy", "Idolize Cooldown", 9999f, "How often the Wild Boy can idolize another player (in seconds).");
            config_WildBoyStartOfRoundActionCoolDown = Config.Bind("Role: Wild Boy", "Idolize Cooldown at start of round", 0f, "How soon after a round has started a WildBoy can idolize another player (in seconds).");

            // Cupid parameters
            config_CupidInteractRange              = Config.Bind("Role: Cupid", "Romance Range", 30f, "How far Cupid can make someone fall in love.");
            config_CupidActionCoolDown             = Config.Bind("Role: Cupid", "Romance Cooldown", 9999f, "How often Cupid can create lovers (in seconds).");
            config_CupidStartOfRoundActionCoolDown = Config.Bind("Role: Cupid", "Romance Cooldown at start of round", 0f, "How soon after a round has started Cupid can create lovers (in seconds).");
        }


        void Awake()
        {
            

            // Setup logging
            logger = BepInEx.Logging.Logger.CreateLogSource($"{GUID}");
            logdebug = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- debug");
            logupdate = BepInEx.Logging.Logger.CreateLogSource($"{GUID} -- update");
            logger.LogInfo("Plugin is initializing...");


            //BepInEx.Logging.Logger.Sources.Remove(logdebug);
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
            logdebug.LogInfo("Setting up the config");
            ConfigSetup();

            // Initiate the Inputs class
            //logdebug.LogInfo("Create the inputs class");
            //InitiateInputsSystem();

            // Patch the game using Harmony
            logdebug.LogInfo("Harmony patching");
            harmony.PatchAll();

            // Initialize the random number generator
            logdebug.LogInfo("Initiate the random generator");
            rng = new System.Random();

            // Load asset bundle for RolesManager
            logdebug.LogInfo("Creating the roles manager");
            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcodemod");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);
            netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/WerewolvesCompany/RolesManager.prefab");
            netManagerPrefab.AddComponent<RolesManager>();

            // Create a persistent ModManager to handle game initialization
            logger.LogInfo("Plugin.Awake() is creating ModManager.");
            GameObject modManagerObject = new GameObject("ModManager");
            modManagerObject.AddComponent<ModManager>();
            DontDestroyOnLoad(modManagerObject);
            logger.LogInfo("ModManager GameObject created.");

            this.modManager = modManagerObject.GetComponent<ModManager>();
            // Run checks
            //RunChecks();

        }

        private void RunChecks()
        {
            References.CheckIndividualRefInt();

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
            InitializeHUD();
            //InitializeCooldownManager();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            logdebug.LogInfo($"Scene loaded: {scene.name}. Reinitializing HUD components...");
            InitializeHUD();
            InitializeCooldownManager();
            InitializeQuotaManager();
            //InitializeRolesManager();
        }

        private void InitializeRolesManager()
        {
            if (FindObjectOfType<RolesManager>() == null)
            {
                GameObject rolesManagerObject = new GameObject("RolesManager");
                rolesManagerObject.AddComponent<RolesManager>();
                logdebug.LogInfo("RolesManager has been recreated.");
                Plugin.Instance.rolesManager = rolesManagerObject.GetComponent<RolesManager>();
            }
        }

        private void InitializeHUD()
        {
            if (FindObjectOfType<RoleHUD>() == null)
            {
                GameObject roleHUDObject = new GameObject("RoleHUD");
                roleHUDObject.AddComponent<RoleHUD>();
                logdebug.LogInfo("RoleHUD has been recreated.");
                Plugin.Instance.roleHUD = roleHUDObject.GetComponent<RoleHUD>();
            }
        }
        
        private void InitializeCooldownManager()
        {
            if (FindObjectOfType<CooldownManager>() == null)
            {
                GameObject cooldownManagerObject = new GameObject("CooldownManager");
                cooldownManagerObject.AddComponent<CooldownManager>();
                logdebug.LogInfo("CooldownManager has been recreated.");
                Plugin.Instance.cooldownManager = cooldownManagerObject.GetComponent<CooldownManager>();
            }
        }

        private void InitializeQuotaManager()
        {
            if (FindObjectOfType<QuotaManager>() == null)
            {
                GameObject quotaManagerObject = new GameObject("QuotaManager");
                quotaManagerObject.AddComponent<QuotaManager>();
                logdebug.LogInfo("QuotaManager has been recreated.");
                Plugin.Instance.quotaManager = quotaManagerObject.GetComponent<QuotaManager>();
            }
        }


    }
}



