﻿using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using WerewolvesCompany.Managers;
using WerewolvesCompany.UI;


// Highly inspired from TooManyEmotes
namespace WerewolvesCompany.Patches
{
    [HarmonyPatch]
    internal class TerminalPatcher
    {
        public static Terminal terminalInstance;
        public static bool initializedTerminalNodes = false;

        static RolesManager rolesManager => Plugin.Instance.rolesManager;
        static RoleHUD roleHUD => Plugin.Instance.roleHUD;
        static QuotaManager quotaManager => Plugin.Instance.quotaManager;
        
        public static List<Role> availableRoles => References.GetAllRoles();


        //public List<Role> currentRoles => Utils.GetRolesManager().currentRolesSetup;


        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;



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
                        string addText = "\n\n[Werewolves Company]\nType \"Werewolves\" or \"wc\" for a list of commands.";
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
                else if (args[1] == "add")
                {
                    // If number of roles were provided
                    if (args.Length == 4)
                    {
                        string roleName = args[2];
                        if (!RoleIsAvailable(roleName))
                        {
                            logger.LogInfo($"Cannot add the role {roleName}, it is not part of the available roles");
                            __result = BuildRoleNotAvailableNode(roleName);
                        }

                        int N;
                        if (int.TryParse(args[3], out N))
                        {
                            AddNewRole(roleName,N);
                            // Refresh the window
                            __result = BuildTerminalNodeHome();
                            return false;
                        }
                    }

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
                            // Refresh the window
                            __result = BuildTerminalNodeHome();
                        }
                        
                    }
                }

                // if the delete keyword was provided, add a role to the list
                else if ((args[1] == "delete") || (args[1] == "del"))
                {
                    if (args.Length > 3)
                    {
                        logger.LogInfo($"Can only delete roles one at a time");
                        __result = BuilDeleteRolesOnceAtATimeNode();
                        return false;
                    }

                    if (args[2] == "*")
                    {
                        DeleteAllRoles();
                        logger.LogInfo($"Deleted all roles");
                        __result = BuildTerminalNodeHome();
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
                else if (args[1] == "debug")
                {
                    __result = BuildTerminalNodeDebug();

                    if (args.Length == 2)
                    {
                        return false;
                    }

                    else if (args[2] == "cd")
                    {
                        rolesManager.ResetAllCooldownsServerRpc();
                    }
                    else if ((args[2] == "distrib") || args[2] == "distribute")
                    {
                        rolesManager.BuildAndSendRolesServerRpc();
                    }

                    else if (args[2] == "reset")
                    {
                        rolesManager.ResetRolesServerRpc();
                    }

                    else if (args[2] == "quota")
                    {
                        rolesManager.CheatQuotaServerRpc();
                    }

                    return false;
                }

                else if (RoleIsAvailable(args[1]))
                {
                    logger.LogInfo($"Displaying terminal information for role {args[1]}");
                    __result = BuildRoleInformationNode(args[1]);
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
        

        private static void AddNewRole(string roleName, int N = 1)
        {
            Role roleToAdd = References.GetRoleByName(roleName);
            logger.LogInfo($"Adding role x{N} {roleToAdd.roleName} to the list");
            for (int i = 0; i < N; i++)
            {
                rolesManager.currentRolesSetup.Add(roleToAdd);
            }
            rolesManager.UpdateCurrentRolesServerRpc(rolesManager.WrapRolesList(rolesManager.currentRolesSetup));
        }


        private static void DeleteRole(string roleName)
        {
            for (int i=0 ; i< rolesManager.currentRolesSetup.Count ; i++)
            {
                logdebug.LogInfo($"Checking {roleName} against {rolesManager.currentRolesSetup[i].terminalName}");
                if (rolesManager.currentRolesSetup[i].terminalName.ToLower() == roleName.ToLower())
                {
                    //logger.LogInfo($"Delete role {rolesManager.currentRolesSetup[i].terminalName.ToLower()} from the list");
                    rolesManager.currentRolesSetup.RemoveAt(i);
                    rolesManager.UpdateCurrentRolesServerRpc(rolesManager.WrapRolesList(rolesManager.currentRolesSetup));
                    return;
                }
            }
            throw new Exception($"Role {roleName} not found in the list of current roles. This shoud have been caught earlier.");
        }


        public static void DeleteAllRoles()
        {
            rolesManager.currentRolesSetup = new List<Role> ();
            rolesManager.UpdateCurrentRolesServerRpc(rolesManager.WrapRolesList(rolesManager.currentRolesSetup));
        }

        private static void RemoveRole(string roleName)
        {

        }

        private static TerminalNode BuildTerminalNodeDebug()
        {
            TerminalNode homeTerminalNode = new TerminalNode
            {
                displayText = "[Werewolves Company]\n\n" +
                              "------------------------------\n" +
                              "Debug commands\n" +
                              "------------------------------\n" +
                              "wc debug         -> show this page\n" +
                              "wc debug cd      -> set all players cooldowns to 0\n" +
                              "wc debug distrib -> distribute roles\n" +
                              "wc debug reset   -> reset all players roles to their initial state\n" +
                              "wc debug quota   -> bypass the current daily quota\n\n",
                clearPreviousText = true,
                acceptAnything = false
            };
            return homeTerminalNode;
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

        private static TerminalNode BuildRoleInformationNode(string roleName)
        {
            TerminalNode homeTerminalNode = new TerminalNode
            {
                displayText = "[Werewolves Company]\n\n" +
                              "------------------------------\n" +
                              $"[[[{roleName.ToLower()}]]]\n\n",
                clearPreviousText = true,
                acceptAnything = false
            };
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
                    replacementText += role.terminalNameColored + "\n";
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
                    replacementText += role.terminalNameColored + "\n";
                }
                replacementText += "\nRemaining slots will be filled with Villagers\n\n";
                replacementText += "wc add role_name (N) -> Add (N) roles\n";
                replacementText += "wc add role1 role2   -> Add roles\n";
                replacementText += "wc del role_name     -> Delete role\n";
                replacementText += "wc del *             -> Delete all roles\n";
                replacementText += "wc role_name         -> Check role informations\n";
                replacementText += "wc debug             -> Debug commands\n\n";
                modifiedDisplayText = modifiedDisplayText.Replace(textToReplace, replacementText);
            }

            // add informations for roles
            foreach (Role role in availableRoles)
            {
                string thisRoleInformationPlaceHolder = $"[[[{role.terminalName.ToLower()}]]]";
                
                if (modifiedDisplayText.Contains(thisRoleInformationPlaceHolder))
                {
                    modifiedDisplayText = modifiedDisplayText.Replace(thisRoleInformationPlaceHolder, $"-> {role.roleName}\n{role.roleDescription}");
                }
                
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
