using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using WerewolvesCompany.Managers;


// Highly inspired from TooManyEmotes
namespace WerewolvesCompany.Patches
{
    [HarmonyPatch]
    internal class TerminalPatcher
    {
        public static Terminal terminalInstance;
        public static bool initializedTerminalNodes = false;

        static RolesManager rolesManager => Utils.GetRolesManager();
        public static List<Role> availableRoles => References.GetAllRoles();
        //public List<Role> currentRoles => Utils.GetRolesManager().currentRolesSetup;


        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;
        static public ManualLogSource logupdate = Plugin.Instance.logupdate;



        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), "Awake")]
        private static void InitializeTerminal(Terminal __instance)
        {
            terminalInstance = __instance;
            initializedTerminalNodes = false;
            EditExistingTerminalNodes();
        }


        private static void EditExistingTerminalNodes()
        {
            logdebug.LogInfo("Editting existing terminal nodes");
            initializedTerminalNodes = true;

            //if (ConfigSync.instance.syncUnlockEverything)
            //    return;

            foreach (TerminalNode node in terminalInstance.terminalNodes.specialNodes)
            {
                if (node.name == "Start" && !node.displayText.Contains("[WerwolvesCompany]"))
                {
                    logdebug.LogInfo("Editting Start node");
                    string keyword = "Type \"Help\" for a list of commands.";
                    int insertIndex = node.displayText.IndexOf(keyword);
                    if (insertIndex != -1)
                    {
                        insertIndex += keyword.Length;
                        string addText = "\n\n[Werewolves Company]\nType \"Werewolves\" for a list of commands.";
                        node.displayText = node.displayText.Insert(insertIndex, addText);
                    }
                    else
                        logger.LogError("Failed to add werewolves tip to terminal. Maybe an update broke it?");
                }

                else if (node.name == "HelpCommands" && !node.displayText.Contains(">WEREWOLVES"))
                {
                    logdebug.LogInfo("Editting HelpCommands node");
                    string keyword = "[numberOfItemsOnRoute]";
                    int insertIndex = node.displayText.IndexOf(keyword);
                    if (insertIndex != -1)
                    {
                        string addText = ">WEREWOLVES\n" +
                            "For a list of Werewolves commands.\n\n";
                        node.displayText = node.displayText.Insert(insertIndex, addText);
                    }
                }
            }
        }



        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        private static void OnBeginUsingTerminal(Terminal __instance)
        {
            logdebug.LogInfo("I just started using the terminal");
            if (!initializedTerminalNodes)
                EditExistingTerminalNodes();
        }



        [HarmonyPrefix]
        [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
        private static bool ParsePlayerSentence(ref TerminalNode __result, Terminal __instance)
        {
            if (__instance.screenText.text.Length <= 0)
            {
                logdebug.LogInfo("Apparently the text length was negative?");
                return true;
            }

            //string input = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower();
            string input = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower();
            string[] args = input.Split(' ');

            logdebug.LogInfo($"Parsing sentence {input}");
            logdebug.LogInfo($"Input: {input}, with args: {args.ToString()} of length {args.Length.ToString()}");

            if (!(input.StartsWith("werewolves") || input.StartsWith("wc")))
            {
                logdebug.LogInfo("Not a werewolves command");
                return true;
            }

            // Enter the werewolves Menu
            if (input.StartsWith("werewolves") || input.StartsWith("wc"))
            {
                logdebug.LogInfo("werewolves command was invoked");
                // if werewolves command was invoked
                if (args.Length == 1)
                {
                    logger.LogInfo("Loading WerewolvesCompany Terminal's home menu");
                    __result = BuildTerminalNodeHome();
                }

                // if the add keyword was provided, add a role to the list
                else if (args[1].ToLower() == "add")
                {
                    for (int i = 2; i < args.Length; i++)
                    {
                        string roleName = args[i];
                        if (!RoleIsAvailable(roleName))
                        {
                            logger.LogInfo($"Cannot add the role {roleName}, it is not part of the available roles");
                            __result = BuildRoleNotAvailableNode(roleName);
                        }
                        else
                        {
                            AddNewRole(roleName);
                            logger.LogInfo($"Added role {roleName} to the current roles");
                            // Refresh the window
                            __result = BuildTerminalNodeHome();
                        }
                        
                    }
                }

                // if the delete keyword was provided, add a role to the list
                else if ((args[1].ToLower() == "delete") || (args[1].ToLower() == "del"))
                {
                    if (args.Length >3)
                    {
                        logger.LogInfo($"Can only delete roles one at a time");
                        __result = BuilDeleteRolesOnceAtATimeNode();
                        return false;
                    }

                    for (int i = 2; i < args.Length; i++)
                    {
                        string roleName = args[i];
                        // Check if role is in fact in play
                        if (!RoleIsInPlay(roleName))
                        {
                            logger.LogInfo($"Cannot delete the role {roleName}, it is not part of the current roles");
                            __result = BuildRoleNotInPlayNode(roleName);
                        }
                        else
                        {
                            DeleteRole(roleName);
                            logger.LogInfo($"Removed role {roleName} from the current roles");
                            // Refresh the window
                            __result = BuildTerminalNodeHome();
                        }
                    }
                }
                // Debug inputs
                else if (args[1].ToLower() == "debug")
                {
                    __result = BuildTerminalNodeHome();
                    if (args[2].ToLower() == "cd")
                    {
                        rolesManager.ResetAllCooldownsServerRpc();
                    }
                    if ((args[2].ToLower() == "distrib") || args[2].ToLower() == "distribute")
                    {
                        rolesManager.BuildAndSendRoles();
                    }

                    if (args[2].ToLower() == "reset")
                    {
                        rolesManager.ResetRolesServerRpc();
                    }

                    return false;
                }


                else
                {
                    return true;
                }

                return false;

            }

            // If input is not werewolves, pass to the regular parser
            logdebug.LogInfo("Not a werewolf command, pass.");
            return true;
            
        }
        

        private static void AddNewRole(string roleName)
        {
            Role roleToAdd = References.GetRoleByName(roleName);
            logger.LogInfo($"Adding role {roleToAdd.roleName} to the list");
            rolesManager.currentRolesSetup.Add(roleToAdd);
            rolesManager.UpdateCurrentRolesServerRpc(rolesManager.WrapRolesList(rolesManager.currentRolesSetup));
        }


        private static void DeleteRole(string roleName)
        {
            for (int i=0 ; i< rolesManager.currentRolesSetup.Count ; i++)
            {
                logdebug.LogInfo($"Checking {roleName} against {rolesManager.currentRolesSetup[i].terminalName}");
                if (rolesManager.currentRolesSetup[i].terminalName.ToLower() == roleName.ToLower())
                {
                    logger.LogInfo($"Delete role {rolesManager.currentRolesSetup[i].terminalName.ToLower()} from the list");
                    rolesManager.currentRolesSetup.RemoveAt(i);
                    rolesManager.UpdateCurrentRolesServerRpc(rolesManager.WrapRolesList(rolesManager.currentRolesSetup));
                    return;
                }
            }
            throw new Exception($"Role {roleName} not found in the list of current roles. This shoud have been caught earlier.");
        }


        private static void RemoveRole(string roleName)
        {

        }

        private static TerminalNode BuildTerminalNodeHome()
        {
            TerminalNode homeTerminalNode = new TerminalNode
            {
                displayText = "[Werewolves Company]\n\n" +
                    "------------------------------\n" +
                    "[[[availableRoles]]]\n" +
                    "------------------------------\n" +
                    "[[[currentRolesSetup]]]\n\n",
                clearPreviousText = true,
                acceptAnything = false
            };
            rolesManager.QueryCurrentRolesServerRpc();
            return homeTerminalNode;
        }

        private static TerminalNode BuildRoleNotAvailableNode(string roleName)
        {
            TerminalNode homeTerminalNode = new TerminalNode
            {
                displayText = "[Werewolves Company]\n\n" +
                    "------------------------------\n" +
                    $"The role '{roleName}' is not available\n\n",
                clearPreviousText = true,
                acceptAnything = false
            };
            return homeTerminalNode;
        }


        private static TerminalNode BuildRoleNotInPlayNode(string roleName)
        {
            TerminalNode homeTerminalNode = new TerminalNode
            {
                displayText = "[Werewolves Company]\n\n" +
                    "------------------------------\n" +
                    $"The role '{roleName}' is not part of the current roles\n\n",
                clearPreviousText = true,
                acceptAnything = false
            };
            return homeTerminalNode;
        }


        private static TerminalNode BuilDeleteRolesOnceAtATimeNode()
        {
            TerminalNode homeTerminalNode = new TerminalNode
            {
                displayText = "[Werewolves Company]\n\n" +
                   "------------------------------\n" +
                   $"Please remove roles one at a time\n\n",
                clearPreviousText = true,
                acceptAnything = false
            };
            return homeTerminalNode;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Terminal), "TextPostProcess")]
        private static void TextPostProcess(ref string modifiedDisplayText, TerminalNode node)
        {
            logdebug.LogInfo("I just started text post processing");
            if (modifiedDisplayText.Length <= 0)
                return;

            // Add available roles
            string availableRolesPlaceholderText = "[[[availableRoles]]]";
            if (modifiedDisplayText.Contains(availableRolesPlaceholderText))
            {
                int index0 = modifiedDisplayText.IndexOf(availableRolesPlaceholderText);
                int index1 = index0 + availableRolesPlaceholderText.Length;
                string textToReplace = modifiedDisplayText.Substring(index0, index1 - index0);
                string replacementText = "";

                // Add the roles
                replacementText += "Available Roles:\n";
                foreach (var role in availableRoles)
                {
                    replacementText += role.terminalName + "\n";
                }
                modifiedDisplayText = modifiedDisplayText.Replace(textToReplace, replacementText);
            }

            // Add current roles setup
            string currentRolesSetupPlaceholderText = "[[[currentRolesSetup]]]";
            if (modifiedDisplayText.Contains(currentRolesSetupPlaceholderText))
            {
                int index0 = modifiedDisplayText.IndexOf(currentRolesSetupPlaceholderText);
                int index1 = index0 + currentRolesSetupPlaceholderText.Length;
                string textToReplace = modifiedDisplayText.Substring(index0, index1 - index0);
                string replacementText = "";

                // Add the roles
                replacementText += "Current Roles Setup:\n";
                foreach (var role in rolesManager.currentRolesSetup)
                {
                    replacementText += role.terminalName + "\n";
                }
                replacementText += "\nRemaining slots will be filled with Villagers\n\n";
                replacementText += "Add roles    -> werewolves add role_name\n";
                replacementText += "Delete roles -> werewolves del role_name\n\n";
                modifiedDisplayText = modifiedDisplayText.Replace(textToReplace, replacementText);
            }
        }

        private static bool RoleIsAvailable(string roleName)
        {
            foreach(Role role in availableRoles)
            {
                if (role.terminalName.ToLower() == roleName.ToLower()) return true;
            }
            return false;
        }

        private static bool RoleIsInPlay(string roleName)
        {
            foreach (Role role in rolesManager.currentRolesSetup)
            {
                if (role.terminalName.ToLower() == roleName.ToLower()) return true;
            }
            return false;
        }
    }
}
