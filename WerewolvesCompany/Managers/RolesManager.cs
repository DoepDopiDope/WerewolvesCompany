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



namespace WerewolvesCompany.Managers
{
    internal class RolesManager : NetworkBehaviour
    {
        
        public RolesManager Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;
        public ManualLogSource logupdate = Plugin.Instance.logupdate;

        public System.Random rng = Plugin.Instance.rng;
        public Dictionary<ulong, Role> allRoles;

        public List<Role> currentRolesSetup = new List<Role>();

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
            MakeDefaultRoles();

            if (IsServer)
            {
                

                // Default parameters
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
            }
            else
            {
                logger.LogInfo("Duplicate detected, delted the just-created RolesManager");
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
        public PlayerControllerB? CheckForPlayerInRange(ulong myId, ManualLogSource mls)
        {
            if (myRole == null)
            {
                throw new Exception("myRole is null in CheckForPlayerInRange. This should have been caught earlier.");
            }

            //ulong myId = NetworkObjectId;
            mls.LogInfo($"My networkObjectId is {myId}");


            mls.LogInfo("Grab the playerObject");
            GameObject playerObject = GetPlayerByNetworkId(myId);
            mls.LogInfo("Grab the PlayerControllerB");
            PlayerControllerB player = playerObject.GetComponent<PlayerControllerB>();
            //PlayerControllerB player = HUDManager.Instance.localPlayer;
            mls.LogInfo("Grab the Camera");
            Camera playerCamera = player.gameplayCamera;


            // Cast rays to check whether another player is in range
            mls.LogInfo("Grab the layer");
            int playerLayerMask = 1 << playerObject.layer;

            mls.LogInfo("Cast rays");
            Vector3 castDirection = playerCamera.transform.forward.normalized;
            RaycastHit[] pushRay = Physics.RaycastAll(playerCamera.transform.position, castDirection, myRole.interactRange.Value, playerLayerMask);
            foreach (RaycastHit hit in pushRay)
            {
                if (hit.transform.gameObject != playerObject)
                {
                    PlayerControllerB hitPlayer = hit.transform.GetComponent<PlayerControllerB>();
                    return hitPlayer;

                }
            }
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
                if (playerId == player.GetComponent<PlayerControllerB>().actualClientId)
                {
                    return player.GetComponent<PlayerControllerB>();
                }
            }
            logger.LogError("Could not find the desired player");
            throw new Exception("Could not find the player");
        }


        public void BuildAndSendRoles()
        {
            logger.LogInfo("Roles generation has started");


            // Build roles
            Dictionary<ulong, Role> finalRoles;
            finalRoles = BuildFinalRolesFromScratch();
            logger.LogInfo("Roles generation has finished");

            logdebug.LogInfo($"{finalRoles}");
            logger.LogInfo("Sending roles to each player");
            // Send the role to each player
            foreach (var item in finalRoles)
            {
                logdebug.LogInfo($"Trying to send role {item.Value} to player id {item.Key}");

                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { item.Key }
                    }
                };
                logdebug.LogInfo($"Using ClientRpcParams: {clientRpcParams}");


                Utils.PrintDictionary<ulong, Role>(finalRoles);
                logdebug.LogInfo($"{item.Value.refInt}");

                logdebug.LogInfo("Invoking the SendRoleClientRpc method");
                SendRoleClientRpc(item.Value.refInt, clientRpcParams);
            }

            logger.LogInfo("Finished sending roles to each player");

