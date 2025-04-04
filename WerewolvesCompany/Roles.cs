﻿using WerewolvesCompany.Managers;
using UnityEngine;
using BepInEx.Logging;
using System.Collections.Generic;
using System;
using WerewolvesCompany.Config;



namespace WerewolvesCompany
{
    class References
    {
        public static List<Role> GetAllRoles()
        {
            var roles = new List<Role>();

            roles.Add(new Werewolf());      // 0
            roles.Add(new Villager());      // 1
            roles.Add(new Witch());         // 2
            roles.Add(new Seer());          // 3
            roles.Add(new WildBoy());       // 4
            roles.Add(new Cupid());         // 5
            roles.Add(new Minion());        // 6
            roles.Add(new DrunkenMan());    // 7
            roles.Add(new AlphaWerewolf()); // 8
            roles.Add(new FakeSeer());      // 9

            return roles;
        }

        // Checks whether I have duplicates in the refInts for each role.
        public static void CheckIndividualRefInt()
        {

            bool flag = true;
            List<Role> roles = GetAllRoles();
            List<int> individualRefInts = new List<int>();
            foreach (Role role in roles)
            {
                if (individualRefInts.Contains(role.refInt))
                {
                    flag = false;
                    break;
                }
                individualRefInts.Add(role.refInt);
            }
            
            if (!flag)
            {
                throw new Exception("There are duplicates RefInts within the roles");
            }
        }

        public static Dictionary<int, Role> references()
        {
            List<Role> roles = GetAllRoles();
            Dictionary<int, Role> dic = new Dictionary<int, Role>();
            foreach (Role role in roles)
            {
                dic.Add(role.refInt, role);
            }
            return dic;
        }

        public static Role GetRoleByName(string roleName)
        {
            foreach (var entry in references())
            {
                Role role = entry.Value;
                if (role.roleName.ToLower() == roleName.ToLower())
                {
                    return role;
                }
            }

            foreach (var entry in references())
            {
                Role role = entry.Value;
                if (role.terminalName.ToLower() == roleName.ToLower())
                {
                    return role;
                }
            }

            throw new Exception("No corresponding role found. This should have been caught earlier.");
            
        }
    }


    // Interactions with other roles
    class RolesInteractions
    {

        public bool isImmune = false;
        public ulong? isInLoveWith = null;

        public RolesInteractions()
        {

        }
    }



    class Role
    {
        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public RolesManager rolesManager => Plugin.Instance.rolesManager;
        public ConfigManager configManager => Plugin.Instance.configManager;

        public virtual string roleName { get; set; } = "Default Role Name";
#nullable enable
        public virtual string? roleNameColor { get; set; }  = null;
#nullable disable
        public string roleNameColored => GetRoleNameColored();
        public virtual string terminalName => roleName.Replace(" ", "_");
        public string terminalNameColored => GetTerminalRoleNameColored();
        public virtual int refInt { get; set; } = -1;
        public virtual string team { get; set; } = "NoTeam";
        public virtual string winCondition { get; set; } = "Role Win Condition";
        public virtual string roleShortDescription { get; set; } = "Role Short Description";
        public virtual string roleDescription { get; set; } = "Role Description";
        public virtual string rolePopUp  => $"{winCondition} {roleShortDescription}";
        public virtual Sprite roleIcon => null; // Default icon (null if none)

        


        // Who's in range
        public ulong? targetInRangeId { get; set; }
#nullable enable
        public string? targetInRangeName { get; set; }
#nullable disable


        // ToolTip

        public virtual string mainActionKey { get; set; } = "Z";
        public virtual string secondaryActionKey { get; set; } = "V";
        public virtual string mainActionName { get; set; } = "Main Action Name";
        public virtual string secondaryActionName { get; set; } = "Secondary Action Name";
        
