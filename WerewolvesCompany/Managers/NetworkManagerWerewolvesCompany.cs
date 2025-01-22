using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib.Tools;
using Unity.Netcode;
using UnityEngine;

namespace WerewolvesCompany.Managers
{
    public class NetworkManagerWerewolvesCompany : NetworkBehaviour
    {
        public static NetworkManagerWerewolvesCompany Instance;

        public ManualLogSource logger = Plugin.instance.logger;
        public ManualLogSource logdebug = Plugin.instance.logdebug;

        void Awake()
        {
            Instance = this;
        }


        // Send death notification to everyone
        [ServerRpc(RequireOwnership = false)]
        public void RequestDeathNotificationServerRpc(ulong id)
        {
            DeathNotificationClientRpc(id);
        }


        [ClientRpc]
        public void DeathNotificationClientRpc(ulong id)
        {
            string name = StartOfRound.Instance.allPlayerObjects[(int)id].GetComponent<PlayerControllerB>().playerUsername;
            ;
            HUDManager.Instance.DisplayTip("Player Has Died", $"{name} is now dead");
            
        }


        [ServerRpc(RequireOwnership = false)]
        public void SimpleTipDisplayServerRpc()
        {
            SimpleTipDisplayClientRpc();
        }

        [ClientRpc]
        public void SimpleTipDisplayClientRpc()
        {
            logdebug.LogInfo("Displaying a simple tip");
            HUDManager.Instance.DisplayTip("Test Header", "Test Text");
            logdebug.LogInfo("Displayed a simple tip");
        }


        // Send Roles to players
        //[ServerRpc(RequireOwnership = false)]
        //public void SendRoleServerRpc(Role role)
        //{
        //    SendRoleClientRpc(role);
        //}

        [ClientRpc]
        public void SendRoleClientRpc(int roleInt, ClientRpcParams clientRpcParams = default)
        {

            Dictionary<int, Role> references;
            Role role;
            
            references = References.references();
            role = references[roleInt];


            logdebug.LogInfo("Testing if I received the command");
            logdebug.LogInfo($"I can see the role : {role} with name {role.roleName} and refInt {role.refInt}");
            HUDManager.Instance.DisplayTip("Test", "Message received");

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

            logdebug.LogInfo("Display the role using the HUDManager.Instance.DisplayTip method");
            HUDManager.Instance.DisplayTip($"To {playerName}", $"Your role is {roleName}");

            logdebug.LogInfo($"I am player {playerName} and I have fully completed and received the role {roleName}");

            
        }

        
    }
}