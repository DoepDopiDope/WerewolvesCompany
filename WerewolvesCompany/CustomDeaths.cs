using System;
using System.Collections.Generic;
using System.Text;
using Coroner;

namespace WerewolvesCompany
{
    static class CustomDeaths
    {
        // Werewolf
        const string WEREWOLF_LANGUAGE_KEY = "Werewolf";
        public static AdvancedCauseOfDeath WEREWOLF = Coroner.API.Register(WEREWOLF_LANGUAGE_KEY);

         // Witch
        const string WITCH_LANGUAGE_KEY = "Witch";
        public static AdvancedCauseOfDeath WITCH = Coroner.API.Register(WITCH_LANGUAGE_KEY);

         // Lover
        const string LOVER_LANGUAGE_KEY = "Lover";
        public static AdvancedCauseOfDeath LOVER = Coroner.API.Register(LOVER_LANGUAGE_KEY);
    }
}
