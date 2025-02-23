using System;
using System.Collections.Generic;
using System.Text;
using Coroner;
using UnityEngine;

namespace WerewolvesCompany
{

    class CustomDeaths
    {

        public static Dictionary<string, AdvancedCauseOfDeath> references => GetReferences();

        // Custom deaths
        public const string WEREWOLF_KEY = "Werewolf";
        public static AdvancedCauseOfDeath WEREWOLF = Coroner.API.Register(WEREWOLF_KEY);

        public const string WITCH_KEY = "Witch";
        public static AdvancedCauseOfDeath WITCH = Coroner.API.Register(WITCH_KEY);

        public const string LOVER_KEY = "Lover";
        public static AdvancedCauseOfDeath LOVER = Coroner.API.Register(LOVER_KEY);


        public static Dictionary<string, AdvancedCauseOfDeath>  GetReferences()
        {
            Dictionary<string, AdvancedCauseOfDeath> dic = new Dictionary<string, AdvancedCauseOfDeath>();
            dic.Add(WEREWOLF_KEY, WEREWOLF);
            dic.Add(WITCH_KEY, WITCH);
            dic.Add(LOVER_KEY, LOVER);

            return dic;
        }



    }



}
