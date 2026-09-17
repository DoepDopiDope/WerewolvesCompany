using System;
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;
using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

using WerewolvesCompany.UI;
using WerewolvesCompany.Inputs;
using WerewolvesCompany.Config;
using Coroner;
using TMPro;



namespace WerewolvesCompany.Managers
{
    class RolesManager : NetworkBehaviour
    {
        
        public static RolesManager Instance { get; private set; }
        private static bool keybindCallbacksRegistered;
        public RoleHUD roleHUD => Plugin.Instance.roleHUD;
        public QuotaManager quotaManager => Plugin.Instance.quotaManager;
        public ConfigManager configManager => Plugin.Instance.configManager;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public System.Random rng = Plugin.Instance.rng;
        public Dictionary<ulong, Role> allRoles;
        public Dictionary<ulong,string> allPlayersList;
        public List<ulong> allPlayersIds;

        public Dictionary<ulong, ulong?> allPlayersVotes;

        public List<Role> currentRolesSetup = new List<Role>();

        public float voteKillCurrentCooldown = 0f;
        public bool isVoteOnCooldown => (voteKillCurrentCooldown > 0);
        public bool hasAlreadyDistributedRolesThisRound = false;
        private readonly HashSet<ulong> processedDeaths = new HashSet<ulong>();
        private readonly RaycastHit[] targetingHits = new RaycastHit[32];
        //public Role spectatedPlayerRole;

#nullable enable
        public Role? myRole { get; set; }
#nullable disable


        public bool onlyWerewolvesALive => (!Utils.AreThereAliveVillagers());



        public override void OnNetworkSpawn()
        {
            logdebug.LogInfo("RolesManager NetworkSpawn");
            if (!keybindCallbacksRegistered)
            {
                logger.LogInfo("Setup Keybinds CallBacks");
                SetupKeybindCallbacks();
                keybindCallbacksRegistered = true;
            }


            if (IsServer)
            {
                logdebug.LogInfo("Making default roles");
                MakeDefaultRoles();
            }

            QueryCurrentRolesServerRpc(); 
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Plugin.Instance.rolesManager = this;
            }
            else
            {
                logdebug.LogInfo("Duplicate detected, delted the just-created RolesManager");
                Destroy(gameObject); // Prevent duplicate instances
            }
        }

        void Update()
        {
            if (myRole != null)
            {
                myRole.UpdateCooldowns(Time.deltaTime);
            }

            // Update cooldown
            voteKillCurrentCooldown -= Time.deltaTime;

        }



        private void SetupKeybindCallbacks()
        {
            
            // Roles Keys
            Plugin.InputActionsInstance.MainRoleActionKey.performed += KeybindsLogic.OnRoleMainKeyPressed;
            Plugin.InputActionsInstance.SecondaryRoleActionKey.performed += KeybindsLogic.OnRoleSecondaryKeyPressed;
            Plugin.InputActionsInstance.PopUpRoleActionKey.performed += KeybindsLogic.OnPopUpRoleActionKeyPressed;
            Plugin.InputActionsInstance.DistributeRolesKey.performed += KeybindsLogic.OnDistributeRolesKeyPressed;

            // Vote Keys
            Plugin.InputActionsInstance.OpenCloseVotingWindow.performed += KeybindsLogic.OnOpenCloseVotingWindowKeyPressed;
            Plugin.InputActionsInstance.VoteScrollUp.performed += KeybindsLogic.OnVoteScrollUpKeyPressed;
            Plugin.InputActionsInstance.VoteScrollDown.performed += KeybindsLogic.OnVoteScrollDownKeyPressed;
            Plugin.InputActionsInstance.CastVote.performed += KeybindsLogic.OnCastVoteKeyPressed;
        }

        




        public override void OnDestroy()
        {
            base.OnDestroy();
            if (Instance == this)
            {
                Instance = null;
            }
            if (Plugin.Instance != null && Plugin.Instance.rolesManager == this)
            {
                Plugin.Instance.rolesManager = null;
            }
            logdebug.LogDebug($"{name} has been destroyed.");
        }


        public void DisplayWinningTeam()
        {
            var components = HUDManager.Instance.endgameStatsAnimator.gameObject.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var comp in components)
            {
                if (comp.name == "HeaderText")
                {
                    if (onlyWerewolvesALive)
                    {
                        comp.text = "WEREWOLVES HAVE WON";
                    }
                    else
                    {
                        comp.text = "VILLAGERS HAVE WON";
                    }
                    return;
                }   
            }
            throw new Exception("Did not find the Header Text");
        }

        

