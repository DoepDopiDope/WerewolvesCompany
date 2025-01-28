using System;
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
using static UnityEngine.InputSystem.Layouts.InputControlLayout;
using System.ComponentModel;
using WerewolvesCompany.Patches;
using System.Runtime.CompilerServices;
using UnityEngine.Windows;
using static UnityEngine.InputSystem.InputRemoting;
using TMPro;
using BepInEx;
using BepInEx.Configuration;
using System.Collections;
using UnityEngine.InputSystem.HID;
using DunGen.Graph;
using UnityEngine.UIElements;



namespace WerewolvesCompany.Managers
{
    class RolesManager : NetworkBehaviour
    {
        
        public RolesManager Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;
        public ManualLogSource logupdate = Plugin.Instance.logupdate;

        public System.Random rng = Plugin.Instance.rng;
        public Dictionary<ulong, Role> allRoles;

        public List<Role> currentRolesSetup = new List<Role>();

        //public Role spectatedPlayerRole;

#nullable enable
        public Role? myRole { get; set; }
#nullable disable



        // Default parameters
        public NetworkVariable<float> DefaultInteractRange   = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> DefaultActionCoolDown  = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> DefaultStartOfRoundActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Werewolf parameters
        public NetworkVariable<float> WerewolfInteractRange  = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> WerewolfActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> WerewolfStartOfRoundActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Villager parameters
        public NetworkVariable<float> VillagerInteractRange  = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> VillagerActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> VillagerStartOfRoundActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Witch parameters
        public NetworkVariable<float> WitchInteractRange     = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> WitchActionCoolDown    = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> WitchStartOfRoundActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Seer parameters
        public NetworkVariable<float> SeerInteractRange      = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> SeerActionCooldown     = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> SeerStartOfRoundActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Wild Boy parameters
        public NetworkVariable<float> WildBoyInteractRange   = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> WildBoyActionCoolDown  = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<float> WildBoyStartOfRoundActionCoolDown = new NetworkVariable<float>(0.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);


       

        public override void OnNetworkSpawn()
        {
            logger.LogInfo("Setup Keybinds CallBacks");
            SetupKeybindCallbacks();


            if (IsServer)
            {
                logdebug.LogInfo("Making default roles");
                MakeDefaultRoles();
            }
            QueryCurrentRolesServerRpc();

            logdebug.LogInfo("RolesManager NetworkSpawn");

            if (IsServer)
            {
                // Default 
                DefaultInteractRange.Value = Plugin.config_DefaultInteractRange.Value;
                DefaultActionCoolDown.Value = Plugin.config_DefaultActionCoolDown.Value;
                DefaultStartOfRoundActionCoolDown.Value = Plugin.config_DefaultStartOfRoundActionCoolDown.Value;

                // Werewolf parameters
                WerewolfInteractRange.Value = Plugin.config_WerewolfInteractRange.Value;
                WerewolfActionCoolDown.Value = Plugin.config_WerewolfActionCoolDown.Value;
                WerewolfStartOfRoundActionCoolDown.Value = Plugin.config_WerewolfStartOfRoundActionCoolDown.Value;

                // Villager parameters
                VillagerInteractRange.Value = Plugin.config_VillagerInteractRange.Value;
                VillagerActionCoolDown.Value = Plugin.config_VillagerActionCoolDown.Value;
                VillagerStartOfRoundActionCoolDown.Value = Plugin.config_VillagerStartOfRoundActionCoolDown.Value;

                // Witch parameters
                WitchInteractRange.Value = Plugin.config_WitchInteractRange.Value;
                WitchActionCoolDown.Value = Plugin.config_WitchActionCoolDown.Value;
                WitchStartOfRoundActionCoolDown.Value = Plugin.config_WitchStartOfRoundActionCoolDown.Value;

                // Seer parameters
                SeerInteractRange.Value = Plugin.config_SeerInteractRange.Value;
                SeerActionCooldown.Value = Plugin.config_SeerActionCoolDown.Value;
                SeerStartOfRoundActionCoolDown.Value = Plugin.config_SeerStartOfRoundActionCoolDown.Value;

                // Wild Boy parameters
                WildBoyInteractRange.Value = Plugin.config_WildBoyInteractRange.Value;
                WildBoyActionCoolDown.Value = Plugin.config_WildBoyActionCoolDown.Value;
                WildBoyStartOfRoundActionCoolDown.Value = Plugin.config_WildboyStartOfRoundActionCoolDown.Value;
            }
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep it across scenes if needed
                Plugin.Instance.rolesManager = this;
            }
            else
            {
                logdebug.LogInfo("Duplicate detected, delted the just-created RolesManager");
                Destroy(gameObject); // Prevent duplicate instances
            }

        }

