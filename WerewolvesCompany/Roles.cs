
using BepInEx;
using HarmonyLib;
using WerewolvesCompany.Managers;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using HarmonyLib.Tools;
using System.Collections.Generic;
using UnityEngine.Jobs;
using UnityEngine.Windows;
using GameNetcodeStuff;
using BepInEx.Configuration;
using System;
using static UnityEngine.GraphicsBuffer;
using System.Data;
using JetBrains.Annotations;
using Unity.IO.LowLevel.Unsafe;
using Unity.Netcode;
using System.Drawing;



namespace WerewolvesCompany
{
    class References
    {
        public static List<Role> GetAllRoles()
        {
            var roles = new List<Role>();

            roles.Add(new Werewolf());
            roles.Add(new Villager());
            roles.Add(new Witch());
            roles.Add(new Seer());
            roles.Add(new WildBoy());
            roles.Add(new Cupid());

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

    class Role
    {
        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public RolesManager rolesManager = Utils.GetRolesManager();

        public virtual string roleName { get; }
#nullable enable
        public virtual string? roleNameColor => null;
#nullable disable
        public string roleNameColored => GetRoleNameColored();
        public string terminalName => roleName.Replace(" ", "_");
        public virtual int refInt { get; }
        public virtual string winCondition { get; }
        public virtual string roleShortDescription { get; set; }
        public virtual string roleDescription { get; set; }
        public virtual string rolePopUp  => $"{winCondition} {roleShortDescription}";
        public virtual Sprite roleIcon => null; // Default icon (null if none)

        

        // Who's in range
        public ulong? targetInRangeId { get; set; }
#nullable enable
        public string? targetInRangeName { get; set; }
#nullable disable


        // ToolTip
        
        public virtual string mainActionKey { get { return "Z"; } }
        public virtual string secondaryActionKey { get { return "V"; } }
        public virtual string mainActionName { get { return "Main Action Name"; } }
        public virtual string secondaryActionName { get { return "Secondary Action Name"; } }

        public virtual string mainActionText { get { return $"{mainActionName} {targetInRangeName}"; } }
        //public virtual string mainActionTooltip { get { return $"[{mainActionKey}] {mainActionText} {GetCurrentMainActionCooldownText()}".Trim(); } }
        public virtual string mainActionTooltip => GetMainActionTooltip();

        public virtual string secondaryActionText => $"{secondaryActionName} {targetInRangeName}";
        //public virtual string secondaryActionTooltip { get { return $"[{secondaryActionKey}] {secondaryActionText} {GetCurrentSecondaryActionCooldownText()}".Trim(); } }
        public virtual string secondaryActionTooltip => GetSecondaryActionTooltip();
        public virtual string roleActionText { get { return GetRoleActionText(); } }



        // Settings
        public virtual NetworkVariable<float> interactRange => rolesManager.DefaultInteractRange;
        public virtual NetworkVariable<float> baseActionCooldown => rolesManager.DefaultActionCoolDown;
        public virtual NetworkVariable<float> startOfRoundActionCooldown => rolesManager.DefaultStartOfRoundActionCoolDown;


        // Cooldowns
        public float currentMainActionCooldown;
        public float currentSecondaryActionCooldown;

        public float baseMainActionCooldown;
        public float baseSecondaryActionCooldown;

        public bool IsMainActionOnCooldown => (currentMainActionCooldown > 0);
        public bool IsSecondaryActionOnCooldown => (currentSecondaryActionCooldown > 0);


        // Interactions with others roles
        public bool isImmune = false;

        
        public Role()
        {
            baseMainActionCooldown = baseActionCooldown.Value;
            baseSecondaryActionCooldown = baseActionCooldown.Value;

            currentMainActionCooldown = startOfRoundActionCooldown.Value;
            currentSecondaryActionCooldown = startOfRoundActionCooldown.Value;
        }
        
        

        public string GetRoleActionText()
        {
            if (secondaryActionText.Contains("Secondary Action Name"))
            {
                return mainActionTooltip;
            }
            return $"{mainActionTooltip}\n{secondaryActionTooltip}";
        }

        public string GetRoleNameColored()
        {
            if (roleNameColor == null)
            {
                return roleName;
            }
            return $"<color={roleNameColor}>{roleName}</color>";
        }

        public void DisplayRolePopUp()
        {
            logdebug.LogInfo("Display the role PopUp");
            HUDManager.Instance.DisplayTip($"{roleName}", rolePopUp); 
            //HUDManager.Instance.DisplayTip($"Test <color=red>red</color>", "Test <color=blue>blue</color>");
        }

        public bool IsLocallyAllowedToPerformMainAction()
        {
            return (!IsMainActionOnCooldown && !(targetInRangeId == null) && IsLocallyAllowedToPerformMainActionRoleSpecific());
        }

        public bool IsLocallyAllowedToPerformSecondaryAction()
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


        public virtual bool IsAllowedToPerformMainAction()
        {
            return true;
        }

        public virtual bool IsAllowedToPerformSecondaryAction()
        {
            return true;
        }

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
                HUDManager.Instance.DisplayTip("Error", "Failed to perform my Main Action");
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
                HUDManager.Instance.DisplayTip("Error", "Failed to perform my Secondary Action");
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
        public virtual void NotifyMainActionSuccess(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleName, "Main action success");
        }
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
            HUDManager.Instance.DisplayTip(roleName, "Secondary action success");
        }
        public virtual void NotifySecondaryActionSuccess(ulong targetId)
        {
            HUDManager.Instance.DisplayTip(roleName, "Secondary action success");
        }


