using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Logging;

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
    }
}
