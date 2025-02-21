using BepInEx.Logging;
using Unity.Netcode;
using UnityEngine.PlayerLoop;

namespace WerewolvesCompany.Config
{
    internal class ConfigManager : NetworkBehaviour
    {
        public ConfigManager Instance;
        public ManualLogSource logger => Plugin.Instance.logger;
        public ManualLogSource logdebug => Plugin.Instance.logdebug;



        // Global parameters
        public NetworkVariable<bool> CanWerewolvesSeeEachOther = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<bool> DisableTooltipWhenBodyDroppedInShip = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> VoteCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> VoteAmount = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Quota parameters
        public NetworkVariable<float> quotaMinMultiplier = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> quotaPlayersWeight = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> quotaNplayersOffset = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> quotaMaxMultiplier = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Werewolf parameters
        public NetworkVariable<float> WerewolfInteractRange = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> WerewolfActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> WerewolfStartOfRoundActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Villager parameters
        public NetworkVariable<float> VillagerInteractRange = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> VillagerActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> VillagerStartOfRoundActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Witch parameters
        public NetworkVariable<float> WitchInteractRange = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> WitchActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> WitchStartOfRoundActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Seer parameters
        public NetworkVariable<float> SeerInteractRange = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> SeerActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> SeerStartOfRoundActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Wild Boy parameters
        public NetworkVariable<float> WildBoyInteractRange = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> WildBoyActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> WildBoyStartOfRoundActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> WildBoyActionCooldownOnTransform = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        // Wild Boy parameters
        public NetworkVariable<float> CupidInteractRange = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> CupidActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<float> CupidStartOfRoundActionCooldown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep it across scenes if needed
                Plugin.Instance.configManager = this;
            }
            else
            {
                logdebug.LogInfo("Duplicate detected, delted the just-created RolesManager");
                Destroy(gameObject); // Prevent duplicate instances
            }
        }

        public override void OnNetworkSpawn()
        {
            logdebug.LogInfo("ConfigManager NetworkSpawn");

            if (IsServer)
            {
                UpdateConfigValues();
            }
        }

        public void UpdateConfigValues()
        {
            if (!IsHost) return;
            // Global parameters
            CanWerewolvesSeeEachOther.Value = ConfigParameters.config_CanWerewolvesSeeEachOther.Value;
            DisableTooltipWhenBodyDroppedInShip.Value = ConfigParameters.config_DisableTooltipWhenBodyDroppedInShip.Value;
            VoteCooldown.Value = ConfigParameters.config_VoteCooldown.Value;
            VoteAmount.Value = ConfigParameters.config_VoteAmount.Value;

            // Quota parameter
            quotaMinMultiplier.Value = ConfigParameters.config_QuotaMinMultiplier.Value;
            quotaPlayersWeight.Value = ConfigParameters.config_QuotaPlayersWeight.Value;
            quotaNplayersOffset.Value = ConfigParameters.config_QuotaNplayersOffset.Value;
            quotaMaxMultiplier.Value = ConfigParameters.config_QuotaMaxMultiplier.Value;

            // Werewolf parameters
            WerewolfInteractRange.Value = ConfigParameters.config_WerewolfInteractRange.Value;
            WerewolfActionCooldown.Value = ConfigParameters.config_WerewolfActionCooldown.Value;
            WerewolfStartOfRoundActionCooldown.Value = ConfigParameters.config_WerewolfStartOfRoundActionCooldown.Value;

            // Villager parameters
            VillagerInteractRange.Value = ConfigParameters.config_VillagerInteractRange.Value;
            VillagerActionCooldown.Value = ConfigParameters.config_VillagerActionCooldown.Value;
            VillagerStartOfRoundActionCooldown.Value = ConfigParameters.config_VillagerStartOfRoundActionCooldown.Value;

            // Witch parameters
            WitchInteractRange.Value = ConfigParameters.config_WitchInteractRange.Value;
            WitchActionCooldown.Value = ConfigParameters.config_WitchActionCooldown.Value;
            WitchStartOfRoundActionCooldown.Value = ConfigParameters.config_WitchStartOfRoundActionCooldown.Value;

            // Seer parameters
            SeerInteractRange.Value = ConfigParameters.config_SeerInteractRange.Value;
            SeerActionCooldown.Value = ConfigParameters.config_SeerActionCooldown.Value;
            SeerStartOfRoundActionCooldown.Value = ConfigParameters.config_SeerStartOfRoundActionCooldown.Value;

            // Wild Boy parameters
            WildBoyInteractRange.Value = ConfigParameters.config_WildBoyInteractRange.Value;
            WildBoyActionCooldown.Value = ConfigParameters.config_WildBoyActionCooldown.Value;
            WildBoyStartOfRoundActionCooldown.Value = ConfigParameters.config_WildBoyStartOfRoundActionCooldown.Value;
            WildBoyActionCooldownOnTransform.Value = ConfigParameters.config_WildBoyActionCooldownOnTransform.Value;

            // Cupid parameters
            CupidInteractRange.Value = ConfigParameters.config_CupidInteractRange.Value;
            CupidActionCooldown.Value = ConfigParameters.config_CupidActionCooldown.Value;
            CupidStartOfRoundActionCooldown.Value = ConfigParameters.config_CupidStartOfRoundActionCooldown.Value;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            logdebug.LogError($"{name} has been destroyed!");
        }
    }
}