        // ------ Main action failed
        public virtual void NotifyMainActionFailed(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleName, "Main action failed");
        }


        // ------ Secondary action failed
        public virtual void NotifySecondaryActionFailed(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleName, "Secondary action failed");
        }



        public ulong GrabTargetPlayer()
        {
            if (targetInRangeId == null)
            {
                logger.LogError("targetInRange is null. It should have been caught earlier. Halting execution");
                throw new Exception("targetInRange should not be null at this point");
            }
            return targetInRangeId.Value;
        }
    }

    // ----------------------------------------
    // Roles


    // Role: Werewolf
    class Werewolf : Role
    {
        public override string roleName => "Werewolf";
        public override int refInt => 0;
        public override string roleNameColor => "red";
        public override string winCondition => "You win by killing all Villagers";
        public override string roleShortDescription => "You have the ability to kill other players";
        public override string mainActionName => "Kill";
        public override string roleDescription => "The Werewolves shall kill other players before ship departure.\nThe Werewolf has the ability to kill another player.";

        // Parameters
        public override NetworkVariable<float> interactRange => rolesManager.WerewolfInteractRange;
        public override NetworkVariable<float> baseActionCooldown => rolesManager.WerewolfActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => rolesManager.WerewolfStartOfRoundActionCoolDown;



        public Werewolf() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is hunting!");
            ulong targetId = GrabTargetPlayer();
            
            logger.LogInfo($"Killing {rolesManager.GetPlayerById(targetId).playerUsername}.");
            rolesManager.WerewolfKillPlayerServerRpc(targetId);
        }

        public override void NotifyMainActionSuccess(string targetPlayerName)
        {
            logger.LogInfo($"Successfully killed {targetPlayerName}.");
            HUDManager.Instance.DisplayTip($"{roleName}", $"You killed {targetPlayerName}.");
        }

        public override void NotifyMainActionFailed(string targetPlayerName)
        {
            logger.LogInfo($"Failed to kill {targetPlayerName}, he was immune");
            HUDManager.Instance.DisplayTip($"{roleName}", $"{targetPlayerName} was immune");
        }
    }



    // Role: Villager
    class Villager : Role
    {
        public override string roleName => "Villager";
        public override int refInt => 1;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleShortDescription => "You do not have any special ability.";
        public override string roleActionText => "";
        public override string roleDescription => "The Villager shall find and kill the Werewolves before ship departure.\nThe Villager can patpat others players.";

        // Parameters
        public override NetworkVariable<float> interactRange => rolesManager.VillagerInteractRange;
        public override NetworkVariable<float> baseActionCooldown => rolesManager.VillagerActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => rolesManager.VillagerStartOfRoundActionCoolDown;
        
        public Villager() : base() { }

        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is staying safe.");
            HUDManager.Instance.DisplayTip($"{roleName}", "*pat pat*");
        }
    }



    // Role: Witch
    class Witch : Role
    {
        public override string roleName => "Witch";
        public override int refInt => 2;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleShortDescription => "You have the ability to protect one player, and kill another one.";
        public override string mainActionName => "Poison";
        public override string secondaryActionName => "Protect";
        public override string roleDescription => "The Witch is part of the village. She shall find and kill the Werewolves before ship departure.\nThe Witch has two potions, and can do two things:\n- Poison another player and kill him (once per round)\n- Protect another player and make him immune once to a Werewolf attack (once per round). The immune player won't know he has been immunized, nor will he know he loses his immune status. The Witch cannot protect herself.";

        // Parameters
        public override NetworkVariable<float> interactRange => rolesManager.WitchInteractRange;
        public override NetworkVariable<float> baseActionCooldown => rolesManager.WitchActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => rolesManager.WitchStartOfRoundActionCoolDown;

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

        public override void NotifyMainActionSuccess(string targetPlayerName)
        {
            logger.LogInfo($"Successfully poisoned {targetPlayerName}.");
            HUDManager.Instance.DisplayTip($"{roleName}", $"You poisoned {targetPlayerName}.");
        }

        public override void NotifySecondaryActionSuccess(string targetPlayerName)
        {
            logger.LogInfo($"Successfully immunized {targetPlayerName}.");
            HUDManager.Instance.DisplayTip($"{roleName}", $"You immunized {targetPlayerName}.");
        }
    }



    // Role: Seer
    class Seer : Role
    {
        public override string roleName => "Seer";
        public override int refInt => 3;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleShortDescription => "You have the ability to see a player's role.";
        public override string mainActionName => "Seer role";
        public override string mainActionText => $"Seer {targetInRangeName}'s role";
        public override string roleDescription => "The Seer is part of the village. She shall find and kill the Werewolves before ship departure.\nThe Seer can seer another player's role.";

        // Parameters
        public override NetworkVariable<float> interactRange => rolesManager.SeerInteractRange;
        public override NetworkVariable<float> baseActionCooldown => rolesManager.SeerActionCooldown;
        public override NetworkVariable<float> startOfRoundActionCooldown => rolesManager.SeerStartOfRoundActionCoolDown;

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
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"{targetPlayerName} is a {role.roleNameColored}");
        }
    }



    // Role: Wild Boy
    class WildBoy : Role
    {
        public override string roleName => "Wild Boy";
        public override int refInt => 4;
        public override string winCondition => "For now, you win with the village.";
        public string _roleShortDescription = "You can idolize a player. If he dies, you become a werewolf.";
        public override string roleShortDescription
        {
            get { return _roleShortDescription; }
            set { _roleShortDescription = value; }
        }
        public override string roleDescription => "The Wild Boy wins either with the Villagers or the Werewolves, depending on his status.\nThe Wild Boy can target a player who becomes his idol. If his idol dies, the Wild Boy becomes a Werewolf. As long as his idol is alive, he wins with the Villagers.";
        public override string mainActionName => "Idolize";
        public ulong? idolizedId;

        // Parameters
        public override NetworkVariable<float> interactRange => rolesManager.WildBoyInteractRange;
        public override NetworkVariable<float> baseActionCooldown => rolesManager.WildBoyActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => rolesManager.WildBoyStartOfRoundActionCoolDown;


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

            roleShortDescription = $"You have idolized {playerName}. If he dies, you become a werewolf.";
            logdebug.LogInfo("Displaying Idolization on HUD");
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"You have idolized {playerName}. If he dies, you will become a werewolf.");
        }


        public void BecomeWerewolf()
        {
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"Your mentor, {rolesManager.GetPlayerById(idolizedId.Value).playerUsername}, is dead. You have become a werewolf.");
            rolesManager.myRole = new Werewolf();

            // Update the roles list to all other clients
            rolesManager.QueryAllRolesServerRpc(sendToAllPlayers: true);
        }
    }



    // Role: Cupid
    class Cupid : Role
    {
        public override string roleName => "Cupid";
        public override int refInt => 5;
        public override string winCondition => "You win with the village.";
        public override string roleShortDescription => "You can make two players fall in love. They must win together. They also die together.";
        public override string roleDescription => "Cupid is able to make two players fall deeply in love.\nWhen one lover die, the other one also dies.\nThe lovers win together. If they're part of the same team, they win with their team. If they're part of different teams, they must be the only two survivors.";

        public override string mainActionName => "Romance";
        public override string secondaryActionName => "Romance Myself";
        public override string secondaryActionText => secondaryActionName;

        new float currentSecondaryActionCooldown => currentMainActionCooldown;

        public List<ulong> lovers = new List<ulong>();
        //public ulong? firstRomancedId;
        //public ulong? secondRomancedId;

        public bool TargetIsAlreadyRomanced => lovers.Contains(targetInRangeId.Value);
        public bool AmIAlreadyRomanced => lovers.Contains(Utils.GetLocalPlayerControllerB().OwnerClientId);
        public bool AreBothTargetsRomanced => (lovers.Count == 2);

        // Parameters
        public override NetworkVariable<float> interactRange => rolesManager.CupidInteractRange;
        public override NetworkVariable<float> baseActionCooldown => rolesManager.CupidActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => rolesManager.CupidStartOfRoundActionCoolDown;
        
        
        public Cupid() : base() { }


        // Cupid really only has one action: romancing someone. The "secondary" action only serves as the main action targetting himself
        public override void SetSecondaryActionOnCooldown()
        {
            SetMainActionOnCooldown();
        }

        public override void SetMainActionOnCooldown()
        {
            // Only set action on cooldown if two targets have been romanced
            if (lovers.Count == 2)
            {
                currentMainActionCooldown = baseMainActionCooldown;
            }
        }


        public override void PerformMainAction()
        {
            logger.LogInfo($"{roleName} is romancing a target: {targetInRangeName}.");
        }

        public override void PerformSecondaryAction()
        {
            logger.LogInfo($"{roleName} is romancing himself.");
        }
        

        public override void NotifyMainActionSuccess(ulong targetId)
        {
            logdebug.LogInfo("I am running the Romancing confirmation Confirmation");
            lovers.Add(targetId);
            NotifyRomancingSuccess(targetId);
        }

        public override void NotifySecondaryActionSuccess(ulong targetId)
        {
            logdebug.LogInfo("I am running the Romancing confirmation Confirmation");
            lovers.Add(Utils.GetLocalPlayerControllerB().OwnerClientId);
            NotifyRomancingSuccess(targetId);
        }

        private void NotifyRomancingSuccess(ulong targetId)
        {
            logdebug.LogInfo("Displaying romancing status on HUD");
            if (AreBothTargetsRomanced)
            {
                roleShortDescription = $"{rolesManager.GetPlayerById(lovers[0])} and {rolesManager.GetPlayerById(lovers[1])} are deeply in love. They die together. They must win together.";
                DisplayRolePopUp();
            }
            else
            {
                HUDManager.Instance.DisplayTip($"{roleName}", $"You targetted {rolesManager.GetPlayerById(targetId)} to be romanced.");
            }
        }

        public override bool IsLocallyAllowedToPerformMainActionRoleSpecific()
        {
            if (TargetIsAlreadyRomanced)
            {
                HUDManager.Instance.DisplayTip($"{roleName}", $"{targetInRangeName} is already romanced.");
                return false;
            }
            return true;
        }

        public override bool IsLocallyAllowedToPerformSecondaryActionRoleSpecific()
        {
            if (AmIAlreadyRomanced)
            {
                HUDManager.Instance.DisplayTip($"{roleName}", $"You are already romanced.");
                return false;
            }
            return true;
        }




    }


}