        public bool hasMainAction => (!mainActionName.Contains("Main Action Name"));
        public bool hasSecondaryAction => (!secondaryActionName.Contains("Secondary Action Name"));
        public virtual string mainActionText => $"{mainActionName} {targetInRangeName}";
        //public virtual string mainActionTooltip { get { return $"[{mainActionKey}] {mainActionText} {GetCurrentMainActionCooldownText()}".Trim(); } }
        public virtual string mainActionTooltip => GetMainActionTooltip();

        public virtual string secondaryActionText => $"{secondaryActionName} {targetInRangeName}";
        //public virtual string secondaryActionTooltip { get { return $"[{secondaryActionKey}] {secondaryActionText} {GetCurrentSecondaryActionCooldownText()}".Trim(); } }
        public virtual string secondaryActionTooltip => GetSecondaryActionTooltip();
        public virtual string roleActionText
{
            get { return GetRoleActionText(); } 
            set { roleActionText = value; }
        }





        // Settings
        public virtual float interactRange => 0f;
        public virtual float baseActionCooldown => 0f;
        public virtual float startOfRoundActionCooldown => 0f;


        // Cooldowns
        public virtual float currentMainActionCooldown { get; set; }
        public virtual float currentSecondaryActionCooldown { get; set; }

        public virtual float baseMainActionCooldown { get; set; }
        public virtual float baseSecondaryActionCooldown { get; set; }

        public bool IsMainActionOnCooldown => (currentMainActionCooldown > 0);
        public bool IsSecondaryActionOnCooldown => (currentSecondaryActionCooldown > 0);


        // Interactions with others roles
        //public bool isImmune = false;
        public RolesInteractions interactions = new RolesInteractions();

        public Role()
        {
            InitiateCooldowns();
        }
        
        public virtual void InitiateCooldowns()
        {
            baseMainActionCooldown = baseActionCooldown;
            baseSecondaryActionCooldown = baseActionCooldown;

            currentMainActionCooldown = startOfRoundActionCooldown;
            currentSecondaryActionCooldown = startOfRoundActionCooldown;
        }

        public string GetRoleActionText()
        {
            string outMainTooltip;
            string outSecondaryTooltip;

            // If the role has no main action

            if (!hasMainAction) { outMainTooltip = ""; }
            else { outMainTooltip = mainActionTooltip; }

            // If the role has no secondary
            if (!hasSecondaryAction) { outSecondaryTooltip = ""; }
            else { outSecondaryTooltip = secondaryActionTooltip; }

            string outString = $"{outMainTooltip}\n{outSecondaryTooltip}".Trim('\n');

            return outString;


        }

        public string GetRoleNameColored()
        {
            if (roleNameColor == null)
            {
                return roleName;
            }
            return $"<color={roleNameColor}>{roleName}</color>";
        }

        public string GetTerminalRoleNameColored()
        {
            if (roleNameColor == null)
            {
                return terminalName;
            }
            return $"<color={roleNameColor}>{terminalName}</color>";
        }

        public void DisplayRolePopUp()
        {
            logdebug.LogInfo("Display the role PopUp");
            HUDManager.Instance.DisplayTip(roleNameColored, rolePopUp);
        }

        public virtual bool IsLocallyAllowedToPerformMainAction()
        {
            return (!IsMainActionOnCooldown && !(targetInRangeId == null) && IsLocallyAllowedToPerformMainActionRoleSpecific());
        }

        public virtual bool IsLocallyAllowedToPerformSecondaryAction()
        {
            return (!IsSecondaryActionOnCooldown && !(targetInRangeId == null) && IsLocallyAllowedToPerformMainActionRoleSpecific());
        }

        public virtual bool IsLocallyAllowedToPerformMainActionRoleSpecific()
        {
            return true;
        }

        public virtual bool IsLocallyAllowedToPerformSecondaryActionRoleSpecific()
        {
            return true;
        }


        //public virtual bool IsAllowedToPerformMainAction()
        //{
        //    return true;
        //}

