
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
        public virtual string roleDescription { get; }
        public virtual Sprite roleIcon => null; // Default icon (null if none)

        public ulong? targetInRange { get; set; }


        public Role()
        {
            
        }
        
        
        public virtual bool IsAllowedToPerformAction()
        {
            logger.LogWarning("Action is allowed by default as no requirements for it were implemented");
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
    }


    public class Werewolf : Role
    {
        public override string roleName => "Werewolf";
        public override int refInt => 0;
        public override string winCondition => "You win by killing all Villagers";
        public override string roleDescription => "You have the ability to kill other players";

        public Werewolf() : base() { }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is hunting!");

        }
        
    }


    public class Villager : Role
    {
        public override string roleName => "Villager";
        public override int refInt => 1;
        public override string winCondition => "You win by killing the Werewolves";
        public override string roleDescription => "You do not have any special ability";

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
        public override string winCondition => "You win by killing the Werewolves";
        public override string roleDescription => "You have the ability to revive one Villager, and kill one player.";

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
        public override string winCondition => "You win by killing the Werewolves";
        public override string roleDescription => "You have the ability to see a player's role";

        public Seer() : base() { }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is omniscient.");
            logger.LogInfo("Looking the role of someone");

            logdebug.LogInfo("Gather the desired player");
            GameObject[] allPlayers;
            allPlayers = StartOfRound.Instance.allPlayerObjects;
            logdebug.LogInfo("Grabbed all Players");
            ulong targetIdOld    = allPlayers[0].GetComponent<PlayerControllerB>().actualClientId;
            string playerName = allPlayers[0].GetComponent<PlayerControllerB>().playerUsername;
            logdebug.LogInfo("Grabbed target Id");

            ulong? targetId = targetInRange;

            if (targetId == null)
            {
                logger.LogError("targetInRange is null. It should have been caught earlier");
            }
            
            
            RolesManager roleManagerObject = Plugin.FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            roleManagerObject.CheckRoleServerRpc(targetId.Value);


        }

        public void DisplayCheckedRole(Role role, string playerName)
        {
            logdebug.LogInfo("Displaying Checked role on HUD");
            HUDManager.Instance.DisplayTip($"Dear {roleName}", $"{playerName} is a {role.roleName}");
        }
    }
}
