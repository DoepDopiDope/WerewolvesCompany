
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

        public Role()
        {
            
        }
        
        

        public virtual void PerformRoleAction()
        {
            // Default behavior for a role

            logger.LogInfo($"Performing action for role: {roleName}");
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
        }
    }
}
