﻿
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



namespace WerewolvesCompany
{
    public class References
    {
        public static List<Role> GetAllRoles()
        {
            var roles = new List<Role>();

            roles.Add(new Werewolf());
            roles.Add(new Villager());
            roles.Add(new Witch());
            roles.Add(new Seer());
            roles.Add(new WildBoy());

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

    public class Role
    {
        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public virtual string roleName { get; }
        public string terminalName => roleName.Replace(" ", "_");
        public virtual int refInt { get; }
        public virtual string winCondition { get; }
        public virtual string roleDescription { get; set; }
        public virtual string rolePopUp  => $"{winCondition} {roleDescription}";
        public virtual Sprite roleIcon => null; // Default icon (null if none)

        

        // Who's in range
        public ulong? targetInRangeId { get; set; }
#nullable enable
        public string? targetInRangeName { get; set; }
#nullable disable


        // ToolTip
        
        public virtual string mainActionKey { get { return "K"; } }
        public virtual string secondaryActionKey { get { return "L"; } }
        public virtual string mainActionName { get { return "Main Action Name"; } }
        public virtual string secondaryActionName { get { return "Secondary Action Name"; } }

        public virtual string mainActionText { get { return $"{mainActionName} {targetInRangeName}"; } }
        public virtual string mainActionTooltip { get { return $"[{mainActionKey}] {mainActionText} {GetCurrentMainActionCooldownText()}".Trim(); } }
        public virtual string secondaryActionText { get { return $"{secondaryActionName} {targetInRangeName}"; } }
        public virtual string secondaryActionTooltip { get { return $"[{secondaryActionKey}] {secondaryActionText} {GetCurrentSecondaryActionCooldownText()}".Trim(); } }
        public virtual string roleActionText { get { return GetRoleActionText(); } }



        // Settings
        public virtual NetworkVariable<float> interactRange => Utils.GetRolesManager().DefaultInteractRange;
        public virtual NetworkVariable<float> baseActionCooldown => Utils.GetRolesManager().DefaultActionCoolDown;
        public virtual NetworkVariable<float> startOfRoundActionCooldown => Utils.GetRolesManager().DefaultStartOfRoundActionCoolDown;


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

        public void DisplayRolePopUp()
        {
            logdebug.LogInfo("Display the role PopUp");
            HUDManager.Instance.DisplayTip($"You are a {roleName}", rolePopUp);
        }

        public virtual bool IsLocallyAllowedToPerformMainAction()
        {
            return (!IsMainActionOnCooldown && !(targetInRangeId == null));
        }

        public virtual bool IsLocallyAllowedToPerformSecondaryAction()
        {
            return (!IsSecondaryActionOnCooldown && !(targetInRangeId == null));
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
                RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object}
                roleManagerObject.SuccessFullyPerformedMainActionServerRpc();
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
                Utils.GetRolesManager().SuccessFullyPerformedSecondaryActionServerRpc();
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



        public void SetMainActionOnCooldown()
        {
            currentMainActionCooldown = baseMainActionCooldown;
        }

        public void SetSecondaryActionOnCooldown()
        {
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


        // Notifications of success and failed actions
        public virtual void NotifyMainActionSuccess(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleName, "Main action success");
        }


        public virtual void NotifySecondaryActionSuccess(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleName, "Secondary action success");
        }

        public virtual void NotifyMainActionFailed(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleName, "Main action failed");
        }


