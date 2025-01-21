
using BepInEx;
using HarmonyLib;
using WerewolvesCompany.Managers;
using System.IO;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;
using HarmonyLib.Tools;




namespace WerewolvesCompany
{
    public class Role
    {
        public ManualLogSource logger = Plugin.instance.logger;

        public string roleName { get; set; }

        public Role(string roleFullName)
        {
            roleName = roleFullName;
            
        }

        public virtual void PerformRoleAction()
        {
            // Default behavior for a role

            logger.LogInfo($"Performing action for role: {roleName}");
        }
    }


    public class Werewolf : Role
    {
        public Werewolf() : base("Werewolf") { }

        public override void PerformRoleAction()
        {
            logger.LogInfo("The Werewolf is hunting!");

        }
    }


    public class Villager : Role
    {
        public Villager() : base("Villager") { }

        public override void PerformRoleAction()
        {
            logger.LogInfo("The Villager is staying safe.");
        }
    }


    public class Witch : Role
    {
        public Witch() : base("Witch") { }

        public override void PerformRoleAction()
        {
            logger.LogInfo("The Witch is staying safe.");
        }
    }


    public class Seer : Role
    {
        public Seer() : base("Seer") { }

        public override void PerformRoleAction()
        {
            logger.LogInfo("The Seer is staying safe.");
        }
    }
}
