using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx.Logging;
using GameNetcodeStuff;
using JetBrains.Annotations;
using UnityEngine.InputSystem;
using WerewolvesCompany.Managers;
using WerewolvesCompany.UI;

namespace WerewolvesCompany.Inputs
{
    static class KeybindsLogic
    {
        static public RolesManager rolesManager => Plugin.Instance.rolesManager;
        static public RoleHUD roleHUD => Plugin.Instance.roleHUD;
        static public Role myRole => Plugin.Instance.rolesManager.myRole;

        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;
        static public ManualLogSource logupdate = Plugin.Instance.logupdate;

        static public bool IsHost => rolesManager.IsHost;
        static public PlayerControllerB localController => Utils.GetLocalPlayerControllerB();


        // ----------------------------------------------------------------------------
        // Roles Methods
        static public void OnRoleMainKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (myRole == null) return; // Prevents NullReferenceException
            if (myRole.roleName == null) return; // Prevents the default Role class to use the function

            if (!keyContext.performed) return;

            if (!myRole.hasMainAction) return;

            if (!myRole.IsLocallyAllowedToPerformMainAction())
            {
                logdebug.LogInfo("I am not locally allowed to perform my Main Action");
                return;
            }

            logdebug.LogInfo($"Pressed the key, performing main action for my role {myRole.roleName}");

            rolesManager.PerformMainActionServerRpc();
        }

        static public void OnRoleSecondaryKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (myRole == null) return; // Prevents NullReferenceException
            if (myRole.roleName == null) return; // Prevents the default Role class to use the function

            if (!keyContext.performed) return;

            if (!myRole.hasSecondaryAction) return;

            if (!myRole.IsLocallyAllowedToPerformSecondaryAction())
            {
                logdebug.LogInfo("I am not locally allowed to perform my Secondary Action");
                return;
            }

            logdebug.LogInfo($"Pressed the key, performing secondary action for my role {myRole.roleName}");

            rolesManager.PerformSecondaryActionServerRpc();
        }

        static public void OnPopUpRoleActionKeyPressed(InputAction.CallbackContext keyContext)
        {
            rolesManager.DisplayMyRolePopUp();
        }

        static public void OnDistributeRolesKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (!IsHost) return;
            if (!keyContext.performed) return;
            rolesManager.BuildAndSendRoles();
        }


        // ----------------------------------------------------------------------------
        // Vote Methods

        static public void OnOpenCloseVotingWindowKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (rolesManager == null || roleHUD == null) return;
            if (localController.inTerminalMenu || localController.isPlayerDead) return;
            logdebug.LogInfo("Toggling vote window On/Off");
            roleHUD.OpenCloseVoteTab();
        }

        static public void OnVoteScrollUpKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (rolesManager == null || roleHUD == null) return;
            if (!roleHUD.voteWindowContainer.activeSelf || localController.inTerminalMenu) return;

            roleHUD.voteWindowSelectedPlayer = Utils.Modulo(roleHUD.voteWindowSelectedPlayer - 1, rolesManager.allPlayersList.Count);
            logdebug.LogInfo($"Selected player {roleHUD.voteWindowSelectedPlayer}");
            
            
        }

        static public void OnVoteScrollDownKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (rolesManager == null || roleHUD == null) return;
            if (!roleHUD.voteWindowContainer.activeSelf || localController.inTerminalMenu) return;
            
            roleHUD.voteWindowSelectedPlayer = Utils.Modulo(roleHUD.voteWindowSelectedPlayer + 1, rolesManager.allPlayersList.Count);
            logdebug.LogInfo($"Selected player {roleHUD.voteWindowSelectedPlayer}");
            
        }

        static public void OnCastVoteKeyPressed(InputAction.CallbackContext keyContext)
        {
            if (rolesManager == null || roleHUD == null) return;
            if (rolesManager.allPlayersIds == null) return;
            if (rolesManager.isVoteOnCooldown || !roleHUD.voteWindowContainer.activeSelf || localController.inTerminalMenu || localController.isPlayerDead) return;

            if (roleHUD.voteCastedPlayer != roleHUD.voteWindowSelectedPlayer)
            {
                roleHUD.voteCastedPlayer = roleHUD.voteWindowSelectedPlayer;
            }
            else
            {
                roleHUD.voteCastedPlayer = null;
            }

            if (roleHUD.voteCastedPlayer == null)
            {
                rolesManager.CastVoteServerRpc();
            }
            else
            {
                ulong castPlayerId = rolesManager.allPlayersIds[roleHUD.voteCastedPlayer.Value];
                rolesManager.CastVoteServerRpc(castPlayerId);
            }
            
        }


    }
}