        private void SetupKeybindCallbacks()
        {
            Plugin.InputActionsInstance.MainRoleActionKey.performed += OnRoleMainKeyPressed;
            Plugin.InputActionsInstance.SecondaryRoleActionKey.performed += OnRoleSecondaryKeyPressed;
            Plugin.InputActionsInstance.PopUpRoleActionKey.performed += OnPopUpRoleActionKeyPressed;
            Plugin.InputActionsInstance.DistributeRolesKey.performed += OnDistributeRolesKeyPressed;
        }

        public void OnRoleMainKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (myRole == null) return; // Prevents NullReferenceException
            if (myRole.roleName == null) return; // Prevents the default Role class to use the function
            
            if (!keyContext.performed) return;

            if (!myRole.IsLocallyAllowedToPerformMainAction()) 
            {
                logdebug.LogInfo("I am not locally allowed to perform my Main Action");
                return;
            }
            
            logdebug.LogInfo($"Pressed the key, performing main action for my role {myRole.roleName}");

            PerformMainActionServerRpc();
        }

        public void OnRoleSecondaryKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (myRole == null) return; // Prevents NullReferenceException
            if (myRole.roleName == null) return; // Prevents the default Role class to use the function

            if (!keyContext.performed) return;

            if (!myRole.IsLocallyAllowedToPerformSecondaryAction())
            {
                logdebug.LogInfo("I am not locally allowed to perform my Secondary Action");
                return;
            }

            logdebug.LogInfo($"Pressed the key, performing secondary action for my role {myRole.roleName}");

            PerformSecondaryActionServerRpc();
        }

        public void OnPopUpRoleActionKeyPressed(InputAction.CallbackContext keyContext)
        {
            DisplayMyRolePopUp();
        }

        public void OnDistributeRolesKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (!IsHost) return;
            if (!keyContext.performed) return;
            BuildAndSendRoles();
        }


        public override void OnDestroy()
        {
            base.OnDestroy();
            logger.LogError($"{name} has been destroyed!");
        }


        void Update()
        {
            if (myRole != null)
            {
                myRole.UpdateCooldowns(Time.deltaTime);
            }
        }

#nullable enable
        public PlayerControllerB? CheckForPlayerInRange(ulong myId)
        {
            if (myRole == null)
            {
                throw new Exception("myRole is null in CheckForPlayerInRange. This should have been caught earlier.");
            }

            GameObject playerObject = GetPlayerByNetworkId(myId);
            //mls.LogInfo("Grab the PlayerControllerB");
            PlayerControllerB player = playerObject.GetComponent<PlayerControllerB>();
            //PlayerControllerB player = HUDManager.Instance.localPlayer;
            Camera playerCamera = player.gameplayCamera;


            // Cast rays to check whether another player is in range
            int playerLayerMask = 1 << playerObject.layer;

            Vector3 castDirection = playerCamera.transform.forward.normalized;
            RaycastHit[] pushRay = Physics.RaycastAll(playerCamera.transform.position, castDirection, myRole.interactRange.Value, playerLayerMask);

            RaycastHit[] allHits = Physics.RaycastAll(playerCamera.transform.position, castDirection, myRole.interactRange.Value);
            System.Array.Sort(allHits, (a, b) => (a.distance.CompareTo(b.distance)));

            if (allHits.Length == 0) // No hits found
            {
                return null;
            }
            
            foreach (RaycastHit hit in allHits)
            {
                if (hit.transform.gameObject == playerObject)
                {
                    logdebug.LogInfo("I just saw my own player");
                    //logdebug.LogInfo($"Skipped roles manager");
                    continue;
                }

                //if (hit.transform.gameObject.name.ToLower().Contains("rolesmanager"))
                //{   logdebug.LogInfo($"Skipped roles manager");
                //    continue;
                //}

                if (hit.transform.GetComponent<Collider>() == null)
                {
                    logdebug.LogInfo($"The game object {transform.name} has no collider");
                    continue;
                }


                if (hit.transform.gameObject.GetComponent<Collider>() == null)
                {
                    logdebug.LogInfo($"The game object {transform.gameObject.name} has no collider");
                    continue;
                }

                Collider collider = hit.transform.gameObject.GetComponent<Collider>();
                logdebug.LogInfo($"This is the collider {collider}");
                if (hit.transform.gameObject.layer == playerObject.layer)
                {
                    logdebug.LogInfo($"I see an object : {hit.transform.gameObject.name}, that's probably a player");
                    return hit.transform.gameObject.GetComponent<PlayerControllerB>();
                }

            }
            return null;


            //foreach (RaycastHit hit in pushRay)
            //{
            //    logdebug.LogInfo(hit.transform.gameObject.name);

            //    if (hit.transform.gameObject != playerObject) // note: playerobject is the current player object. Therefore it's returning the PlayerComponentB of any player it's finding, which
            //    {
            //        logdebug.LogInfo($"Found player? {hit.transform.gameObject.name}");
            //        PlayerControllerB hitPlayer = hit.transform.GetComponent<PlayerControllerB>();
            //        return hitPlayer;

            //    }
            //}
            //logdebug.LogInfo($"Did not find a player. My camera was in {playerCamera.transform.position.ToString()}, with direction {castDirection.ToString()}, range = {myRole.interactRange.Value} at layer mask {playerLayerMask.ToString()}");
            return null;
        }
