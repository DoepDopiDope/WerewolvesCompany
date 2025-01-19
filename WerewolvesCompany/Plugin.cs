using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace WerewolvesCompany
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class WerewolvesCompanyBase : BaseUnityPlugin
    {
        private const string modGUID = "Doep.WerewolvesCompany";
        private const string modName = "WerewolvesCompany";
        private const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        private static WerewolvesCompanyBase Instance;

        internal ManualLogSource mls;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("WerewolvesCompany has awaken.");

            harmony.PatchAll(typeof(WerewolvesCompanyBase));
        }
    }
}
