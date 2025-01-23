﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.Managers;

using UnityEngine.InputSystem;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Managers
{
    internal class RolesManager : NetworkBehaviour
    {
        
        public RolesManager Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public System.Random rng = Plugin.Instance.rng;
        public Dictionary<ulong, Role> allRoles;

        public Role myRole { get; set; } = new Role();

        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep it across scenes if needed
            }
            else
            {
                logger.LogInfo("Duplicate detected, delted the just-created RolesManager");
                Destroy(gameObject); // Prevent duplicate instances
            }

            SetupKeybindCallbacks();

        }

        private void SetupKeybindCallbacks()
        {
            Plugin.InputActionsInstance.RoleActionKey.performed += OnRoleKeyPressed;
        }

        public void OnRoleKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (myRole.roleName == null) return; // Prevents the default Role class to use the function
            logdebug.LogInfo($"Pressed the key, performing action for my role {myRole.roleName}");
            if (!keyContext.performed) return;
            myRole.PerformRoleAction();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            logger.LogError($"{name} has been destroyed!");
        }


        void Update()
        {

            
           
        }


        // Cast ray to check if a player


        // Automatically gathers the number of players
        public List<Role> GenerateRoles()
        {
            //return GenerateRoles(StartOfRound.Instance.allPlayerObjects.Length);
            return GenerateRoles(GameNetworkManager.Instance.connectedPlayers);
        }

        


        // Specified number of players
        public List<Role> GenerateRoles(int totalPlayers)
        {
            List<Role> roles = new List<Role>();

            // Example logic: One Werewolf and the rest are Villagers
            roles.Add(new Seer());
            for (int i = 1; i < totalPlayers; i++)
            {
                roles.Add(new Villager());
            }

            return roles;
        }

        // Shuffle the roles
        public void ShuffleRoles(List<Role> roles)
        {
            int n = roles.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Role value = roles[k];
                roles[k] = roles[n];
                roles[n] = value;
            }
        }

        // Build final roles from scratch
        public Dictionary<ulong, Role> BuildFinalRolesFromScratch()
        {
            // Build the roles list and other stuff
            GameObject[] allPlayers;
            //List<GameObject> allPlayersList;
            List<Role> roles;
            List<ulong> playersIds;


            logdebug.LogInfo("Getting the list of all connected players");
            // Get list of all players
            int Nplayers = GameNetworkManager.Instance.connectedPlayers;
            allPlayers = StartOfRound.Instance.allPlayerObjects;

            string stringnames = $"Found {Nplayers} players : ";
            for (int i = 0; i < Nplayers;i++)
            {
                string name = allPlayers[i].GetComponent<PlayerControllerB>().playerUsername;
                stringnames += $"{name}";
            }
            logdebug.LogInfo(stringnames);
            //allPlayers = StartOfRound.Instance.allPlayerObjects;

            logdebug.LogInfo("Generate the roles");
            // Generate the roles
            roles = GenerateRoles();

            logdebug.LogInfo("Shuffle the roles");
            // Shuffle the roles
            ShuffleRoles(roles);

            logdebug.LogInfo("Get the list of players client Ids");
            // Get the list of players Client Ids
            playersIds = new List<ulong>();
            for (int i = 0; i<Nplayers;i++)
            {
                GameObject player = allPlayers[i];
                ulong playerId = player.GetComponent<PlayerControllerB>().actualClientId;
                string playerName = player.GetComponent<PlayerControllerB>().playerUsername;
                logdebug.LogInfo($"Added playerName {playerName} with id {playerId.ToString()} to the list");

                playersIds.Add(playerId);
            }


            logdebug.LogInfo("Associate each client Id with a role");
            // Associate each Client Id with a role
            Dictionary<ulong, Role> finalRoles;
            finalRoles = new Dictionary<ulong, Role>();
            for (int i = 0; i < Nplayers; i++)
            {
                logdebug.LogInfo($"{playersIds[i]} {roles[i]}");
                finalRoles.Add(playersIds[i], roles[i]);
            }

            return finalRoles;
        }

        public Role DisplayRoleToolTip()
        {
            logger.LogInfo("Displaying my role tooltip");
            logdebug.LogInfo("Grabbing my Role");
            Role role = myRole;
            logdebug.LogInfo("Grab the PlayerControllerB instance");
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            logdebug.LogInfo("Grab the playerUsername from the HUDManager instance");
            string playerName = player.playerUsername;

            logdebug.LogInfo("Grab the role name from the role name itself");
            string roleName = role.roleName;
            string winCondition = role.winCondition;
            string roleDescription = role.roleDescription;

            logdebug.LogInfo("Display the role using the HUDManager.Instance.DisplayTip method");
            HUDManager.Instance.DisplayTip($"You are a {roleName}", $"{winCondition}\n{roleDescription}");
            return role;

        }


        [ClientRpc]
        public void SendRoleClientRpc(int roleInt, ClientRpcParams clientRpcParams = default)
        {
            // Retrieve the role
            logdebug.LogInfo($"Received roleInt {roleInt}");
            Role role = References.references()[roleInt];
            logdebug.LogInfo($"I can see the role : {role} with name {role.roleName} and refInt {role.refInt}");


            // Assign the player's role
            //RolesManager roleManagerObject = FindObjectOfType<RolesManager>();
            myRole = role;


            logdebug.LogInfo("I have succesfully set my own role");

            // Display the tooltip for the role
            DisplayRoleToolTip();
            logdebug.LogInfo("I have successfully displayed my Role tooltip");

            // Locate the RoleHUD and update it
            logdebug.LogInfo("Trying to update HUD");
            RoleHUD roleHUD = FindObjectOfType<RoleHUD>();
            if (roleHUD != null)
            {
                logger.LogInfo("Update the HUD with the role");
                roleHUD.UpdateRoleDisplay(role);
            }
            else
            {
                logger.LogInfo("Did not find the HUD");
            }

            string playerName = GameNetworkManager.Instance.localPlayerController.playerUsername;
            string roleName = myRole.roleName;
            logdebug.LogInfo($"I am player {playerName} and I have fully completed and received the role {roleName}");
        }


        [ServerRpc(RequireOwnership = false)]
        public void CheckRoleServerRpc(ulong targetId, string playerName, ServerRpcParams serverRpcParams = default)
        {
            logdebug.LogInfo($"Executing ServerRpc while I am the host: {IsHost || IsServer}");

            //RolesManager roleManagerObject = FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            //logdebug.LogInfo("Grabbed RoleManager");
            ulong senderId = serverRpcParams.Receive.SenderClientId; // Get the sender Id
            logdebug.LogInfo($"Grabbed sender ID: {senderId}");
            int refInt = allRoles[targetId].refInt; // Find the refInt of the desired role
            logdebug.LogInfo($"grabbed refInt of checked role : {refInt}");

            // Build the clientRpcParams to only answer to the caller
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            };

            logdebug.LogInfo("Built ClientRpcParams");

            CheckRoleClientRpc(refInt, playerName, clientRpcParams);

        }

        [ClientRpc]
        public void CheckRoleClientRpc(int refInt, string playerName, ClientRpcParams clientRpcParams = default)
        {
            // Retrieve the role
            logdebug.LogInfo($"Received refInt {refInt}");
            Role role = References.references()[refInt];
            logdebug.LogInfo("Reversed the refInt into a Role");
            new Seer().DisplayCheckedRole(role, playerName);
        }

    }
    
}

