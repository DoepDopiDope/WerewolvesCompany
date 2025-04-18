﻿using BepInEx.Configuration;
using LethalConfig;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using Unity.Netcode;
using UnityEngine.UI;
using System;
//using System;
//using System.Collections.Generic;
//using System.Text;

namespace WerewolvesCompany.Config
{
    internal static class ConfigParameters
    {
        // Global parameters
        public static ConfigEntry<bool> config_CanWerewolvesSeeEachOther;
        public static ConfigEntry<bool> config_DisableTooltipWhenBodyDroppedInShip;
        public static ConfigEntry<float> config_VoteCooldown;
        public static ConfigEntry<float> config_VoteAmount;

        // Quota parameters
        public static ConfigEntry<bool> config_UseQuota;
        public static ConfigEntry<float> config_QuotaMinMultiplier;
        public static ConfigEntry<float> config_QuotaPlayersWeight;
        public static ConfigEntry<int> config_QuotaNplayersOffset;
        public static ConfigEntry<float> config_QuotaMaxMultiplier;

        // Werewolf Role Parameters
        public static ConfigEntry<float> config_WerewolfInteractRange;
        public static ConfigEntry<float> config_WerewolfActionCooldown;
        public static ConfigEntry<float> config_WerewolfStartOfRoundActionCooldown;

        // Villager Role Parameters
        public static ConfigEntry<float> config_VillagerInteractRange;
        public static ConfigEntry<float> config_VillagerActionCooldown;
        public static ConfigEntry<float> config_VillagerStartOfRoundActionCooldown;

        // Witch Role Parameters
        public static ConfigEntry<float> config_WitchInteractRange;
        public static ConfigEntry<float> config_WitchActionCooldown;
        public static ConfigEntry<float> config_WitchStartOfRoundActionCooldown;

        // Seer Role Parameters
        public static ConfigEntry<float> config_SeerInteractRange;
        public static ConfigEntry<float> config_SeerActionCooldown;
        public static ConfigEntry<float> config_SeerStartOfRoundActionCooldown;

        // Wild Boy Role Parameters
        public static ConfigEntry<float> config_WildBoyInteractRange;
        public static ConfigEntry<float> config_WildBoyActionCooldown;
        public static ConfigEntry<float> config_WildBoyStartOfRoundActionCooldown;
        public static ConfigEntry<float> config_WildBoyActionCooldownOnTransform;

        // Cupid Role Parameters
        public static ConfigEntry<float> config_CupidInteractRange;
        public static ConfigEntry<float> config_CupidActionCooldown;
        public static ConfigEntry<float> config_CupidStartOfRoundActionCooldown;
        
        // Alpha Werewolf parameters
        public static ConfigEntry<float> config_AlphaWerewolfInteractRange;
        public static ConfigEntry<float> config_AlphaWerewolfActionCooldown;
        public static ConfigEntry<float> config_AlphaWerewolfStartOfRoundActionCooldown;
        public static ConfigEntry<float> config_AlphaWerewolfCooldownAfterTransform;


        public static void ConfigSetup()
        {
            Plugin.Instance.logdebug.LogInfo("Config bepinex binds");
            ConfigBepInExBinds();
            Plugin.Instance.logdebug.LogInfo("Config LethalConfig");
            ConfigLethalConfig();
        }


