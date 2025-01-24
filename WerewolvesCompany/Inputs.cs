using System;
using System.Collections.Generic;
using System.Text;
using LethalCompanyInputUtils.Api;
using LethalCompanyInputUtils.BindingPathEnums;
using UnityEngine.InputSystem;

public class InputsClass : LcInputActions
{
    [InputAction(KeyboardControl.K, Name = "RoleActionKey")]
    public InputAction RoleActionKey { get; set; }

    [InputAction(KeyboardControl.P, Name = "DistributeRoles")]
    public InputAction DistributeRolesKey { get; set; }

}