#nullable enable
        public PlayerControllerB? CheckForPlayerInRange(ulong myId)
        {
            if (myRole == null)
            {
                throw new Exception("myRole is null in CheckForPlayerInRange. This should have been caught earlier.");
            }
            return CheckForPlayerInRange(myId, myRole.interactRange);
        }

        public PlayerControllerB? CheckForPlayerInRange(ulong myId, float checkRange)
        {
            if (myRole == null)
            {
                throw new Exception("myRole is null in CheckForPlayerInRange. This should have been caught earlier.");
            }

            GameObject playerObject = GetPlayerByNetworkId(myId);
            if (playerObject == null || !playerObject.TryGetComponent(out PlayerControllerB player)) return null;
            //PlayerControllerB player = HUDManager.Instance.localPlayer;
            Camera playerCamera = player.gameplayCamera;
            if (playerCamera == null) return null;


            // Cast rays to check whether another player is in range
            UnityEngine.Vector3 castDirection = playerCamera.transform.forward.normalized;
            int hitCount = Physics.RaycastNonAlloc(playerCamera.transform.position, castDirection, targetingHits, checkRange);
            PlayerControllerB? closestPlayer = null;
            float closestPlayerDistance = float.MaxValue;
            float closestBlockingDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = targetingHits[i];
                Collider hitCollider = hit.collider;
                if (hitCollider == null) continue;

                Transform hitTransform = hitCollider.transform;
                if (hitTransform == null) continue;

                GameObject checkedObject = hitTransform.gameObject;
                if (checkedObject == null) continue;

                PlayerControllerB hitPlayer = hitTransform.GetComponentInParent<PlayerControllerB>();
                if (hitPlayer != null)
                {
                    if (hitPlayer == player || hitPlayer.isPlayerDead || !hitPlayer.isPlayerControlled) continue;
                    if (hit.distance < closestPlayerDistance)
                    {
                        closestPlayer = hitPlayer;
                        closestPlayerDistance = hit.distance;
                    }
                    continue;
                }

                // Skip line of sight
                if (hitCollider.name.IndexOf("lineofsight", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    continue;
                }
                Renderer renderer = checkedObject.GetComponent<Renderer>();
                if (renderer != null && renderer.enabled && renderer.isVisible && hit.distance < closestBlockingDistance)
                    closestBlockingDistance = hit.distance;
            }
            return closestPlayerDistance < closestBlockingDistance ? closestPlayer : null;
        }