        public static void ConfigBepInExBinds()
        {
            // -----------------------------------------------
            // Global parameters

            // Vote parameters
            config_CanWerewolvesSeeEachOther = Plugin.Instance.Config.Bind("Global Parameters", "Werewolves Know Each Other", true, "Do werewolves know each other?");
            config_DisableTooltipWhenBodyDroppedInShip = Plugin.Instance.Config.Bind("Global Parameters", "Disable Body in Ship tooltip", true, "Prevents the display of the tooltip to all players when a body is dropped in the ship.");
            config_VoteCooldown = Plugin.Instance.Config.Bind("Global Parameters", "Vote Kill Cooldown", 120f, "Cooldown for the next vote after someone has been vote-kicked.");
            config_VoteAmount = Plugin.Instance.Config.Bind("Global Parameters", "Vote Kill Required Amount", 0.5f, "Which fraction of the alive players are required to vote kick someone.");

            // Quota parameters
            config_UseQuota          = Plugin.Instance.Config.Bind("Quota", "Use Quota", true, "Whether to use the daily quota feature or not.");
            config_QuotaMinMultiplier = Plugin.Instance.Config.Bind("Quota", "minMultiplier", 0.25f, "Minimum multiplier to the total level value.");
            config_QuotaPlayersWeight = Plugin.Instance.Config.Bind("Quota", "playerWeight", 0.05f, "Multiplier weight of each player to the multiplier.");
            config_QuotaNplayersOffset = Plugin.Instance.Config.Bind("Quota", "NplayersMin", 3, "Number of players at which player Weight is counted positively.");
            config_QuotaMaxMultiplier = Plugin.Instance.Config.Bind("Quota", "maxMultiplier", 0.5f, "Fraction of the total level value that the quota cannot exceed.");

            // -----------------------------------------------
            // Roles parameters

            // Werewolf parameters
            config_WerewolfInteractRange = Plugin.Instance.Config.Bind("Role: Werewolf", "Kill Range", 1.5f, "How far a Werewolf can kill another player.");
            config_WerewolfActionCooldown = Plugin.Instance.Config.Bind("Role: Werewolf", "Kill Cooldown", 120f, "How often a Werewolf can kill another player (in seconds).");
            config_WerewolfStartOfRoundActionCooldown = Plugin.Instance.Config.Bind("Role: Werewolf", "Kill Cooldown at start of round", 120f, "How soon after the start of a round a Werewolf can Kill someone (in seconds).");

            // Villager parameters
            config_VillagerInteractRange = Plugin.Instance.Config.Bind("Role: Villager", "PatPat Range", 1.5f, "Unused -- How far the player can use his Action on another player");
            config_VillagerActionCooldown = Plugin.Instance.Config.Bind("Role: Villager", "PatPat Cooldown", 0f, "Unused -- How often can a Villager use his action (in seconds).");
            config_VillagerStartOfRoundActionCooldown = Plugin.Instance.Config.Bind("Role: Villager", "PatPat Cooldown at start of round", 0f, "How soon after a round has started a Villager can PatPat another villager (in seconds).");

            // Witch parameters
            config_WitchInteractRange = Plugin.Instance.Config.Bind("Role: Witch", "Potion Range", 1.5f, "How far a Witch can use a potion on another.");
            config_WitchActionCooldown = Plugin.Instance.Config.Bind("Role: Witch", "Potion Cooldown", 9999f, "How often a Witch can use each potion (in seconds).");
            config_WitchStartOfRoundActionCooldown = Plugin.Instance.Config.Bind("Role: Witch", "Potion Cooldown at start of round", 120f, "How soon after a round has started a Witch can use her potions (in seconds).");

            // Seer parameters
            config_SeerInteractRange = Plugin.Instance.Config.Bind("Role: Seer", "Seer Range", 10f, "How far the Seer can seer another player role.");
            config_SeerActionCooldown = Plugin.Instance.Config.Bind("Role: Seer", "Seer Cooldown", 120f, "How often the Seer can seer another player role (in seconds).");
            config_SeerStartOfRoundActionCooldown = Plugin.Instance.Config.Bind("Role: Seer", "Seer Cooldown at start of round", 120f, "How soon after a round has started a Seer can seer a player role (in seconds).");

            // Wild Boy parameters
            config_WildBoyInteractRange = Plugin.Instance.Config.Bind("Role: Wild Boy", "Idolize Range", 30f, "How far the Wild Boy can idolize another player.");
            config_WildBoyActionCooldown = Plugin.Instance.Config.Bind("Role: Wild Boy", "Idolize Cooldown", 9999f, "How often the Wild Boy can idolize another player (in seconds).");
            config_WildBoyStartOfRoundActionCooldown = Plugin.Instance.Config.Bind("Role: Wild Boy", "Idolize Cooldown at start of round", 0f, "How soon after a round has started a WildBoy can idolize another player (in seconds).");
            config_WildBoyActionCooldownOnTransform = Plugin.Instance.Config.Bind("Role: Wild Boy", "Werewolf Kill Cooldown on transform", 30f, "Cooldown of the Werewolf Kill action after transformation.");

            // Cupid parameters
            config_CupidInteractRange = Plugin.Instance.Config.Bind("Role: Cupid", "Romance Range", 30f, "How far Cupid can make someone fall in love.");
            config_CupidActionCooldown = Plugin.Instance.Config.Bind("Role: Cupid", "Romance Cooldown", 9999f, "How often Cupid can create lovers (in seconds).");
            config_CupidStartOfRoundActionCooldown = Plugin.Instance.Config.Bind("Role: Cupid", "Romance Cooldown at start of round", 0f, "How soon after a round has started Cupid can create lovers (in seconds).");

            // Alpah Werewolf parameters
            config_AlphaWerewolfInteractRange = Plugin.Instance.Config.Bind("Role: Alpha Werewolf", "Transform Range", 1.5f, "How far the Alpha Werewolf can turn someone into a werewolf.");
            config_AlphaWerewolfActionCooldown = Plugin.Instance.Config.Bind("Role: Alpha Werewolf", "Transform Cooldown", 9999f, "How often the Alpha Werewolf can turn someone into a werewolf (in seconds).");
            config_AlphaWerewolfStartOfRoundActionCooldown = Plugin.Instance.Config.Bind("Role: Alpha Werewolf", "Transform Cooldown at start of round", 0f, "How soon after a round has started the Alpha Werewolf can create werewolves (in seconds).");
            config_AlphaWerewolfCooldownAfterTransform = Plugin.Instance.Config.Bind("Role: Alpha Werewolf", "Cooldown after transformation", 30f, "Cooldown of werewolves after transformation (in seconds).");

        }

