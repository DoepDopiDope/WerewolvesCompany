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

namespace WerewolvesCompany.Managers
{
    internal class RolesManager : MonoBehaviour
    {
        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        public System.Random rng = Plugin.Instance.rng;
        public static RolesManager Instance;

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
                Destroy(gameObject); // Prevent duplicate instances
            }
        }

        void OnDestroy()
        {
            logger.LogError($"{name} has been destroyed!");
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

            // Example logic: One Werewolf and the rest are Villagers
            roles.Add(new Werewolf());
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

            

            //foreach (GameObject player in allPlayers)
            //{
            //    ulong playerId = player.GetComponent<PlayerControllerB>().actualClientId;
            //    string playerName = player.GetComponent<PlayerControllerB>().playerUsername;
            //    logger.LogInfo($"Added playerName {playerName} with id {playerId.ToString()} to the list");

            //    playersIds.Add(playerId);
            //}


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
            Role role = RolesManager.Instance.myRole;
            logdebug.LogInfo("Grab the PlayerControllerB instance");
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;



            //if (!(player.playerClientId == id))
            //{
            //    logger.LogInfo($"Skipping : Current player id {player.playerClientId}, Target id {id}, role {role}");
            //    return;
            //}


            logdebug.LogInfo("Grab the playerUsername from the HUDManager instance");
            //string playerName = HUDManager.Instance.localPlayer.playerUsername;
            string playerName = player.playerUsername;

            logdebug.LogInfo("Grab the role name from the role name itself");
            string roleName = role.roleName;
            string winCondition = role.winCondition;
            string roleDescription = role.roleDescription;

            logdebug.LogInfo("Display the role using the HUDManager.Instance.DisplayTip method");
            HUDManager.Instance.DisplayTip($"You are a {roleName}", $"{winCondition}\n{roleDescription}");
            return role;

        }


        

    }
    
}

