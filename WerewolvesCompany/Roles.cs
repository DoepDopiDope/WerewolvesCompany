
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
        public static Dictionary<int, Role> references()
        {
            Dictionary<int, Role> dic = new Dictionary<int, Role>();
            dic.Add(0, new Werewolf());
            dic.Add(1, new Villager());
            dic.Add(2, new Witch());
            dic.Add(3, new Seer());
            dic.Add(4, new WildBoy());

            return dic;
        }
    }

    public class Role
    {
        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public virtual string roleName { get; }
        public virtual int refInt { get; }
        public virtual string winCondition { get; }
        public virtual string roleDescription { get; set; }
        public virtual string rolePopUp  => $"{winCondition} {roleDescription}";
        public virtual Sprite roleIcon => null; // Default icon (null if none)

        public ulong? targetInRangeId { get; set; }
#nullable enable
        public string? targetInRangeName { get; set; }


#nullable disable
        public virtual string roleActionText { get { return $"[{roleActionKey}] {roleActionName} {targetInRangeName}";}  }
        public virtual string roleActionKey { get { return "K"; } }
        public virtual string roleActionName { get { return "Action Name"; } }

        public virtual NetworkVariable<float> interactRange => Utils.GetRolesManager().DefaultInteractRange;
        public virtual NetworkVariable<float> actionCooldown => Utils.GetRolesManager().DefaultActionCoolDown;


        public Role()
        {
            
        }
        
        public void DisplayRolePopUp()
        {
            logdebug.LogInfo("Display the role PopUp");
            HUDManager.Instance.DisplayTip($"You are a {roleName}", rolePopUp);
        }

        public virtual bool IsLocallyAllowedToPerformAction()
        {
            return true;
        }

        public virtual bool IsAllowedToPerformAction()
        {
            return true;
        }

        public void GenericPerformRoleAction()
        {

            bool flag = false;
            try
            {
                PerformRoleAction();
                flag = true;
            }
            catch (Exception e)
            {
                HUDManager.Instance.DisplayTip("Error", "Failed to perform my Role Action");
                logger.LogError("Failed to perform my role action");
                logger.LogError(e);
            }

            if (flag)
            {
                RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object}
                roleManagerObject.SuccessFullyPerformedRoleActionServerRpc();
            }

        }

        public virtual void PerformRoleAction()
        {
            // Default behavior for a role

            logger.LogInfo($"Performing action for role: {roleName}");
        }

        public virtual void SetRoleOnCooldown()
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


    public class Werewolf : Role
    {
        public override string roleName => "Werewolf";
        public override int refInt => 0;
        public override string winCondition => "You win by killing all Villagers";
        public override string roleDescription => "You have the ability to kill other players";
        public override string roleActionName => "Kill";
        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().WerewolfInteractRange;
        public override NetworkVariable<float> actionCooldown => Utils.GetRolesManager().WerewolfActionCoolDown;

        public Werewolf() : base() { }

        public override bool IsLocallyAllowedToPerformAction()
        {
            if (targetInRangeId == null) return false;
            return true;
        }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is hunting!");
            ulong targetId = GrabTargetPlayer();

            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.WerewolfKillPlayerServerRpc(targetId);

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
        public override NetworkVariable<float> actionCooldown => Utils.GetRolesManager().VillagerActionCoolDown;

        public Villager() : base() { }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is staying safe.");
        }
    }


    public class Witch : Role
    {
        public override string roleName => "Witch";
        public override int refInt => 2;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleDescription => "You have the ability to revive one Villager, and kill one player.";
        public override string roleActionName => "NotImplemented";
        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().WitchInteractRange;
        public override NetworkVariable<float> actionCooldown => Utils.GetRolesManager().WitchActionCoolDown;


        public Witch() : base() { }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is making potions.");
        }
    }


    public class Seer : Role
    {
        public override string roleName => "Seer";
        public override int refInt => 3;
        public override string winCondition => "You win by killing the Werewolves.";
        public override string roleDescription => "You have the ability to see a player's role.";
        public override string roleActionName => "Check role";
        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().SeerInteractRange;
        public override NetworkVariable<float> actionCooldown => Utils.GetRolesManager().SeerActionCooldown;


        public Seer() : base() { }

        public override bool IsLocallyAllowedToPerformAction()
        {
            if (targetInRangeId == null) return false;
            return true;
        }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is omniscient.");
            logger.LogInfo("Looking the role of someone");

            ulong targetId = GrabTargetPlayer();
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.CheckRoleServerRpc(targetId);
        }

        public void DisplayCheckedRole(Role role, string playerName)
        {
            logdebug.LogInfo("Displaying Checked role on HUD");
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"{playerName} is a {role.roleName}");
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
        
        public override string roleActionName => "Idolize";
        public ulong? idolizedId;

        public override NetworkVariable<float> interactRange => Utils.GetRolesManager().WildBoyInteractRange;
        public override NetworkVariable<float> actionCooldown => Utils.GetRolesManager().WildBoyActionCoolDown;


        public WildBoy() : base() { }

        public override bool IsLocallyAllowedToPerformAction()
        {
            if (targetInRangeId == null) return false;
            return true;
        }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is loitering.");
            logger.LogInfo("Idolizing someone");

            ulong targetId = GrabTargetPlayer();
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.IdolizeServerRpc(targetId);
        }

        public void ConfirmIdolization(ulong targetId)
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
