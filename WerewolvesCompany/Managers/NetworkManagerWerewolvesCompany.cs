using BepInEx.Logging;
using Unity.Netcode;


namespace WerewolvesCompany.Managers
{
    public class NetworkManagerWerewolvesCompany : NetworkBehaviour
    {
        public static NetworkManagerWerewolvesCompany Instance;

        public ManualLogSource logger = Plugin.Instance.logger;
        public ManualLogSource logdebug = Plugin.Instance.logdebug;

        void Awake()
        {
            Instance = this;
        }
    }
}