
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
        public ManualLogSource logger = Plugin.instance.logger;
        
        public virtual string roleName { get; }
        public virtual int refInt { get; }

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
        public Seer() : base() { }

        public override void PerformRoleAction()
        {
            logger.LogInfo($"The {roleName} is omniscient.");
        }
    }
}