        public static void ConfigLethalConfig()
        {
            foreach (var field in typeof(ConfigParameters).GetFields())
            {
                var type = field.GetType();
                BaseConfigItem configItem;
                // Check for Float
                if (field.FieldType == typeof(ConfigEntry<float>))
                {
                    var configEntry = (ConfigEntry<float>)(field.GetValue(null));
                    configEntry.SettingChanged += (obj, args) => OnSettingChangedPushChanges(obj, args);
                    configItem = new FloatInputFieldConfigItem(configEntry, requiresRestart: false);
                    Plugin.Instance.logdebug.LogInfo($"Adding {field.Name} of type : float");
                }

                // Check for bool
                else if (field.FieldType == typeof(ConfigEntry<bool>))
                {
                    var configEntry = (ConfigEntry<bool>)(field.GetValue(null));
                    configEntry.SettingChanged += (obj, args) => OnSettingChangedPushChanges(obj, args);
                    configItem = new BoolCheckBoxConfigItem(configEntry, requiresRestart: false);
                    Plugin.Instance.logdebug.LogInfo($"Adding {field.Name} of type : bool");
                }

                // Check for int
                else if (field.FieldType == typeof(ConfigEntry<int>))
                {
                    var configEntry = (ConfigEntry<int>)(field.GetValue(null));
                    configEntry.SettingChanged += (obj, args) => OnSettingChangedPushChanges(obj, args);
                    configItem = new IntInputFieldConfigItem(configEntry, requiresRestart: false);
                    Plugin.Instance.logdebug.LogInfo($"Adding {field.Name} of type : int");
                }
                else
                {
                    continue;
                }

                LethalConfigManager.AddConfigItem(configItem);
            }
        }

        public static void OnSettingChangedPushChanges(object sender, EventArgs e)
        {
            Plugin.Instance.logdebug.LogInfo($"Parameters changed: sender = {sender}, event = {e}");
            Plugin.Instance.configManager.UpdateConfigValues();
        }
    }
}
