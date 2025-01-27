using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameNetcodeStuff;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.Managers;

namespace WerewolvesCompany
{
    static class Utils
    {
        static public ManualLogSource logger = Plugin.Instance.logger;
        static public ManualLogSource logdebug = Plugin.Instance.logdebug;

        static public void PrintDictionary<T1, T2>(Dictionary<T1, T2> dictionary)
        {
            foreach (var item in dictionary)
            {
                logdebug.LogInfo($"{item.Key} > {item.Value}");
            }
        }

        static public ClientRpcParams BuildClientRpcParams(ulong targetId)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetId }
                }
            };
            return clientRpcParams;
        }

        static public PlayerControllerB GetLocalPlayerControllerB()
        {
            PlayerControllerB player = StartOfRound.Instance?.localPlayerController;
            return player;
        }

        static public RolesManager GetRolesManager()
        {
            RolesManager roleManagerObject = RolesManager.FindObjectOfType<RolesManager>();
            return roleManagerObject;
        }

        static public void EditDeathMessage(string message = "[LIFE SUPPORT: OFFLINE]")
        {
            GameObject val = GameObject.Find("Systems/UI/Canvas/DeathScreen/GameOverText");
            TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
            ((TMP_Text)component).text = message;
        }
    }
}
