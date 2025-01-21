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


        // Send Roles to players
        [ServerRpc(RequireOwnership = false)]
        public void SendRoleServerRpc(ulong id, Role role)
        {
            SendRoleClientRpc(id, role);
        }

        [ClientRpc]
        public void SendRoleClientRpc(ulong id, Role role)
        {
            //HUDManager.Instance.DisplayTip("Test", "Message received");

            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;

            if (!(player.playerClientId == id))
            {
                return;
            }

            string playerName = player.playerUsername;
            string roleName = role.roleName;
            logger.LogInfo($"Sending role {roleName} to player {playerName}");

            HUDManager.Instance.DisplayTip($"To {playerName}",$"Your role is {roleName}");
        }
        
    }
}