#nullable disable

        private static GameObject GetPlayerByNetworkId(ulong playerId)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject networkObject))
            {
                return networkObject.gameObject;
            }
            return null;
        }

        public PlayerControllerB GetPlayerById(ulong playerId)
        {
            GameObject[] allPlayers = StartOfRound.Instance.allPlayerObjects;
            foreach (GameObject player in allPlayers)
            {
                PlayerControllerB playerController = player.GetComponent<PlayerControllerB>();
                if (playerId == playerController.OwnerClientId)
                {
                    return playerController;
                }
            }
            logger.LogError("Could not find the desired player");
            throw new Exception("Could not find the player");
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuildAndSendRolesServerRpc()
        {
            BuildAndSendRoles();
        }

        public void BuildAndSendRoles()
        {
            
            // Build roles
            Dictionary<ulong, Role> finalRoles;
            logger.LogInfo("Roles generation has started");
            finalRoles = BuildFinalRolesFromScratch();
            logger.LogInfo("Roles generation has finished");

            logdebug.LogInfo($"{finalRoles}");
            logger.LogInfo("Sending roles to each player");
            // Send the role to each player
            foreach (var item in finalRoles)
            {
                logdebug.LogInfo($"Trying to send role {item.Value} to player id {item.Key}");
                
                ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(item.Key);
                logdebug.LogInfo($"Using ClientRpcParams: {clientRpcParams}");


                Utils.PrintDictionary<ulong, Role>(finalRoles);
                logdebug.LogInfo($"{item.Value.refInt}");

                logdebug.LogInfo("Invoking the SendRoleClientRpc method");
                logger.LogInfo($"Sent role to player: {GetPlayerById(item.Key).playerUsername} with id {item.Key}");
                SendRoleClientRpc(item.Value.refInt, clientRpcParams);
            }

            logger.LogInfo("Finished sending roles to each player");

            allRoles = finalRoles;
            logdebug.LogInfo("Stored all roles in RolesManager.");
        }


        public void MakeDefaultRoles()
        {
            currentRolesSetup = References.GetAllRoles();
        }

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

            // Fill roles with the current setup
            for (int i = 0; i < currentRolesSetup.Count; i++)
            {
                roles.Add(currentRolesSetup[i]);
            }

            // Fill remaining slots with Villagers
            for (int i = currentRolesSetup.Count; i < totalPlayers; i++)
            {
                roles.Add(new Villager());
            }

            // Remove the last roles in case there were more roles given than there are players
            for (int i = roles.Count -1; i >= totalPlayers; i--)
            {
                roles.RemoveAt(roles.Count - 1);
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


            logdebug.LogInfo("Show all playerControllers informations");
            foreach (GameObject player in allPlayers)
            {
                PlayerControllerB playerController = player.GetComponent<PlayerControllerB>();
                logdebug.LogInfo($"playerName = {playerController.playerUsername}, playerClientId = {playerController.playerClientId}, actualClientId = {playerController.actualClientId}, OwnerClientId = {playerController.OwnerClientId}, NetworkObjectId = {playerController.NetworkObjectId}, NetworkBehaviourId = {playerController.NetworkBehaviourId}");
            }

            logdebug.LogInfo("Players added to the list for roles distribution");
            // Get the list of players Client Ids
            playersIds = new List<ulong>();
            for (int i = 0; i<Nplayers;i++)
            {
                GameObject player = allPlayers[i];
                PlayerControllerB playerController = player.GetComponent<PlayerControllerB>();
                logdebug.LogInfo($"playerName = {playerController.playerUsername}, playerClientId = {playerController.playerClientId}, actualClientId = {playerController.actualClientId}, OwnerClientId = {playerController.OwnerClientId}, NetworkObjectId = {playerController.NetworkObjectId}, NetworkBehaviourId = {playerController.NetworkBehaviourId}");

                playersIds.Add(playerController.OwnerClientId);
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


        public void DisplayMyRolePopUp()
        {
            if (myRole == null) return;
            logger.LogInfo("Displaying my role tooltip");
            logdebug.LogInfo("Grabbing my Role");
            myRole.DisplayRolePopUp();
        }



        [ClientRpc]
        public void SendRoleClientRpc(int roleInt, ClientRpcParams clientRpcParams = default)
        {
            // Retrieve the role
            logdebug.LogInfo($"Received my role");
            Role role = References.references()[roleInt];
            logdebug.LogInfo($"I was given the role {role} with name {role.roleName} and refInt {role.refInt}");


            // Assign the player's role
            //RolesManager roleManagerObject = FindObjectOfType<RolesManager>();
            myRole = role;


            logdebug.LogInfo("I have succesfully set my own role");

            // Display the tooltip for the role
            DisplayMyRolePopUp();
            logdebug.LogInfo("I have successfully displayed my Role tooltip");

            // Update my own role
            string playerName = GameNetworkManager.Instance.localPlayerController.playerUsername;
            string roleName = myRole.roleName;


            // Locate the RoleHUD and update it
            logdebug.LogInfo("Trying to update HUD");
            RoleHUD roleHUD = FindObjectOfType<RoleHUD>();
            if (roleHUD != null)
            {
                logger.LogInfo("Update the HUD with the role");
                roleHUD.UpdateRoleDisplay();
            }
            else
            {
                logger.LogError("Could not find the RoleHUD");
            }

            logdebug.LogInfo($"I am player {playerName} and I have fully completed and received the role {roleName}");

            // Reset death message to default
            Utils.EditDeathMessage();
        }



        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ServerRpc and ClientRpc roles logic

        // Query Role

        [ServerRpc(RequireOwnership = false)]
        public void QueryAllRolesServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            WrapAllPlayersRoles(out string playersRefsIds, out string rolesRefInts);
            QueryAllRolesClientRpc(playersRefsIds, rolesRefInts, clientRpcParams);
        }

        [ClientRpc]
        public void QueryAllRolesClientRpc(string playersRefsIds, string rolesRefInts, ClientRpcParams clientRpcParams = default)
        {
            allRoles = UnWrapAllPlayersRoles(playersRefsIds, rolesRefInts);
        }

        public void WrapAllPlayersRoles(out string playersRefsIds, out string rolesRefInts)
        {
            List<ulong> playersIds = new List<ulong>();
            List<Role> roles = new List<Role>();

            foreach (var item in allRoles)
            {
                playersIds.Add(item.Key);
                roles.Add(item.Value);
            }

            playersRefsIds = "";
            foreach (ulong id in playersIds)
            {
                playersRefsIds += $"{id}\n";
            }

            rolesRefInts = "";
            foreach (Role role in roles)
            {
                rolesRefInts += $"{role.refInt}\n";
            }
        }


        public Dictionary<ulong, Role> UnWrapAllPlayersRoles(string playersRefsIds, string rolesRefInts)
        {

            Dictionary<ulong, Role> dic = new Dictionary<ulong, Role>();

            string[] roles = rolesRefInts.Split("\n");
            string[] ids   = playersRefsIds.Split("\n");

            for (int i = 0; i < roles.Length; i++)
            {
                if ((roles[i] == "") || (ids[i] == ""))
                {
                    continue;
                }
                logdebug.LogInfo($"Trying to convert: {roles[i]} and {ids[i]}");
                int refInt = Convert.ToInt32(roles[i]);
                ulong playerid = Convert.ToUInt64(ids[i]);
                dic.Add(playerid, References.references()[refInt]);
            }
            return dic;
        }



        // ------------------------------------------------------------------------------------
        // Terminal logic
        [ServerRpc(RequireOwnership = false)]
        public void QueryCurrentRolesServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            UpdateCurrentRolesSetupClientRpc(WrapRolesList(currentRolesSetup), false, clientRpcParams);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateCurrentRolesServerRpc(string newRolesSetup,ServerRpcParams serverRpcParams = default)
        {
            logger.LogInfo($"Roles setup was edited by PlayerSenderId = {serverRpcParams.Receive.SenderClientId}");
            //ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            UpdateCurrentRolesSetupClientRpc(newRolesSetup, true);
        }


        [ClientRpc]
        private void UpdateCurrentRolesSetupClientRpc(string upstreamRolesSetup, bool isUpdate, ClientRpcParams clientRpcParams = default)
        {
            if (isUpdate)
            {
                logger.LogInfo("The roles setup was modified.");
            }
            currentRolesSetup = UnwrapRolesList(upstreamRolesSetup);
        }


        public string WrapRolesList(List<Role> roles)
        {
            string rolesRefInts = "";
            foreach (Role role in roles)
            {
                rolesRefInts += $"{role.refInt}\n";
            }
            return rolesRefInts;

        }


        public List<Role> UnwrapRolesList(string rolesRefInts)
        {
            List<Role> newCurrentRolesSetup = new List<Role>();
            logdebug.LogInfo("Trying to split the list into individual string-ints");
            string[] refInts = rolesRefInts.Split('\n');
            logdebug.LogInfo("Successfully splt the string");

            foreach (string refInt in refInts)
            {
                if (refInt == "")
                {
                    continue;
                }
                newCurrentRolesSetup.Add(References.references()[Convert.ToInt32(refInt)]);
            }
            return newCurrentRolesSetup;
        }


        // ------------------------------------------------------------------------------------
        // Debug 
        // Reset all cooldowns to everyone
        [ServerRpc(RequireOwnership = false)]
        public void ResetAllCooldownsServerRpc()
        {
            ResetAllCooldownsClientRpc();
        }

        [ClientRpc]
        private void ResetAllCooldownsClientRpc()
        {
            if (myRole == null) return;

            myRole.currentMainActionCooldown = 0f;
            myRole.currentSecondaryActionCooldown = 0f;
            HUDManager.Instance.DisplayTip("Admin", "Cooldowns set to 0");
            logger.LogInfo("Cooldowns set to 0");
        }

        [ServerRpc(RequireOwnership =false)]
        public void ResetRolesServerRpc()
        {
            ResetRolesClientRpc();
        }

        [ClientRpc]
        public void ResetRolesClientRpc()
        {
            if (myRole == null) return;
            logger.LogInfo("Resetting my role to its intial state");
            myRole = References.GetRoleByName(myRole.roleName);
            HUDManager.Instance.DisplayTip("Admin", "Role reset to its initial state");
        }


        // ------------------------------------------------------------------------------------
        // Performing Main Action logic

        [ServerRpc(RequireOwnership = false)]
        public void PerformMainActionServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderId = serverRpcParams.Receive.SenderClientId;
            Role senderRole = allRoles[senderId];
            logdebug.LogInfo($"Received action request from Player Id : {senderId}");

            // Build the ClientRpcParams to answer to the caller
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            };

            
            if (senderRole.IsAllowedToPerformMainAction()) // If can perform action, then perform it
            {
                PerformMainActionClientRpc(clientRpcParams); 
            }

            else // Else, notify the sender that he cannot perform his action yet
            {
                CannotPerformThisActionYetClientRpc(clientRpcParams); 
            }
        }


        [ClientRpc]
        public void PerformMainActionClientRpc(ClientRpcParams clientRpcParams = default)
        {

            myRole.GenericPerformMainAction();
        }


        [ClientRpc]
        public void CannotPerformThisActionYetClientRpc(ClientRpcParams clientRpcParams = default)
        {
            HUDManager.Instance.DisplayTip($"{myRole.roleName}", "You cannot perform this action.");
        }

        [ServerRpc(RequireOwnership = false)]
        public void SuccessFullyPerformedMainActionServerRpc(ServerRpcParams serverRpcParams = default)
        {
            logdebug.LogInfo($"Setting Player Id = {serverRpcParams.Receive.SenderClientId} main action on cooldown");
            ulong senderId = serverRpcParams.Receive.SenderClientId;
            allRoles[senderId].SetMainActionOnCooldown();

            // Build the ClientRpcParams to answer to the caller
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            };

            logdebug.LogInfo("Sending the caller the order to set his main action on cooldown");
            SuccessFullyPerformedMainActionClientRpc(clientRpcParams);
        }

        [ClientRpc]
        public void SuccessFullyPerformedMainActionClientRpc(ClientRpcParams clientRpcParams = default)
        {
            logdebug.LogInfo("Setting my main action on cooldown");
            myRole.SetMainActionOnCooldown();
        }



        // ------------------------------------------------------------------------------------
        // Performing Secondary Action logic

        [ServerRpc(RequireOwnership = false)]
        public void PerformSecondaryActionServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ulong senderId = serverRpcParams.Receive.SenderClientId;
            Role senderRole = allRoles[senderId];
            logdebug.LogInfo($"Received secondary action request from Player Id : {senderId}");

            // Build the ClientRpcParams to answer to the caller
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            };


            if (senderRole.IsAllowedToPerformSecondaryAction()) // If can perform action, then perform it
            {
                PerformSecondaryActionClientRpc(clientRpcParams);
            }

            else // Else, notify the sender that he cannot perform his action yet
            {
                CannotPerformThisActionYetClientRpc(clientRpcParams);
            }
        }


        [ClientRpc]
        public void PerformSecondaryActionClientRpc(ClientRpcParams clientRpcParams = default)
        {

            myRole.GenericPerformSecondaryAction();
        }



        [ServerRpc(RequireOwnership = false)]
        public void SuccessFullyPerformedSecondaryActionServerRpc(ServerRpcParams serverRpcParams = default)
        {
            logdebug.LogInfo($"Setting Player Id = {serverRpcParams.Receive.SenderClientId} secondary action on cooldown");
            ulong senderId = serverRpcParams.Receive.SenderClientId;
            allRoles[senderId].SetMainActionOnCooldown();

            // Build the ClientRpcParams to answer to the caller
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { senderId }
                }
            };

            logdebug.LogInfo("Sending the caller the order to set his secondary action on cooldown");
            SuccessFullyPerformedSecondaryActionClientRpc(clientRpcParams);
        }

        [ClientRpc]
        public void SuccessFullyPerformedSecondaryActionClientRpc(ClientRpcParams clientRpcParams = default)
        {
            logdebug.LogInfo("Setting my secondary action on cooldown");
            myRole.SetSecondaryActionOnCooldown();
        }



        // Generalized Notification of Main action success
        [ServerRpc(RequireOwnership = false)]
        public void NotifyMainActionSuccessServerRpc(ulong originId, ServerRpcParams serverRpcParams = default)
        {
            PlayerControllerB targettedPlayer = GetPlayerById(serverRpcParams.Receive.SenderClientId);
            PlayerControllerB originPlayer = GetPlayerById(originId);
            logdebug.LogInfo($"I was notified that the targetted player ({targettedPlayer.playerUsername}) has been affected by the main action of the {allRoles[originId]} {originPlayer.playerUsername}. I therefore notify him.");
            string targetPlayerName = targettedPlayer.playerUsername;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(originId);
            NotifyMainActionSuccessClientRpc(targetPlayerName, clientRpcParams);

        }

        [ClientRpc]
        public void NotifyMainActionSuccessClientRpc(string targetPlayerName, ClientRpcParams clientRpcParams = default)
        {
            myRole.NotifyMainActionSuccess(targetPlayerName);
        }

        // Generalized Notification of Main action fail
        [ServerRpc(RequireOwnership = false)]
        public void NotifyMainActionFailedServerRpc(ulong originId, ServerRpcParams serverRpcParams = default)
        {
            PlayerControllerB targettedPlayer = GetPlayerById(serverRpcParams.Receive.SenderClientId);
            PlayerControllerB originPlayer = GetPlayerById(originId);
            logdebug.LogInfo($"I was notified that the targetted player ({targettedPlayer.playerUsername}) was not affected by the main action of the {allRoles[originId]} {originPlayer.playerUsername}. I therefore notify him.");
            string targetPlayerName = targettedPlayer.playerUsername;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(originId);
            NotifyMainActionFailedClientRpc(targetPlayerName, clientRpcParams);

        }

        [ClientRpc]
        public void NotifyMainActionFailedClientRpc(string targetPlayerName, ClientRpcParams clientRpcParams = default)
        {
            myRole.NotifyMainActionFailed(targetPlayerName);
        }


        // Generalized Notification of Secondary action success
        [ServerRpc(RequireOwnership = false)]
        public void NotifySecondaryActionSuccessServerRpc(ulong originId, ServerRpcParams serverRpcParams = default)
        {
            PlayerControllerB targettedPlayer = GetPlayerById(serverRpcParams.Receive.SenderClientId);
            PlayerControllerB originPlayer = GetPlayerById(originId);
            logdebug.LogInfo($"I was notified that the targetted player ({targettedPlayer.playerUsername}) has been affected by the secondary action of the {allRoles[originId]} {originPlayer.playerUsername}. I therefore notify him.");
            string targetPlayerName = targettedPlayer.playerUsername;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(originId);
            NotifySecondaryActionSuccessClientRpc(targetPlayerName, clientRpcParams);

        }

        [ClientRpc]
        public void NotifySecondaryActionSuccessClientRpc(string targetPlayerName, ClientRpcParams clientRpcParams = default)
        {
            myRole.NotifySecondaryActionSuccess(targetPlayerName);
        }

        // Generalized Notification of Secondary action fail
        [ServerRpc(RequireOwnership = false)]
        public void NotifySecondaryActionFailedServerRpc(ulong originId, ServerRpcParams serverRpcParams = default)
        {
            PlayerControllerB targettedPlayer = GetPlayerById(serverRpcParams.Receive.SenderClientId);
            PlayerControllerB originPlayer = GetPlayerById(originId);
            logdebug.LogInfo($"I was notified that the targetted player ({targettedPlayer.playerUsername}) was not affected by the secondary action of the {allRoles[originId]} {originPlayer.playerUsername}. I therefore notify him.");
            string targetPlayerName = targettedPlayer.playerUsername;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(originId);
            NotifySecondaryActionFailedClientRpc(targetPlayerName, clientRpcParams);

        }

        [ClientRpc]
        public void NotifySecondaryActionFailedClientRpc(string targetPlayerName, ClientRpcParams clientRpcParams = default)
        {
            myRole.NotifySecondaryActionFailed(targetPlayerName);
        }





        // ------------------------------------------------------------------------------------
        // Specific Roles Actions

        // Seer actions
        [ServerRpc(RequireOwnership = false)]
        public void CheckRoleServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            logdebug.LogInfo($"Executing ServerRpc while I am the host: {IsHost || IsServer}");

            //RolesManager roleManagerObject = FindObjectOfType<RolesManager>(); // Load the RolesManager Object
            //logdebug.LogInfo("Grabbed RoleManager");
            ulong senderId = serverRpcParams.Receive.SenderClientId; // Get the sender Id
            logdebug.LogInfo($"Grabbed sender ID: {senderId}");
            //string playerName = GetPlayerById(targetId).GetComponent<PlayerControllerB>().playerUsername;

            string playerName = GetPlayerById(targetId).GetComponent<PlayerControllerB>().playerUsername;
            //string playerName = "test player name";
            int refInt = allRoles[targetId].refInt; // Find the refInt of the desired role
            logdebug.LogInfo($"grabbed refInt of checked role : {refInt}");

            // Build the clientRpcParams to only answer to the caller
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(senderId);

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
            ((Seer)myRole).NotifyMainActionSuccess(playerName,role);
        }




        // Werewolf actions
        [ServerRpc(RequireOwnership = false)]
        public void WerewolfKillPlayerServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            logdebug.LogInfo($"Received Werewolf kill command from {GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername}, towards {GetPlayerById(targetId).playerUsername}");
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(targetId);
            WerewolfKillPlayerClientRpc(serverRpcParams.Receive.SenderClientId, clientRpcParams);
        }



        [ClientRpc]
        private void WerewolfKillPlayerClientRpc(ulong werewolfId, ClientRpcParams clientRpcParams = default)
        {
            logdebug.LogInfo($"Received a Werewolf kill command from the server. Werewolf: {GetPlayerById(werewolfId).playerUsername}");
            // If I don't have a role, skip the check for immunity
            if (myRole != null)
            {
                // If player is immune, remove immunity and notify the werewolf
                if (myRole.isImmune)
                {
                    logdebug.LogInfo("I am immune, therefore I do not die and notify the server");
                    myRole.isImmune = false;
                    NotifyMainActionFailedServerRpc(werewolfId);
                    return;
                }
            }


            // Edit the death screen message
            string message = $"{GetPlayerById(werewolfId).playerUsername.ToUpper()}\nWAS A WEREWOLF";
            Utils.EditDeathMessage(message);

            // Run the kill animation
            logdebug.LogInfo("I am not immune, therefore I run the kill command");
            PlayerControllerB controller = Utils.GetLocalPlayerControllerB();
            controller.KillPlayer(new Vector3(0, 0, 0));
            //HUDManager.Instance.DisplayTip("You were mawled", $"You died from a werewolf: {GetPlayerById(werewolfId).playerUsername}", true);

            

            NotifyMainActionSuccessServerRpc(werewolfId);

        }

        // Witch actions
        // Poison someone
        [ServerRpc(RequireOwnership = false)]
        public void WitchPoisonPlayerServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            string witchName = GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername;
            ulong witchId = serverRpcParams.Receive.SenderClientId;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(targetId);
            WitchPoisonPlayerClientRpc(witchId, clientRpcParams);
        }

        [ClientRpc]
        private void WitchPoisonPlayerClientRpc(ulong witchId, ClientRpcParams clientRpcParams = default)
        {

            // Edit the death screen message
            string message = $"{GetPlayerById(witchId).playerUsername.ToUpper()}\nWAS A WITCH";
            Utils.EditDeathMessage(message);

            PlayerControllerB controller = Utils.GetLocalPlayerControllerB();
            controller.KillPlayer(new Vector3(0, 0, 0));
            //HUDManager.Instance.DisplayTip("You were poisoned", $"You were poisoned by a witch: {GetPlayerById(witchId).playerUsername}", true);
            NotifyMainActionSuccessServerRpc(witchId);
        }


        //[ServerRpc(RequireOwnership = false)]
        //private void NotifyWitchOfPoisonServerRpc(ulong witchId, ServerRpcParams serverRpcParams = default)
        //{
        //    logdebug.LogInfo($"I was notified that the Witch {GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername} has run the kill command, I therefore notify the witch {GetPlayerById(witchId).playerUsername}");
        //    string targetPlayerName = GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername;
        //    ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(witchId);
        //    NotifyWitchOfPoisonClientRpc(targetPlayerName, clientRpcParams);
        //}

        //[ClientRpc]
        //private void NotifyWitchOfPoisonClientRpc(string targetPlayerName, ClientRpcParams clientRpcParams = default)
        //{
        //    ((Witch)myRole).NotifyMainActionSuccess(targetPlayerName);
        //}


        //Immunize someone
        [ServerRpc(RequireOwnership = false)]
        public void WitchImmunizePlayerServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            ulong witchId = serverRpcParams.Receive.SenderClientId;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(targetId);
            WitchImmunizePlayerClientRpc(witchId, clientRpcParams);
        }

        [ClientRpc]
        private void WitchImmunizePlayerClientRpc(ulong witchId, ClientRpcParams clientRpcParams = default)
        {
            myRole.isImmune = true;
            NotifySecondaryActionSuccessServerRpc(witchId);
        }



        // Wild Boy actions
        [ServerRpc(RequireOwnership = false)]
        public void IdolizeServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            IdolizeClientRpc(targetId, clientRpcParams);
        }

        [ClientRpc]
        public void IdolizeClientRpc(ulong targetId, ClientRpcParams clientRpcParams = default)
        {
            ((WildBoy)myRole).NotifyMainActionSuccess(targetId);
        }


        [ServerRpc(RequireOwnership =false)]
        public void OnSomebodyDeathServerRpc(ulong deadId)
        {
            logdebug.LogInfo("Someone just died");
            // Somebody just died, go through all players and check for Wild Boys.
            // If there are wild boys, provide them with the dead ID, and eventually trigger their transformation
            foreach (var item in allRoles)
            {
                if (!(item.Value.GetType() == typeof(WildBoy))) continue;

                ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(item.Key);
                NotifyWildBoyOfDeathClientRpc(deadId, clientRpcParams);
            }
        }


        [ClientRpc]
        public void NotifyWildBoyOfDeathClientRpc(ulong deadId, ClientRpcParams clientRpcParams)
        {
            if (deadId == ((WildBoy)myRole).idolizedId.Value)
            {
                BecomeWerewolfServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void BecomeWerewolfServerRpc(ServerRpcParams serverRpcParams = default)
        {
            allRoles[serverRpcParams.Receive.SenderClientId] = new Werewolf();
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            BecomeWerewolfClientRpc(clientRpcParams);

        }

        [ClientRpc]
        public void BecomeWerewolfClientRpc(ClientRpcParams clientRpcParams = default)
        {
            ((WildBoy)myRole).BecomeWerewolf();
        }
    }
       
}