        //public virtual bool IsAllowedToPerformSecondaryAction()
        //{
        //    return true;
        //}

        public void GenericPerformMainAction()
        {

            bool flag = false;
            try
            {
                PerformMainAction();
                flag = true;
            }
            catch (Exception e)
            {
                HUDManager.Instance.DisplayTip("<color=red>Error</color>", "Failed to perform my Main Action");
                logger.LogError("Failed to perform my role action");
                logger.LogError(e);
            }

            if (flag)
            {
                rolesManager.SuccessFullyPerformedMainActionServerRpc();
            }

        }


        public void GenericPerformSecondaryAction()
        {

            bool flag = false;
            try
            {
                PerformSecondaryAction();
                flag = true;
            }
            catch (Exception e)
            {
                HUDManager.Instance.DisplayTip("<color=red>Error</color>", "Failed to perform my Secondary Action");
                logger.LogError("Failed to perform my secondary action");
                logger.LogError(e);
            }

            if (flag)
            {
                rolesManager.SuccessFullyPerformedSecondaryActionServerRpc();
            }

        }



        public virtual void PerformMainAction()
        {
            // Default behavior for a role

            logger.LogInfo($"{roleName} has no main action.");
        }

        public virtual void PerformSecondaryAction()
        {
            // Default behavior for a role

            logger.LogInfo($"{roleName} has no secondary action.");
        }



        public virtual void SetMainActionOnCooldown()
        {
            currentMainActionCooldown = baseMainActionCooldown;
        }

        public virtual void SetSecondaryActionOnCooldown()
        {
            logdebug.LogInfo("Setting my secondary action on cooldown");
            currentSecondaryActionCooldown = baseSecondaryActionCooldown;
        }


        public void UpdateCooldowns(float deltaTime)
        {
            currentMainActionCooldown -= deltaTime;
            currentSecondaryActionCooldown -= deltaTime;
        }
        

        //
        private string GetCurrentMainActionCooldownText()
        {
            if (currentMainActionCooldown <= 0)
            {
                return "";
            }
            return $"({(int)currentMainActionCooldown}s)";
        }

        private string GetCurrentSecondaryActionCooldownText()
        {
            if (currentSecondaryActionCooldown <= 0)
            {
                return "";
            }
            return $"({(int)currentSecondaryActionCooldown}s)";
        }
        //

        private string GetMainActionTooltip()
        {
            if (currentMainActionCooldown < 5000)
            {
                return $"[{mainActionKey}] {mainActionText} {GetCurrentMainActionCooldownText()}".Trim();
            }
            else
            {
                return "";
            }
        }

        private string GetSecondaryActionTooltip()
        {
            if (currentSecondaryActionCooldown < 5000)
            {
                return $"[{secondaryActionKey}] {secondaryActionText} {GetCurrentSecondaryActionCooldownText()}".Trim();
            }
            else
            {
                return "";
            }
        }



        // ------ Main action success
        // Alternative parameters inputs for the Seer
        public virtual void NotifyMainActionSuccess(string targetPlayerName, Role role)
        {
            return;
        }
        // Alternative parameters inputs for the Wild Boy
        public virtual void NotifyMainActionSuccess(ulong targetId)
        {
            return;
        }


        // ------ Secondary action success
        public virtual void NotifySecondaryActionSuccess(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleNameColored, "Secondary action success");
        }
        public virtual void NotifySecondaryActionSuccess(ulong targetId)
        {
            HUDManager.Instance.DisplayTip(roleNameColored, "Secondary action success");
        }
        public virtual void NotifySecondaryActionSuccess()
        {
            HUDManager.Instance.DisplayTip(roleNameColored, "Secondary action success");
        }


