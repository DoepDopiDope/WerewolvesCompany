﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BepInEx.Logging;
using JetBrains.Annotations;
using UnityEngine.InputSystem;
using WerewolvesCompany.Managers;

namespace WerewolvesCompany.Inputs
{
    static class KeybindsLogic
    {
        static public RolesManager rolesManager => Plugin.Instance.rolesManager;
        static public Role myRole => Plugin.Instance.rolesManager.myRole;

        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;
        static public ManualLogSource logupdate = Plugin.Instance.logupdate;

        static public bool IsHost => rolesManager.IsHost;


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
            return;
        }

        static public void OnVoteScrollUpKeyPressed(InputAction.CallbackContext keyContext)
        {
            return;
        }

        static public void OnVoteScrollDownKeyPressed(InputAction.CallbackContext keyContext)
        {
            return;
        }

        static public void OnCastVoteKeyPressed(InputAction.CallbackContext keyContext)
        {
            return;
        }


    }
}
