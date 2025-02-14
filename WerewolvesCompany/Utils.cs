using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;
using GameNetcodeStuff;
using JetBrains.Annotations;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using WerewolvesCompany.Managers;

namespace WerewolvesCompany
{
    static class Utils
    {
        static public ManualLogSource logger => Plugin.Instance.logger;
        static public ManualLogSource logdebug => Plugin.Instance.logdebug;
        static public RolesManager rolesManager => Plugin.Instance.rolesManager;
        static public PlayerControllerB localController => StartOfRound.Instance?.localPlayerController;

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
            return StartOfRound.Instance?.localPlayerController;
            //return localController;
        }

        static public RolesManager GetRolesManager()
        {
            return rolesManager;
        }

        static public void EditDeathMessage(string message = "[LIFE SUPPORT: OFFLINE]")
        {
            GameObject val = GameObject.Find("Systems/UI/Canvas/DeathScreen/GameOverText");
            TextMeshProUGUI component = val.GetComponent<TextMeshProUGUI>();
            ((TMP_Text)component).text = message;
        }

        static public int Modulo(int a, int b)
        {
            return (a % b + b) % b;
        }


        static public bool AreThereAliveVillagers()
        {
            foreach (PlayerControllerB controller in StartOfRound.Instance.allPlayerScripts)
            {
                if (!rolesManager.allRoles.ContainsKey(controller.OwnerClientId)) continue;

                if (controller.IsSpawned && !controller.isPlayerDead && rolesManager.allRoles.ContainsKey(controller.OwnerClientId))
                {
                    if (rolesManager.allRoles[controller.OwnerClientId].team == "Village")
                    {
                        return true;
                    }
                }
            }
            return false; 
        }
    }
}
