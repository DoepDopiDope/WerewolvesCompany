using System;
using System.Collections.Generic;
using System.Text;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

public class InputsClass : LcInputActions
{
    [InputAction(KeyboardControl.K, Name = "MainRoleActionKey")]
    public InputAction MainRoleActionKey { get; set; }

    [InputAction(KeyboardControl.L, Name = "SecondaryRoleActionKey")]
    public InputAction SecondaryRoleActionKey { get; set; }

    [InputAction(KeyboardControl.P, Name = "DistributeRoles")]
    public InputAction DistributeRolesKey { get; set; }

}