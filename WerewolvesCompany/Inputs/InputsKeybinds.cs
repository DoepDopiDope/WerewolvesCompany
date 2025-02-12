using System;
using System.Collections.Generic;
using System.Text;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;


namespace WerewolvesCompany.Inputs
{
    public class InputsKeybinds : LcInputActions
    {
        // Roles keys
        [InputAction(KeyboardControl.Z, Name = "MainRoleActionKey")]
        public InputAction MainRoleActionKey { get; set; }

        [InputAction(KeyboardControl.V, Name = "SecondaryRoleActionKey")]
        public InputAction SecondaryRoleActionKey { get; set; }

        [InputAction(KeyboardControl.M, Name = "PopUpRole")]
        public InputAction PopUpRoleActionKey { get; set; }

        [InputAction(KeyboardControl.P, Name = "DistributeRoles", KbmInteractions = "hold(duration = 5)")]
        public InputAction DistributeRolesKey { get; set; }



        // Vote keys
        [InputAction(KeyboardControl.N, Name = "Open/Close Voting Window")]
        public InputAction OpenCloseVotingWindow { get; set; }

        [InputAction(KeyboardControl.UpArrow, Name = "VoteScrollUp")]
        public InputAction VoteScrollUp { get; set; }

        [InputAction(KeyboardControl.DownArrow, Name = "VoteScrollDown")]
        public InputAction VoteScrollDown { get; set; }

        [InputAction(KeyboardControl.Enter, Name = "CastVote")]
        public InputAction CastVote { get; set; }



    }
}