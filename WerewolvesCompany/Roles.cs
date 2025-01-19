using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WerewolvesCompany
{
    public class Role
    {
        public bool isVillager;
    }

    public class Villager : Role
    {
        new public bool isVillager = true;
    }

    public class Werewolf : Role
    {
        new public bool isVillager = false;
    }

    public class Witch : Role
    {
        new public bool isVillager = true;
    }
    
    public class Seer : Role
    {
        new public bool isVillager = true;
    }

}