        public virtual void NotifySecondaryActionFailed(string targetPlayerName)
        {
            HUDManager.Instance.DisplayTip(roleName, "Secondary action failed");
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
    public class Werewolf : Role
    {
        public override string roleName => "Werewolf";
        public override int refInt => 0;
        public override string winCondition => "You win by killing all Villagers";
        public override string roleDescription => "You have the ability to kill other players";
        public override string mainActionName => "Kill";
        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().WerewolfInteractRange;
        public override NetworkVariable<float> baseActionCooldown => Utils.GetRolesManager().WerewolfActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => Utils.GetRolesManager().WerewolfStartOfRoundActionCoolDown;

        public Werewolf() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is hunting!");
            ulong targetId = GrabTargetPlayer();
            
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            logger.LogInfo($"Killing {roleManagerObject.GetPlayerById(targetId).playerUsername}.");
            roleManagerObject.WerewolfKillPlayerServerRpc(targetId);
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


    public class Villager : Role
    {
        public override string roleName => "Villager";
        public override int refInt => 1;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleDescription => "You do not have any special ability.";
        public override string roleActionText => "";
        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().VillagerInteractRange;
        public override NetworkVariable<float> baseActionCooldown => Utils.GetRolesManager().VillagerActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => Utils.GetRolesManager().VillagerStartOfRoundActionCoolDown;
        
        public Villager() : base() { }

        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is staying safe.");
            HUDManager.Instance.DisplayTip($"{roleName}", "*pat pat*");
        }
    }


    public class Witch : Role
    {
        public override string roleName => "Witch";
        public override int refInt => 2;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleDescription => "You have the ability to protect one player, and kill another one.";
        public override string mainActionName => "Poison";
        public override string secondaryActionName => "Protect";
        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().WitchInteractRange;
        public override NetworkVariable<float> baseActionCooldown => Utils.GetRolesManager().WitchActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => Utils.GetRolesManager().WitchStartOfRoundActionCoolDown;

        public Witch() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is poisoning someone.");
            ulong targetId = GrabTargetPlayer();

            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.WitchPoisonPlayerServerRpc(targetId);

        }


        public override void PerformSecondaryAction()
        {
            logger.LogInfo($"The {roleName} is immunising someone.");
            ulong targetId = GrabTargetPlayer();
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.WitchImmunizePlayerServerRpc(targetId);
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


    public class Seer : Role
    {
        public override string roleName => "Seer";
        public override int refInt => 3;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleDescription => "You have the ability to see a player's role.";
        public override string mainActionName => "Seer role";
        public override string mainActionText => $"Seer {targetInRangeName}'s role";
        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().SeerInteractRange;
        public override NetworkVariable<float> baseActionCooldown => Utils.GetRolesManager().SeerActionCooldown;
        public override NetworkVariable<float> startOfRoundActionCooldown => Utils.GetRolesManager().SeerStartOfRoundActionCoolDown;

        public Seer() : base() { }

        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is omniscient.");
            logger.LogInfo("Looking the role of someone");

            ulong targetId = GrabTargetPlayer();
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.CheckRoleServerRpc(targetId);
        }


        public override void NotifyMainActionSuccess(string targetPlayerName, Role role)
        {
            logdebug.LogInfo("Displaying Checked role on HUD");
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"{targetPlayerName} is a {role.roleName}");
        }
    }


    public class WildBoy : Role
    {
        public override string roleName => "Wild Boy";
        public override int refInt => 4;
        public override string winCondition => "For now, you win with the village.";
        public string _roleDescription = "You can idolize a player. If he dies, you become a werewolf.";
        public override string roleDescription
        {
            get { return _roleDescription; }
            set { _roleDescription = value; }
        }
        
        public override string mainActionName => "Idolize";
        public ulong? idolizedId;

        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().WildBoyInteractRange;
        public override NetworkVariable<float> baseActionCooldown => Utils.GetRolesManager().WildBoyActionCoolDown;
        public override NetworkVariable<float> startOfRoundActionCooldown => Utils.GetRolesManager().WildBoyStartOfRoundActionCoolDown;


        public WildBoy() : base() { }


        public override void PerformMainAction()
        {
            logger.LogInfo($"The {roleName} is loitering.");
            logger.LogInfo("Idolizing someone");

            ulong targetId = GrabTargetPlayer();
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.IdolizeServerRpc(targetId);
        }

        public override void NotifyMainActionSuccess(ulong targetId)
        {
            logdebug.LogInfo("I am running the Idolization Confirmation");
            idolizedId = targetId;

            logdebug.LogInfo("I have set my idolization mentor");
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            string playerName = roleManagerObject.GetPlayerById(targetId).playerUsername;

            roleDescription = $"You have idolized {playerName}. If he dies, you become a werewolf.";
            logdebug.LogInfo("Displaying Idolization on HUD");
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"You have idolized {playerName}. If he dies, you will become a werewolf.");
        }


        public void BecomeWerewolf()
        {
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"Your mentor {Utils.GetRolesManager().GetPlayerById(idolizedId.Value).playerUsername} is dead. You have become a werewolf.");
            Utils.GetRolesManager().myRole = new Werewolf();
        }
    }

}
