using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using BepInEx.Logging;
using UnityEngine;

namespace WerewolvesCompany.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void checkPlayerNumber(ref int ___connectedPlayersAmount)
        {
            WerewolvesCompanyBase.mls.LogInfo(___connectedPlayersAmount.ToString());
        }
    }
}
