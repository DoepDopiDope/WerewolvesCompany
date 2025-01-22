using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib.Tools;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Managers
{
    public class NetworkManagerWerewolvesCompany : NetworkBehaviour
    {
        public static NetworkManagerWerewolvesCompany Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

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
            // Retrieve the role
            logdebug.LogInfo($"Received roleInt {roleInt}");
            Role role = References.references()[roleInt];
            logdebug.LogInfo($"I can see the role : {role} with name {role.roleName} and refInt {role.refInt}");


            // Assign the player's role
            RolesManager.Instance.myRole = role;
            logdebug.LogInfo("I have succesfully set my own role");

            // Display the tooltip for the role
            RolesManager.Instance.DisplayRoleToolTip();
            logdebug.LogInfo("I have successfully displayed my Role tooltip");

            // Locate the RoleHUD and update it
            RoleHUD roleHUD = FindObjectOfType<RoleHUD>();
            if (roleHUD != null)
            {
                roleHUD.UpdateRoleDisplay(role);
            }

            string playerName = GameNetworkManager.Instance.localPlayerController.playerUsername;
            string roleName = RolesManager.Instance.myRole.roleName;
            logdebug.LogInfo("I have succesfully grabbed my playerName and my roleName");

            // Display the tooltip
            RolesManager.Instance.DisplayRoleToolTip();
            logdebug.LogInfo("I have successfully displayed my Role tooltip");

            logger.LogInfo($"I have received the role {roleName}");
            logdebug.LogInfo($"I am player {playerName} and I have fully completed and received the role {roleName}");   
        }

        
        
    }
}