#nullable disable

        private static GameObject GetPlayerByNetworkId(ulong playerId)
        {
            NetworkManager networkManager = NetworkManager.Singleton;
            if (networkManager == null || networkManager.SpawnManager == null) return null;

            if (networkManager.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject networkObject) &&
                networkObject != null)
            {
                return networkObject.gameObject;
            }
            return null;
        }

        public PlayerControllerB GetPlayerById(ulong playerId)
        {
            if (TryGetPlayerById(playerId, out PlayerControllerB player)) return player;
            logger.LogError($"Could not find player {playerId}");
            throw new KeyNotFoundException($"Could not find player {playerId}");
        }

        public bool TryGetPlayerById(ulong playerId, out PlayerControllerB player)
        {
            player = null;
            if (StartOfRound.Instance == null || StartOfRound.Instance.allPlayerScripts == null) return false;
            foreach (PlayerControllerB candidate in StartOfRound.Instance.allPlayerScripts)
            {
                if (candidate != null && playerId == candidate.OwnerClientId)
                {
                    player = candidate;
                    return true;
                }
            }
            return false;
        }

        public List<PlayerControllerB> GetActivePlayers()
        {
            List<PlayerControllerB> players = new List<PlayerControllerB>();
            HashSet<ulong> seenIds = new HashSet<ulong>();
            if (StartOfRound.Instance == null || StartOfRound.Instance.allPlayerScripts == null) return players;
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                if (player == null || !player.isPlayerControlled || !player.IsSpawned) continue;
                if (seenIds.Add(player.OwnerClientId)) players.Add(player);
            }
            return players;
        }

        [ServerRpc(RequireOwnership = false)]
        public void BuildAndSendRolesServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            BuildAndSendRoles();
        }

        public void BuildAndSendRoles()
        {
            if (!IsServer) return;
            processedDeaths.Clear();
            // Build roles
            Dictionary<ulong, Role> finalRoles;
            logger.LogInfo("Roles generation has started");
            finalRoles = BuildFinalRolesFromScratch();
            foreach (Role role in finalRoles.Values)
            {
                role.InitiateCooldowns();
            }
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

            allRoles = finalRoles;
            logdebug.LogInfo("Stored all roles in RolesManager.");
            

            // Initiate the votes
            ResetVotes();
            ResetVoteCooldownClientRpc();

            // Send all roles list to all players
            // I know that that the previous loop is redundant with this line, but I added this line later on, so whatever...
            logdebug.LogInfo("Trying to send allRoles to all players");
            QueryAllRolesFromServer(sendToAllPlayers: true);
            logdebug.LogInfo("Sent allRoles to all players");

            logger.LogInfo("Finished sending roles to each player");
            hasAlreadyDistributedRolesThisRound = true;


        }

        

        public void MakeDefaultRoles()
        {
            currentRolesSetup = References.GetAllRoles();
        }

        // Automatically gathers the number of players
        public List<Role> GenerateRoles()
        {
            return GenerateRoles(GetActivePlayers().Count);
        }

        // Specified number of players
        public List<Role> GenerateRoles(int totalPlayers)
        {
            List<Role> roles = new List<Role>();

            // Fill roles with the current setup
            for (int i = 0; i < currentRolesSetup.Count; i++)
            {
                roles.Add(References.CreateRole(currentRolesSetup[i].refInt));
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
            List<PlayerControllerB> allPlayers;
            List<Role> roles;
            List<ulong> playersIds;


            logdebug.LogInfo("Getting the list of all connected players");
            // Get list of all players
            allPlayers = GetActivePlayers();
            int Nplayers = allPlayers.Count;

            string stringnames = $"Found {Nplayers} players : ";
            for (int i = 0; i < Nplayers;i++)
            {
                string name = allPlayers[i].playerUsername;
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
            foreach (PlayerControllerB playerController in allPlayers)
            {
                logdebug.LogInfo($"playerName = {playerController.playerUsername}, playerClientId = {playerController.playerClientId}, actualClientId = {playerController.actualClientId}, OwnerClientId = {playerController.OwnerClientId}, NetworkObjectId = {playerController.NetworkObjectId}, NetworkBehaviourId = {playerController.NetworkBehaviourId}");
            }

            logdebug.LogInfo("Players added to the list for roles distribution");
            // Get the list of players Client Ids
            playersIds = new List<ulong>();
            for (int i = 0; i<Nplayers;i++)
            {
                PlayerControllerB playerController = allPlayers[i];
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


        public void BecomeRole(string roleName, bool keepInteractions)
        {
            Role role = References.GetRoleByName(roleName);
            RolesInteractions interactions = myRole.interactions;
            myRole = role;
            myRole.InitiateCooldowns();
            if (keepInteractions) myRole.interactions = interactions;
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
            if (!References.TryCreateRole(roleInt, out Role role))
            {
                logger.LogError($"Received unknown role reference {roleInt}.");
                return;
            }
            logdebug.LogInfo($"I was given the role {role} with name {role.roleName} and refInt {role.refInt}");


            // Assign the player's role
            myRole = role;
            myRole.InitiateCooldowns();


            logdebug.LogInfo("I have succesfully set my own role");

            // Display the tooltip for the role
            DisplayMyRolePopUp();
            logdebug.LogInfo("I have successfully displayed my Role tooltip");

            // Reset my lover status
            myRole.interactions.isInLoveWith = null;


            // Locate the RoleHUD and update it
            logdebug.LogInfo("Trying to update HUD");
            RoleHUD roleHUD = this.roleHUD;
            if (roleHUD != null)
            {
                logger.LogInfo("Update the HUD with the role");
                roleHUD.UpdateRoleDisplay();
                roleHUD.roleTextContainer.SetActive(true);
            }
            else
            {
                logger.LogError("Could not find the RoleHUD");
            }

            string playerName = GameNetworkManager.Instance.localPlayerController.playerUsername;
            string roleName = myRole.roleName;
            logdebug.LogInfo($"I am player {playerName} and I have fully completed and received the role {roleName}");

            // Reset death message to default
            Utils.EditDeathMessage();
            hasAlreadyDistributedRolesThisRound = true;
        }


        public void ResetVotes()
        {
            Dictionary<ulong, ulong?> newVotes = new Dictionary<ulong, ulong?>();

            if (allRoles == null)
            {
                allPlayersVotes = newVotes;
                return;
            }
            foreach (var item in allRoles)
            {
                newVotes.Add(item.Key, null);
            }

            allPlayersVotes = newVotes;
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetVoteCooldownServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            ResetVoteCooldownClientRpc();
        }

        [ClientRpc]
        public void ResetVoteCooldownClientRpc(ClientRpcParams clientRpcParams = default)
        {
            ResetVoteCooldown();
        }

        public void ResetVoteCooldown()
        {
            voteKillCurrentCooldown = 0f;
        }

        public ulong? CheckForVotedPlayer()
        {
            Dictionary<ulong,int> votesResults = new Dictionary<ulong,int>();

            // Check which players are alive
            //logdebug.LogInfo("===================================");
            //logdebug.LogInfo("Build the dictionary");
            foreach (PlayerControllerB controller in GetActivePlayers())
            {
                if (!controller.isPlayerDead)
                {
                    votesResults[controller.OwnerClientId] = 0;
                }
            }

            // Check for existing votes
            foreach (var item in allPlayersVotes)
            {
                if (item.Value != null && votesResults.ContainsKey(item.Key))
                {
                    if (votesResults.ContainsKey(item.Value.Value))
                    {
                        votesResults[item.Value.Value] += 1;
                    }
                }
            }

            // Check if some player should be voted off
            //logdebug.LogInfo("Check fo rvoted off player");
            foreach (var item in votesResults)
            {
                if ((((float)item.Value) / ((float)votesResults.Count)) > configManager.VoteAmount.Value)
                {
                    return item.Key;
                }
            }
            return null;


        }

        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ---------------------------------------------------------------------------------------------------------------
        // ServerRpc and ClientRpc roles logic

        //---------------------------------------------
        // Quota logic


        [ServerRpc(RequireOwnership = false)]
        public void AddQuotaValueServerRpc(int scrapValue, ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId || scrapValue < 0) return;
            AddQuotaValueClientRpc(scrapValue);
        }

        [ClientRpc]
        public void AddQuotaValueClientRpc(int scrapValue, ClientRpcParams clientRpcParams = default)
        {
            quotaManager.AddScrapValue(scrapValue);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetNewDailyQuotaServerRpc(int newQuota, ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId || newQuota < 0) return;
            SetNewDailyQuotaClientRpc(newQuota);
        }
        [ClientRpc]
        public void SetNewDailyQuotaClientRpc(int newQuota, ClientRpcParams clientRpcParams = default)
        {
            quotaManager.SetNewDailyQuota(newQuota);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetCurrentQuotaValueServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            ResetCurrentQuotaValueClientRpc();
        }
        [ClientRpc]
        public void ResetCurrentQuotaValueClientRpc(ClientRpcParams clientRpcParams = default)
        {
            quotaManager.ResetScrapValue();
        }


        [ServerRpc(RequireOwnership = false)]
        public void CheatQuotaServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            CheatQuotaClientRpc();
        }

        [ClientRpc]
        public void CheatQuotaClientRpc(ClientRpcParams clientRpcParams = default)
        {
            quotaManager.CheatValue();
            HUDManager.Instance.DisplayTip("Admin", "Daily Quota has been set to it's required value.");
        }



        //---------------------------------------------
        // Voting logic
        [ServerRpc(RequireOwnership = false)]
        public void CastVoteServerRpc(ulong voteId, ServerRpcParams serverRpcParams = default)
        {
            CastVoteGeneric(voteId, serverRpcParams.Receive.SenderClientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void CastVoteServerRpc(ServerRpcParams serverRpcParams = default)
        {
            CastVoteGeneric(null, serverRpcParams.Receive.SenderClientId);
        }


        public void CastVoteGeneric(ulong? voteId, ulong voterId)
        {
            if (!IsServer || allPlayersVotes == null || allRoles == null || isVoteOnCooldown) return;
            if (!allPlayersVotes.ContainsKey(voterId) || !TryGetPlayerById(voterId, out PlayerControllerB voter)) return;
            if (!voter.IsSpawned || !voter.isPlayerControlled || voter.isPlayerDead) return;

            if (voteId.HasValue)
            {
                if (!allRoles.ContainsKey(voteId.Value) || !TryGetPlayerById(voteId.Value, out PlayerControllerB target)) return;
                if (!target.IsSpawned || !target.isPlayerControlled || target.isPlayerDead) return;
            }

            allPlayersVotes[voterId] = voteId;
            ResolveVotesIfNeeded();
        }

        private void ResolveVotesIfNeeded()
        {
            if (isVoteOnCooldown || allPlayersVotes == null) return;
            ulong? votedPlayer = CheckForVotedPlayer();
            if (!votedPlayer.HasValue) return;

            SetVoteKillOnCooldown();
            VoteKillPlayerClientRpc(Utils.BuildClientRpcParams(votedPlayer.Value));
            NotifyAllPlayersOfVoteKillClientRpc(votedPlayer.Value);
            ResetVotes();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetPlayerVoteServerRpc(ServerRpcParams serverRpcParams = default)
        {
            ResetPlayerVote(serverRpcParams.Receive.SenderClientId);
        }

        public void ResetPlayerVote(ulong playerId)
        {
            if (allPlayersVotes != null && allPlayersVotes.ContainsKey(playerId))
            {
                allPlayersVotes[playerId] = null;
                if (IsServer) ResolveVotesIfNeeded();
            }
        }

        public void SetVoteKillOnCooldown()
        {
            voteKillCurrentCooldown = configManager.VoteCooldown.Value;
        }

        [ClientRpc]
        public void VoteKillPlayerClientRpc(ClientRpcParams clientRpcParams)
        {
            // Edit the death screen message
            string message = $"YOU HAVE BEEN VOTED OFF";
            Utils.EditDeathMessage(message);

            // Run the kill animation
            logger.LogInfo("I have been voted off");
            PlayerControllerB controller = Utils.GetLocalPlayerControllerB();
            controller.KillPlayer(new UnityEngine.Vector3(0, 0, 0));
        }

        [ClientRpc]
        public void NotifyAllPlayersOfVoteKillClientRpc(ulong votedPlayer)
        {
            string playerName = GetPlayerById(votedPlayer).playerUsername;
            HUDManager.Instance.DisplayTip("Vote Kill", $"{playerName} has been vote killed",isWarning:true);
            roleHUD.voteCastedPlayer = null; // reset vote
            roleHUD.voteWindowContainer.SetActive(false); // close voting window
            // Set vote kill on cooldown
            SetVoteKillOnCooldown();
        }

        //--------------------------------
        // Query role logic 
        [ServerRpc(RequireOwnership = false)]
        public void QueryAllRolesServerRpc(bool sendToAllPlayers = false, ServerRpcParams serverRpcParams = default)
        {
            if (sendToAllPlayers && serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            QueryAllRolesFromServer(sendToAllPlayers, serverRpcParams);
        }

        // Conveniently wrapped this code snippet in its own method so I can call it from other places where only the Server should be.
        public void QueryAllRolesFromServer(bool sendToAllPlayers = false, ServerRpcParams serverRpcParams = default)
        {
            if (!IsServer)
            {
                throw new Exception("I am not the server, I should have never been allowed to run this method. Doep, check your code, you suck.");
            }

            WrapAllPlayersRoles(out string playersRefsIds, out string rolesRefInts);

            if (sendToAllPlayers) // If command was invoked asking to update roles to everyone
            {
                QueryAllRolesClientRpc(playersRefsIds, rolesRefInts);
            }
            else // If command was called from a specific client
            {
                ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
                QueryAllRolesClientRpc(playersRefsIds, rolesRefInts, clientRpcParams);
            }
        }

        [ClientRpc]
        public void QueryAllRolesClientRpc(string playersRefsIds, string rolesRefInts, ClientRpcParams clientRpcParams = default)
        {
            logdebug.LogInfo($"I received all the roles. I am the role Manager of name: {Instance.name}");
            allRoles = UnWrapAllPlayersRoles(playersRefsIds, rolesRefInts);
            allPlayersIds = GetAllPlayersIds();
            allPlayersList = GetAllPlayersIdsNamesDic();
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

            StringBuilder playerIdsBuilder = new StringBuilder();
            foreach (ulong id in playersIds)
            {
                playerIdsBuilder.Append(id).Append('\n');
            }

            StringBuilder roleIdsBuilder = new StringBuilder();
            foreach (Role role in roles)
            {
                roleIdsBuilder.Append(role.refInt).Append('\n');
            }
            playersRefsIds = playerIdsBuilder.ToString();
            rolesRefInts = roleIdsBuilder.ToString();
        }


        public Dictionary<ulong, Role> UnWrapAllPlayersRoles(string playersRefsIds, string rolesRefInts)
        {

            Dictionary<ulong, Role> dic = new Dictionary<ulong, Role>();

            string[] roles = rolesRefInts.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            string[] ids = playersRefsIds.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (roles.Length != ids.Length || roles.Length > 128)
            {
                logger.LogWarning("Rejected malformed player-role data.");
                return dic;
            }

            for (int i = 0; i < roles.Length; i++)
            {
                if (!int.TryParse(roles[i], out int refInt) ||
                    !ulong.TryParse(ids[i], out ulong playerId) ||
                    !References.TryCreateRole(refInt, out Role role) ||
                    dic.ContainsKey(playerId))
                {
                    logger.LogWarning("Skipped invalid player-role entry received from the server.");
                    continue;
                }
                logdebug.LogInfo($"Trying to convert: {roles[i]} and {ids[i]}");
                dic.Add(playerId, role);
            }
            return dic;
        }

        public List<ulong> GetAllPlayersIds()
        {
            List<ulong> ids = new List<ulong>();
            if (allRoles == null) return ids;
            foreach (PlayerControllerB player in GetActivePlayers())
            {
                if (!player.isPlayerDead && allRoles.ContainsKey(player.OwnerClientId)) ids.Add(player.OwnerClientId);
            }

            return ids;
        }

        public Dictionary<ulong, string> GetAllPlayersIdsNamesDic()
        {
            Dictionary <ulong, string> dic = new Dictionary<ulong, string>();
            if (allRoles == null) return dic;
            foreach (PlayerControllerB player in GetActivePlayers())
            {
                if (!player.isPlayerDead && allRoles.ContainsKey(player.OwnerClientId)) dic[player.OwnerClientId] = player.playerUsername;
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
            if (quotaManager != null)
                SyncQuotaClientRpc(quotaManager.currentScrapValue, quotaManager.requiredDailyQuota, clientRpcParams);
        }

        [ClientRpc]
        private void SyncQuotaClientRpc(int currentValue, int requiredValue, ClientRpcParams clientRpcParams = default)
        {
            if (quotaManager == null) return;
            quotaManager.currentScrapValue = currentValue;
            quotaManager.requiredDailyQuota = requiredValue;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UpdateCurrentRolesServerRpc(string newRolesSetup,ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            logger.LogInfo($"Roles setup was edited by PlayerSenderId = {serverRpcParams.Receive.SenderClientId}");
            currentRolesSetup = UnwrapRolesList(newRolesSetup);
            UpdateCurrentRolesSetupClientRpc(WrapRolesList(currentRolesSetup), true);
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
            StringBuilder rolesRefInts = new StringBuilder();
            foreach (Role role in roles)
            {
                rolesRefInts.Append(role.refInt).Append('\n');
            }
            return rolesRefInts.ToString();

        }


        public List<Role> UnwrapRolesList(string rolesRefInts)
        {
            List<Role> newCurrentRolesSetup = new List<Role>();
            logdebug.LogInfo("Trying to split the list into individual string-ints");
            string[] refInts = rolesRefInts.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            logdebug.LogInfo("Successfully splt the string");

            if (refInts.Length > 100)
            {
                logger.LogWarning("Rejected a role setup with more than 100 entries.");
                return newCurrentRolesSetup;
            }

            foreach (string refInt in refInts)
            {
                if (!int.TryParse(refInt, out int roleId) || !References.TryCreateRole(roleId, out Role role))
                {
                    logger.LogWarning($"Skipped invalid role reference '{refInt}'.");
                    continue;
                }
                newCurrentRolesSetup.Add(role);
            }
            return newCurrentRolesSetup;
        }


        // ------------------------------------------------------------------------------------
        // Debug 
        // Reset all cooldowns to everyone
        [ServerRpc(RequireOwnership = false)]
        public void ResetAllCooldownsServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            ResetAllCooldownsClientRpc();
        }

        [ClientRpc]
        private void ResetAllCooldownsClientRpc()
        {
            if (myRole == null) return;

            myRole.currentMainActionCooldown = 0f;
            myRole.currentSecondaryActionCooldown = 0f;
            HUDManager.Instance.DisplayTip("Admin", "Cooldowns set to 0");
            logger.LogInfo("Role cooldowns set to 0");

            ResetVoteCooldown();
            logger.LogInfo("Vote cooldown set to 0");

        }

        [ServerRpc(RequireOwnership =false)]
        public void ResetRolesServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            ResetRolesClientRpc();
        }

        [ClientRpc]
        public void ResetRolesClientRpc()
        {
            if (myRole == null) return;
            logger.LogInfo("Resetting my role to its intial state");
            myRole = References.GetRoleByName(myRole.roleName);
            myRole.InitiateCooldowns();
            HUDManager.Instance.DisplayTip("Admin", "Role reset to its initial state");
        }


        // ------------------------------------------------------------------------------------
        // Performing Main Action logic

        //[ServerRpc(RequireOwnership = false)]
        //public void PerformMainActionServerRpc(ServerRpcParams serverRpcParams = default)
        //{
        //    ulong senderId = serverRpcParams.Receive.SenderClientId;
        //    Role senderRole = allRoles[senderId];
        //    logdebug.LogInfo($"Received action request from Player Id : {senderId}");

        //    // Build the ClientRpcParams to answer to the caller
        //    ClientRpcParams clientRpcParams = new ClientRpcParams
        //    {
        //        Send = new ClientRpcSendParams
        //        {
        //            TargetClientIds = new ulong[] { senderId }
        //        }
        //    };

            
        //    if (senderRole.IsAllowedToPerformMainAction()) // If can perform action, then perform it
        //    {
        //        PerformMainActionClientRpc(clientRpcParams);
        //    }

        //    else // Else, notify the sender that he cannot perform his action yet
        //    {
        //        CannotPerformThisActionYetClientRpc(clientRpcParams); 
        //    }
        //}


        //[ClientRpc]
        //public void PerformMainActionClientRpc(ClientRpcParams clientRpcParams = default)
        //{

        //    myRole.GenericPerformMainAction();
        //}


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
            if (allRoles == null || !allRoles.TryGetValue(senderId, out Role senderRole)) return;
            senderRole.SetMainActionOnCooldown();

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
        public void SuccessFullyPerformedSecondaryActionServerRpc(ServerRpcParams serverRpcParams = default)
        {
            logdebug.LogInfo($"Setting Player Id = {serverRpcParams.Receive.SenderClientId} secondary action on cooldown");
            ulong senderId = serverRpcParams.Receive.SenderClientId;
            if (allRoles == null || !allRoles.TryGetValue(senderId, out Role senderRole)) return;
            senderRole.SetSecondaryActionOnCooldown();

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
            
            myRole.SetSecondaryActionOnCooldown();
        }



        // Generalized Notification of Main action success
        [ServerRpc(RequireOwnership = false)]
        public void NotifyMainActionSuccessServerRpc(ulong originId, ServerRpcParams serverRpcParams = default)
        {
            if (allRoles == null || !allRoles.ContainsKey(originId) ||
                !TryGetPlayerById(serverRpcParams.Receive.SenderClientId, out PlayerControllerB targettedPlayer) ||
                !TryGetPlayerById(originId, out PlayerControllerB originPlayer)) return;
            logdebug.LogInfo($"I was notified that the targetted player ({targettedPlayer.playerUsername}) has been affected by the main action of the {allRoles[originId]} {originPlayer.playerUsername}. I therefore notify him.");
            ulong targetPlayerId = targettedPlayer.OwnerClientId;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(originId);
            NotifyMainActionSuccessClientRpc(targetPlayerId, clientRpcParams);
        }

        [ClientRpc]
        public void NotifyMainActionSuccessClientRpc(ulong targetPlayerId, ClientRpcParams clientRpcParams = default)
        {
            myRole.NotifyMainActionSuccess(targetPlayerId);
        }

        // Generalized Notification of Main action fail
        [ServerRpc(RequireOwnership = false)]
        public void NotifyMainActionFailedServerRpc(ulong originId, ServerRpcParams serverRpcParams = default)
        {
            if (allRoles == null || !allRoles.ContainsKey(originId) ||
                !TryGetPlayerById(serverRpcParams.Receive.SenderClientId, out PlayerControllerB targettedPlayer) ||
                !TryGetPlayerById(originId, out PlayerControllerB originPlayer)) return;
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
            if (allRoles == null || !allRoles.ContainsKey(originId) ||
                !TryGetPlayerById(serverRpcParams.Receive.SenderClientId, out PlayerControllerB targettedPlayer) ||
                !TryGetPlayerById(originId, out PlayerControllerB originPlayer)) return;
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
            if (allRoles == null || !allRoles.ContainsKey(originId) ||
                !TryGetPlayerById(serverRpcParams.Receive.SenderClientId, out PlayerControllerB targettedPlayer) ||
                !TryGetPlayerById(originId, out PlayerControllerB originPlayer)) return;
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



        [ServerRpc(RequireOwnership = false)]
        [SerializeField]
        public void NotifyDeathServerRpc(string causeOfDeathName, ServerRpcParams serverRpcParams = default)
        {
            NotifyDeathClientRpc(serverRpcParams.Receive.SenderClientId, causeOfDeathName);
        }


        [ClientRpc]
        [SerializeField]
        public void NotifyDeathClientRpc(ulong deadId, string causeOfDeathName, ClientRpcParams clientRpcParams = default)
        {
            if (TryGetPlayerById(deadId, out PlayerControllerB deadPlayer) &&
                CustomDeaths.references.TryGetValue(causeOfDeathName, out AdvancedCauseOfDeath cause))
            {
                Coroner.API.SetCauseOfDeath(deadPlayer, cause);
            }
            
        }

        // ------------------------------------------------------------------------------------
        // Specific Roles Actions


        // -------------
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

            if (!TryGetPlayerById(targetId, out PlayerControllerB targetPlayer)) return;
            string playerName = targetPlayer.playerUsername;
            //string playerName = "test player name";
            if (!allRoles.TryGetValue(targetId, out Role targetRole)) return;
            int refInt = targetRole.refInt; // Find the refInt of the desired role
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
            if (!References.TryCreateRole(refInt, out Role role)) return;
            logdebug.LogInfo("Reversed the refInt into a Role");
            ((Seer)myRole).NotifyMainActionSuccess(playerName,role);
        }



        // -------------
        // Werewolf actions
        [ServerRpc(RequireOwnership = false)]
        public void WerewolfKillPlayerServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            if (!TryGetPlayerById(serverRpcParams.Receive.SenderClientId, out PlayerControllerB werewolf) ||
                !TryGetPlayerById(targetId, out PlayerControllerB target)) return;
            logdebug.LogInfo($"Received Werewolf kill command from {werewolf.playerUsername}, towards {target.playerUsername}");
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
                if (myRole.interactions.isImmune)
                {
                    logdebug.LogInfo("I am immune, therefore I do not die and notify the server");
                    myRole.interactions.isImmune = false;
                    NotifyMainActionFailedServerRpc(werewolfId);
                    return;
                }
            }


            // Edit the death screen message
            string message = $"<color=red>{GetPlayerById(werewolfId).playerUsername.ToUpper()}</color>\nWAS A WEREWOLF";
            Utils.EditDeathMessage(message);

            // Run the kill animation
            logdebug.LogInfo("I am not immune, therefore I run the kill command");
            PlayerControllerB controller = Utils.GetLocalPlayerControllerB();
            controller.KillPlayer(new UnityEngine.Vector3(0, 0, 0));

            NotifyMainActionSuccessServerRpc(werewolfId);
            NotifyDeathServerRpc(CustomDeaths.WEREWOLF_KEY);
            

        }


        // -------------
        // Witch actions
        // Poison someone
        [ServerRpc(RequireOwnership = false)]
        public void WitchPoisonPlayerServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            ulong witchId = serverRpcParams.Receive.SenderClientId;
            if (!TryGetPlayerById(witchId, out _) || !TryGetPlayerById(targetId, out _)) return;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(targetId);
            WitchPoisonPlayerClientRpc(witchId, clientRpcParams);
        }

        [ClientRpc]
        private void WitchPoisonPlayerClientRpc(ulong witchId, ClientRpcParams clientRpcParams = default)
        {

            if (myRole.GetType() == typeof(DrunkenMan))
            {
                NotifyMainActionFailedServerRpc(witchId);
                ((DrunkenMan)myRole).NotifyOldLadyStrongBeverage(witchId);
                return;
            }

            // Edit the death screen message
            string message = $"{GetPlayerById(witchId).playerUsername.ToUpper()}\nWAS A WITCH";
            Utils.EditDeathMessage(message);

            PlayerControllerB controller = Utils.GetLocalPlayerControllerB();
            controller.KillPlayer(new UnityEngine.Vector3(0, 0, 0));
            NotifyMainActionSuccessServerRpc(witchId);
            NotifyDeathServerRpc(CustomDeaths.WITCH_KEY);
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
            if (!TryGetPlayerById(witchId, out _) || !TryGetPlayerById(targetId, out _)) return;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(targetId);
            WitchImmunizePlayerClientRpc(witchId, clientRpcParams);
        }

        [ClientRpc]
        private void WitchImmunizePlayerClientRpc(ulong witchId, ClientRpcParams clientRpcParams = default)
        {
            myRole.interactions.isImmune = true;
            NotifySecondaryActionSuccessServerRpc(witchId);
        }




        // -------------
        // Wild Boy actions
        [ServerRpc(RequireOwnership = false)]
        public void IdolizeServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            if (!TryGetPlayerById(serverRpcParams.Receive.SenderClientId, out _) || !TryGetPlayerById(targetId, out _)) return;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            IdolizeClientRpc(targetId, clientRpcParams);
        }

        [ClientRpc]
        public void IdolizeClientRpc(ulong targetId, ClientRpcParams clientRpcParams = default)
        {
            ((WildBoy)myRole).NotifyMainActionSuccess(targetId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void OnSomebodyDeathServerRpc(ulong deadId, ServerRpcParams serverRpcParams = default)
        {
            if (deadId != serverRpcParams.Receive.SenderClientId) return;
            if (!TryGetPlayerById(deadId, out PlayerControllerB deadPlayer) || !deadPlayer.isPlayerDead) return;
            if (!processedDeaths.Add(deadId)) return;

            logdebug.LogInfo("Someone just died");
            // Somebody just died, notify everyone so they can do their stuff
            OnSomebodyDeathClientRpc(deadId);

            // Reset his vote to null
            ResetPlayerVote(deadId);
        }


        [ClientRpc]
        public void OnSomebodyDeathClientRpc(ulong deadId)
        {
            logdebug.LogInfo("I was notified that somebody died");
            allPlayersIds = GetAllPlayersIds();
            allPlayersList = GetAllPlayersIdsNamesDic();
            allPlayersIds.Remove(deadId);
            allPlayersList.Remove(deadId);
            if (roleHUD != null)
            {
                roleHUD.voteWindowSelectedPlayer = allPlayersIds.Count == 0
                    ? 0
                    : Utils.Modulo(roleHUD.voteWindowSelectedPlayer, allPlayersIds.Count);
            }
            if (myRole == null || Utils.localController == null || Utils.localController.isPlayerDead) return;
            // Check for Wild Boy
            if (myRole.GetType() == typeof(WildBoy))
            {
                ulong? idolizedId = ((WildBoy)myRole).idolizedId;
                if (idolizedId.HasValue && deadId == idolizedId.Value)
                {
                    BecomeWerewolfServerRpc(myRole.roleName);
                }
            }

            // Check for lovers
            if (myRole.interactions.isInLoveWith != null)
            {
                if (myRole.interactions.isInLoveWith.Value == deadId)
                {
                    // Edit the death screen message
                    string message = $"<color=#ff00ffff>{GetPlayerById(deadId).playerUsername.ToUpper()}</color>\nHAS DIED";
                    Utils.EditDeathMessage(message);

                    Utils.localController.KillPlayer(new UnityEngine.Vector3(0, 0, 0));
                    NotifyDeathServerRpc(CustomDeaths.LOVER_KEY);


                }
            }
        }


        [ServerRpc(RequireOwnership = false)]
        public void BecomeWerewolfServerRpc(string reason, ServerRpcParams serverRpcParams = default)
        {
            if (allRoles == null || !allRoles.ContainsKey(serverRpcParams.Receive.SenderClientId)) return;
            Role werewolfRole = new Werewolf();
            werewolfRole.InitiateCooldowns();
            allRoles[serverRpcParams.Receive.SenderClientId] = werewolfRole;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(serverRpcParams.Receive.SenderClientId);
            BecomeWerewolfClientRpc(reason, clientRpcParams);
            QueryAllRolesFromServer(sendToAllPlayers: true);

        }

        [ClientRpc]
        public void BecomeWerewolfClientRpc(string reason, ClientRpcParams clientRpcParams = default)
        {
            if (reason == "Wild Boy")
            {
                if (myRole.roleName == "Wild Boy")
                {
                    ((WildBoy)myRole).BecomeWerewolf();
                }
            }
        }



        // -------------
        // Cupid actions
        [ServerRpc(RequireOwnership = false)]
        public void CupidRomancePlayerServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            if (!TryGetPlayerById(serverRpcParams.Receive.SenderClientId, out PlayerControllerB cupid) ||
                !TryGetPlayerById(targetId, out PlayerControllerB target)) return;
            logdebug.LogInfo($"Received Cupid romance command from {cupid.playerUsername}, towards {target.playerUsername}");
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(targetId);
            CupidRomancePlayerClientRpc(serverRpcParams.Receive.SenderClientId, clientRpcParams);
        }



        [ClientRpc]
        private void CupidRomancePlayerClientRpc(ulong cupidId, ClientRpcParams clientRpcParams = default)
        {
            logdebug.LogInfo($"Received a Cupid Romance command from the server. Cupid: {GetPlayerById(cupidId).playerUsername}");


            NotifyMainActionSuccessServerRpc(cupidId);

        }

        [ServerRpc(RequireOwnership =false)]
        public void CupidSendLoversTheirLoverServerRpc(ulong firstLoverId, ulong secondLoverId,  ServerRpcParams serverRpcParams = default)
        {
            ulong cupidId = serverRpcParams.Receive.SenderClientId;
            if (!TryGetPlayerById(cupidId, out _) || !TryGetPlayerById(firstLoverId, out _) || !TryGetPlayerById(secondLoverId, out _)) return;
            // Send to first lover
            ClientRpcParams firstLoverclientRpcParams = Utils.BuildClientRpcParams(firstLoverId);
            CupidSendLoversTheirLoverClientRpc(cupidId, secondLoverId, firstLoverclientRpcParams);

            // Send to second lover
            ClientRpcParams secondLoverclientRpcParams = Utils.BuildClientRpcParams(secondLoverId);
            CupidSendLoversTheirLoverClientRpc(cupidId, firstLoverId, secondLoverclientRpcParams);
        }

        [ClientRpc]
        public void CupidSendLoversTheirLoverClientRpc(ulong cupidId, ulong myLoverId, ClientRpcParams clientRpcParams = default)
        {
            myRole.interactions.isInLoveWith = myLoverId;
            HUDManager.Instance.DisplayTip("<color=#ff00ffff>Cupid</color> put its fate upon you", $"You fell deeply in love with <color=#ff00ffff>{GetPlayerById(myLoverId).playerUsername}</color>. You must win together");
            AnswerToCupidServerRpc(cupidId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void AnswerToCupidServerRpc(ulong cupidId, ServerRpcParams serverRpcParams = default)
        {
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(cupidId);
            AnswerToCupidClientRpc(clientRpcParams);
        }

        [ClientRpc]
        public void AnswerToCupidClientRpc(ClientRpcParams clientRpcParams = default)
        {
            ((Cupid)myRole).CheckForCallBackOfLover();
        }



        // Alpha Werewolf actions
        [ServerRpc(RequireOwnership =false)]
        public void AlphaWerewolfTransformToWerewolfServerRpc(ulong targetId, ServerRpcParams serverRpcParams = default)
        {
            ulong alphaId = serverRpcParams.Receive.SenderClientId;
            if (allRoles == null || !allRoles.ContainsKey(alphaId) || !TryGetPlayerById(targetId, out _)) return;
            ClientRpcParams clientRpcParams = Utils.BuildClientRpcParams(targetId);
            Role werewolfRole = new Werewolf();
            werewolfRole.InitiateCooldowns();
            allRoles[targetId] = werewolfRole;
            AlphaWerewolfTransformToWerewolfClientRpc(alphaId, clientRpcParams);
            QueryAllRolesFromServer(sendToAllPlayers: true);
        }
        
        [ClientRpc]
        public void AlphaWerewolfTransformToWerewolfClientRpc(ulong alphaId, ClientRpcParams clientRpcParams = default)
        {
            HUDManager.Instance.DisplayTip("<color=red>Werewolf</color>", $"The Alpha Werewolf has turned you into a <color=red>Werewolf</color>.");

            BecomeRole("Werewolf", true);

            // Set role cd to 30s
            myRole.currentMainActionCooldown = configManager.AlphaWerewolfCooldownAfterTransform.Value;

            NotifyMainActionSuccessServerRpc(alphaId);
        }


    }
       
}