        // ------ Main action failed
        public virtual void NotifyMainActionFailed(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleNameColored, "Main action failed");
        }


        // ------ Secondary action failed
        public virtual void NotifySecondaryActionFailed(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleNameColored, "Secondary action failed");
        }



        public ulong GrabTargetPlayer()
        {
            if (targetInRangeId == null)
            {
                logger.LogError("targetInRange is null. It should have been caught earlier. Halting execution.");
                throw new Exception("targetInRange should not be null at this point.");
            }
            return targetInRangeId.Value;
        }

        

    }

    // ----------------------------------------
    // Roles


    // Role: Werewolf
    class Werewolf : Role
    {
        public override string roleName { get; set; } = "Werewolf";
        public override int refInt { get; set; }  = 0;
        public override string team => "Werewolves";
        public override string roleNameColor { get; set; } = "red";
        public override string winCondition { get; set; } = "You win by killing all Villagers";
        public override string roleShortDescription { get; set; } = "You have the ability to kill other players";
        public override string mainActionName { get; set; } = "Kill";
        public override string roleDescription { get; set; } = "The Werewolves shall kill other players before ship departure.\nThe Werewolf has the ability to kill another player.";

        // Parameters
        public override float interactRange => configManager.WerewolfInteractRange.Value;
        public override float baseActionCooldown => configManager.WerewolfActionCooldown.Value;
        public override float startOfRoundActionCooldown => configManager.WerewolfStartOfRoundActionCooldown.Value;



        public Werewolf() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is hunting!");
            ulong targetId = GrabTargetPlayer();
            
            logger.LogInfo($"Killing {rolesManager.GetPlayerById(targetId).playerUsername}.");
            rolesManager.WerewolfKillPlayerServerRpc(targetId);
        }

        public override void NotifyMainActionSuccess(ulong targetId)
        {
            string targetPlayerName = rolesManager.GetPlayerById(targetId).playerUsername;

            logger.LogInfo($"Successfully killed {targetPlayerName}.");
            HUDManager.Instance.DisplayTip(roleNameColored, $"You killed {targetPlayerName}.");
        }

        public override void NotifyMainActionFailed(string targetPlayerName)
        {
            logger.LogInfo($"Failed to kill {targetPlayerName}, he was immune");
            HUDManager.Instance.DisplayTip(roleNameColored, $"{targetPlayerName} was immune");
        }
    }



    // Role: Villager
    class Villager : Role
    {
        public override string roleName { get; set; } = "Villager";
        public override int refInt { get; set; } = 1;
        public override string team => "Village";
        public override string winCondition { get; set; } = "You win by killing the Werewolves.";
        public override string roleShortDescription { get; set; } = "You do not have any special ability.";
        public override string roleActionText { get; set; } = "";
        public override string mainActionName { get; set; } = "patpat";
        public override string roleDescription { get; set; } = "The Villager shall find and kill the Werewolves before ship departure.\nThe Villager can patpat others players.";

        // Parameters
        public override float interactRange => configManager.VillagerInteractRange.Value;
        public override float baseActionCooldown => configManager.VillagerActionCooldown.Value;
        public override float startOfRoundActionCooldown => configManager.VillagerStartOfRoundActionCooldown.Value;
        
        public Villager() : base() { }

        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is staying safe.");
            HUDManager.Instance.DisplayTip(roleNameColored, "*pat pat*");
        }
    }



    // Role: Witch
    class Witch : Role
    {
        public override string roleName { get; set; } = "Witch";
        public override int refInt { get; set; } = 2;
        public override string team => "Village";
        public override string winCondition { get; set; } = "You win by killing the Werewolves.";
        public override string roleShortDescription { get; set; } = "You have the ability to protect one player, and kill another one.";
        public override string mainActionName { get; set; } = "Poison";
        public override string secondaryActionName { get; set; } = "Protect";
        public override string roleDescription { get; set; } = "The Witch is part of the village. She shall find and kill the Werewolves before ship departure.\nThe Witch has two potions, and can do two things:\n- Poison another player and kill him (once per round)\n- Protect another player and make him immune once to a Werewolf attack (once per round). The immune player won't know they has been immunized, nor will they know they loses his immune status. The Witch cannot protect herself.";

        // Parameters
        public override float interactRange => configManager.WitchInteractRange.Value;
        public override float baseActionCooldown => configManager.WitchActionCooldown.Value;
        public override float startOfRoundActionCooldown => configManager.WitchStartOfRoundActionCooldown.Value;

        public Witch() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is poisoning someone.");
            ulong targetId = GrabTargetPlayer();

            rolesManager.WitchPoisonPlayerServerRpc(targetId);

        }


        public override void PerformSecondaryAction()
        {
            logger.LogInfo($"The {roleName} is immunising someone.");
            ulong targetId = GrabTargetPlayer();

            rolesManager.WitchImmunizePlayerServerRpc(targetId);
        }

        public override void NotifyMainActionSuccess(ulong targetId)
        {
            string targetPlayerName = rolesManager.GetPlayerById(targetId).playerUsername;

            logger.LogInfo($"Successfully poisoned {targetPlayerName}.");
            HUDManager.Instance.DisplayTip(roleNameColored, $"You poisoned {targetPlayerName}.");
        }

        public override void NotifyMainActionFailed(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleNameColored, $"{targetPlayerName} is so drunk that your poison does not seem that have any effect on him.");
        }

        public override void NotifySecondaryActionSuccess(string targetPlayerName)
        {
            logger.LogInfo($"Successfully immunized {targetPlayerName}.");
            HUDManager.Instance.DisplayTip(roleNameColored, $"You immunized {targetPlayerName}.");
        }
    }



    // Role: Seer
    class Seer : Role
    {
        public override string roleName { get; set; } = "Seer";
        public override int refInt { get; set; } = 3;
        public override string team => "Village";
        public override string winCondition { get; set; } = "You win by killing the Werewolves.";
        public override string roleShortDescription { get; set; } = "You have the ability to see a player's role.";
        public override string mainActionName { get; set; } = "Seer role";
        public override string mainActionText => $"Seer {targetInRangeName}'s role";
        public override string roleDescription { get; set; } = "The Seer is part of the village. She shall find and kill the Werewolves before ship departure.\nThe Seer can seer another player's role.";

        // Parameters
        public override float interactRange => configManager.SeerInteractRange.Value;
        public override float baseActionCooldown => configManager.SeerActionCooldown.Value;
        public override float startOfRoundActionCooldown => configManager.SeerStartOfRoundActionCooldown.Value;

        public bool isFakeSeer = false;

        public Seer() : base() { }

        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is omniscient.");
            logger.LogInfo("Looking the role of someone");

            ulong targetId = GrabTargetPlayer();
            rolesManager.CheckRoleServerRpc(targetId);
        }


        public override void NotifyMainActionSuccess(string targetPlayerName, Role role)
        {
            logdebug.LogInfo("Displaying Checked role on HUD");

            Role displayedRole;
            if (isFakeSeer)
            {
                logdebug.LogInfo("Is a Fake Seer");
                // 50% chance to see to true role
                int c = rolesManager.rng.Next(2);
                if (c == 0)
                {
                    displayedRole = role;
                }
                else
                {
                    // Randomizes the displayed role
                    List<Role> rolesList = rolesManager.currentRolesSetup;
                    int k = rolesManager.rng.Next(rolesList.Count);
                    displayedRole = rolesList[k];
                }
            }
            else
            {
                logdebug.LogInfo("Is not a Fake Seer");
                displayedRole = role;
            }

            HUDManager.Instance.DisplayTip(roleNameColored, $"{targetPlayerName} is a {displayedRole.roleNameColored}");

        }
    }

    class FakeSeer : Seer
    {
        public override int refInt { get; set; } = 9;
        public override string terminalName => "Fake_Seer";

        public FakeSeer() : base()
        {
            isFakeSeer = true;
        }
    }

    // Role: Wild Boy
    class WildBoy : Role
    {
        public override string roleName { get; set; } = "Wild Boy";
        public override int refInt { get; set; } = 4;
        public override string team => "Village";
        public override string winCondition { get; set; } = "For now, you win with the village.";
        public string _roleShortDescription = "You can idolize a player. If they dies, you become a werewolf.";
        public override string roleShortDescription
        {
            get { return _roleShortDescription; }
            set { _roleShortDescription = value; }
        }
        public override string roleDescription { get; set; } = "The Wild Boy wins either with the Villagers or the Werewolves, depending on his status.\nThe Wild Boy can target a player who becomes his idol. If his idol dies, the Wild Boy becomes a Werewolf. As long as his idol is alive, he wins with the Villagers.";
        public override string mainActionName { get; set; } = "Idolize";
        public ulong? idolizedId;

        // Parameters
        public override float interactRange => configManager.WildBoyInteractRange.Value;
        public override float baseActionCooldown => configManager.WildBoyActionCooldown.Value;
        public override float startOfRoundActionCooldown => configManager.WildBoyStartOfRoundActionCooldown.Value;


        public WildBoy() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is loitering.");
            logger.LogInfo("Idolizing someone");

            ulong targetId = GrabTargetPlayer();
            rolesManager.IdolizeServerRpc(targetId);
        }

        public override void NotifyMainActionSuccess(ulong targetId)
        {
            logdebug.LogInfo("I am running the Idolization Confirmation");
            idolizedId = targetId;

            logdebug.LogInfo("I have set my idolization mentor");
            string playerName = rolesManager.GetPlayerById(targetId).playerUsername;

            roleShortDescription = $"You have idolized {playerName}. If they die, you become a werewolf.";
            logdebug.LogInfo("Displaying Idolization on HUD");
            HUDManager.Instance.DisplayTip(roleNameColored, $"You have idolized {playerName}. If they die, you will become a werewolf.");
        }


        public void BecomeWerewolf()
        {
            HUDManager.Instance.DisplayTip(roleNameColored, $"{rolesManager.GetPlayerById(idolizedId.Value).playerUsername} is dead. You have become a <color=red>Werewolf</color>.");

            rolesManager.BecomeRole("Werewolf", true);
            
            // Set role cd to 30s
            rolesManager.myRole.currentMainActionCooldown = configManager.WildBoyActionCooldownOnTransform.Value;

            // Update the roles list to all other clients
            rolesManager.QueryAllRolesServerRpc(sendToAllPlayers: true);
        }
    }



    // Role: Cupid
    class Cupid : Role
    {
        public override string roleName { get; set; } = "Cupid";
        public override int refInt { get; set; } = 5;
        public override string team => "Village";
        public override string winCondition { get; set; }  = "You win with the village.";
        public override string roleShortDescription { get; set; }  = "You can make two players fall in love. They must win together. They also die together.";
        public override string roleDescription { get; set; } = "Cupid is able to make two players fall deeply in love.\nWhen one lover die, the other one also dies.\nThe lovers win together. If they're part of the same team, they win with their team. If they're part of different teams, they must be the only two survivors.";

        public override string mainActionName { get; set; } = "Romance";
        public override string secondaryActionName { get; set; } = "Romance Myself";
        public override string secondaryActionText => secondaryActionName;

        public override float currentSecondaryActionCooldown => currentMainActionCooldown;
        public override float baseSecondaryActionCooldown => baseMainActionCooldown;


        public List<ulong> lovers = new List<ulong>();
        //public ulong? firstRomancedId;
        //public ulong? secondRomancedId;

        public bool TargetIsAlreadyRomanced => lovers.Contains(targetInRangeId.Value);
        public bool AmIAlreadyRomanced => lovers.Contains(Utils.GetLocalPlayerControllerB().OwnerClientId);
        public bool AreBothTargetsRomanced => (lovers.Count == 2);

        public int romancedPlayersCallbackAmount = 0;

        // Parameters
        public override float interactRange => configManager.CupidInteractRange.Value;
        public override float baseActionCooldown => configManager.CupidActionCooldown.Value;
        public override float startOfRoundActionCooldown => configManager.CupidStartOfRoundActionCooldown.Value;
        
        
        public Cupid() : base() { }

        public override void InitiateCooldowns()
        {
            baseMainActionCooldown = baseActionCooldown;
            //baseSecondaryActionCooldown = baseActionCooldown.Value;

            currentMainActionCooldown = startOfRoundActionCooldown;
            //currentSecondaryActionCooldown = startOfRoundActionCooldown.Value;
        }

        // Cupid really only has one action: romancing someone. The "secondary" action only serves as the main action targetting himself
        public override void SetSecondaryActionOnCooldown()
        {
            SetMainActionOnCooldown();
        }

        public override void SetMainActionOnCooldown()
        {
            logdebug.LogInfo("Trying to set main action on cooldown");
            // Only set action on cooldown if two targets have been romanced
            if (lovers.Count == 2)
            {
                logdebug.LogInfo("Success");
                currentMainActionCooldown = baseMainActionCooldown;
            }
        }


        public override void PerformMainAction()
        {
            logger.LogInfo($"{roleName} is romancing a target: {targetInRangeName}.");
            rolesManager.CupidRomancePlayerServerRpc(targetInRangeId.Value);
        }

        public override void PerformSecondaryAction()
        {
            logger.LogInfo($"{roleName} is romancing himself.");
            rolesManager.CupidRomancePlayerServerRpc(Utils.GetLocalPlayerControllerB().OwnerClientId);
        }
        

        public override void NotifyMainActionSuccess(ulong targetId)
        {
            NotifyRomancingSuccess(targetId);
        }

        public override void NotifySecondaryActionSuccess(ulong targetId)
        {
            NotifyRomancingSuccess(targetId);
        }

        private void NotifyRomancingSuccess(ulong targetId)
        {
            logdebug.LogInfo("I am running the Romancing confirmation Confirmation");
            lovers.Add(targetId);

            logdebug.LogInfo("Displaying romancing status on HUD");
            if (!AreBothTargetsRomanced)
            {
                logdebug.LogInfo("Only one target is romanced. Notifying Cupid");
                HUDManager.Instance.DisplayTip(roleNameColored, $"<color=#ff00ffff>{rolesManager.GetPlayerById(targetId).playerUsername}</color> will be romanced.");
                
            }
            else // If both targets are now romanced, we have a couple. Send them that they are now romanced.
            {
                logdebug.LogInfo("Both targets are romanced. Sending the lovers their respective lover.");
                romancedPlayersCallbackAmount = 0;
                rolesManager.CupidSendLoversTheirLoverServerRpc(lovers[0], lovers[1]);
            }
        }

        public void CheckForCallBackOfLover()
        {
            logdebug.LogInfo($"Received a callback from a lover. Current value: {romancedPlayersCallbackAmount}");
            romancedPlayersCallbackAmount += 1;
            if (romancedPlayersCallbackAmount == 2)
            {
                logdebug.LogInfo("Received 2 callbacks from lovers, notifying to Cupid.");
                string player1Name = rolesManager.GetPlayerById(lovers[0]).playerUsername;
                string player2Name = rolesManager.GetPlayerById(lovers[1]).playerUsername;
                roleShortDescription = $"<color=#ff00ffff>{player1Name}</color> and <color=#ff00ffff>{player2Name}</color> are deeply in love. They will die together. They must win together.";
                if (!lovers.Contains(Utils.GetLocalPlayerControllerB().OwnerClientId))
                {
                    DisplayRolePopUp();
                }

                logger.LogInfo($"Successfully romanced {player1Name} and {player2Name}");

                SetMainActionOnCooldown();
            }
        }

        

        public override bool IsLocallyAllowedToPerformMainActionRoleSpecific()
        {
            if (TargetIsAlreadyRomanced)
            {
                HUDManager.Instance.DisplayTip(roleNameColored, $"{targetInRangeName} is already romanced.");
                return false;
            }
            return true;
        }

        public override bool IsLocallyAllowedToPerformSecondaryActionRoleSpecific()
        {
            if (AreBothTargetsRomanced)
            {
                return false;
            }
            {
                
            }
            if (AmIAlreadyRomanced)
            {
                HUDManager.Instance.DisplayTip(roleNameColored, $"You are already romanced.");
                return false;
            }
            return true;
        }

        public override bool IsLocallyAllowedToPerformSecondaryAction()
        {
            return (!IsSecondaryActionOnCooldown && IsLocallyAllowedToPerformSecondaryActionRoleSpecific());
        }
    }


    class Minion : Role
    {
        public override string roleName { get; set; } = "Minion";
        public override int refInt { get; set; } = 6;
        public override string team => "Werewolves";
        public override string roleNameColor { get; set; } = "red";
        public override string winCondition { get; set; } = "You win with the werewolves.";
        public override string roleShortDescription { get; set; } = "You can see the werewolves, they cannot see you.";
        public override string roleDescription { get; set; } = "The minion is part of the werewolves team. He is able to see who the werewolves are, but they cannot see him.";


        public Minion() : base() { }

    }


    // Drunken Man
    class DrunkenMan : Role
    {
        public override string roleName { get; set; } = "Drunken Man";
        public override int refInt { get; set; } = 7;
        public override string team => "Village";
        public override string winCondition { get; set; } = "You win with the village.";
        public override string roleShortDescription { get; set; } = "You've been drinking so much that you are immune to the Witch poison.";
        public override string roleDescription { get; set; } = "The Drunken Man has spent too much time at the local tavern. He has become immune to all kinds of poison, making him immune to the Witch poison.";


        public DrunkenMan() : base() { }

        public void NotifyOldLadyStrongBeverage(ulong witchId)
        {
            string witchName = rolesManager.GetPlayerById(witchId).playerUsername;
            HUDManager.Instance.DisplayTip(roleNameColored, $"{witchName} has provided you with a really strong beverage. You drank worse than that.");
        }

    }



    // Alpha Werewolf
    class AlphaWerewolf : Role
    {
        public override string roleName { get; set; } = "Alpha Werewolf";
        public override int refInt { get; set; } = 8;
        public override string team => "Werewolves";
        public override string roleNameColor { get; set; } = "red";
        public override string winCondition { get; set; } = "You win with the werewolves.";
        public override string mainActionName { get; set; } = "Transform";
        public override string roleShortDescription { get; set; } = "You can transform players into werewolves.";
        public override string roleDescription { get; set; } = "The Alpha Werewolf can turn players into werewolves.";


        public override float interactRange => configManager.AlphaWerewolfInteractRange.Value;
        public override float baseActionCooldown => configManager.AlphaWerewolfActionCooldown.Value;
        public override float startOfRoundActionCooldown => configManager.AlphaWerewolfStartOfRoundActionCooldown.Value;



        public AlphaWerewolf() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is a big boss.");
            logger.LogInfo("Turning someone into a werewolf");

            ulong targetId = GrabTargetPlayer();
            rolesManager.AlphaWerewolfTransformToWerewolfServerRpc(targetId);
        }

        public override void NotifyMainActionSuccess(ulong targetId)
        {
            logdebug.LogInfo("I am running the Werewolf Transformation Confirmation");
            string playerName = rolesManager.GetPlayerById(targetId).playerUsername;
            HUDManager.Instance.DisplayTip(roleNameColored, $"You have successfuly turned {playerName} into a <color=red>Werewolf</color>.");
        }


    }


}