            allRoles = finalRoles;
            logdebug.LogInfo("Stored all roles in RolesManager.");
        }


        public List<Role> MakeDefaultRoles()
        {
            return References.GetAllRoles();
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
            logdebug.LogInfo($"Received roleInt {roleInt}");
            Role role = References.references()[roleInt];
            logdebug.LogInfo($"I can see the role : {role} with name {role.roleName} and refInt {role.refInt}");


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
                roleHUD.UpdateRoleDisplay(role);
            }
            else
            {
                logger.LogInfo("Did not find the HUD");
            }

            logdebug.LogInfo($"I am player {playerName} and I have fully completed and received the role {roleName}");
        }



        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ServerRpc and ClientRpc roles logic




        // ------------------------------------------------------------------------------------
        // Terminal logic
        [ServerRpc(RequireOwnership = false)]
        public void QueryCurrentRolesServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            UpdateCurrentRolesSetupClientRpc(WrapRolesList(currentRolesSetup), clientRpcParams);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateCurrentRolesServerRpc(string newRolesSetup,ServerRpcParams serverRpcParams = default)
        {
            //ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            UpdateCurrentRolesSetupClientRpc(newRolesSetup);
        }


        [ClientRpc]
        private void UpdateCurrentRolesSetupClientRpc(string upstreamRolesSetup, ClientRpcParams clientRpcParams = default)
        {
            logger.LogInfo("The roles setup was modified.");
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
            // If player is immune, remove immunity and notify the werewolf
            if (myRole.isImmune)
            {
                logdebug.LogInfo("I am immune, therefore I do not die and notify the server");
                myRole.isImmune = false;
                NotifyMainActionFailedServerRpc(werewolfId);
                return;
            }

            logdebug.LogInfo("I am not immune, therefore I run the kill command");
            string werewolfName = GetPlayerById(werewolfId).playerUsername;
            PlayerControllerB controller = Utils.GetLocalPlayerControllerB();
            controller.KillPlayer(new Vector3(0, 0, 0));
            HUDManager.Instance.DisplayTip("You were mawled", $"You died from a werewolf: {werewolfName}");

            NotifyMainActionSuccessServerRpc(werewolfId);
        }

        //[ServerRpc(RequireOwnership = false)]
        //public void NotifyWerewolfOfImmunityServerRpc(ulong werewolfId, ServerRpcParams serverRpcParams = default)
        //{
        //    logdebug.LogInfo($"I was notified that {GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername} is immune, I therefore notify the werewolf {GetPlayerById(werewolfId).playerUsername}");
        //    string targetPlayerName = GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername;
        //    ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(werewolfId);
        //    NotifyWerewolfOfImmunityClientRpc(targetPlayerName, clientRpcParams);
        //}

        //[ClientRpc]
        //private void NotifyWerewolfOfImmunityClientRpc(string targetPlayerName, ClientRpcParams clientRpcParams = default)
        //{
        //    ((Werewolf)myRole).NotifyMainActionFailed(targetPlayerName);
        //}

        //[ServerRpc(RequireOwnership = false)]
        //private void NotifyWerewolfOfKillServerRpc(ulong werewolfId, ServerRpcParams serverRpcParams = default)
        //{
        //    logdebug.LogInfo($"I was notified that the Werewolf {GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername} has run the kill command, I therefore notify the werewolf {GetPlayerById(werewolfId).playerUsername}");
        //    string targetPlayerName = GetPlayerById(serverRpcParams.Receive.SenderClientId).playerUsername;
        //    ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(werewolfId);
        //    NotifyWerewolfOfKillClientRpc(targetPlayerName, clientRpcParams);
        //}

        //[ClientRpc]
        //private void NotifyWerewolfOfKillClientRpc(string targetPlayerName, ClientRpcParams clientRpcParams = default)
        //{
        //    ((Werewolf)myRole).NotifyMainActionSuccess(targetPlayerName);
        //}



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
            PlayerControllerB controller = Utils.GetLocalPlayerControllerB();
            controller.KillPlayer(new Vector3(0, 0, 0));
            HUDManager.Instance.DisplayTip("You were poisoned", $"You were poisoned by a witch: {GetPlayerById(witchId).playerUsername}");
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
            if (deadId == ((WildBoy)Utils.GetRolesManager().myRole).idolizedId.Value)
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

