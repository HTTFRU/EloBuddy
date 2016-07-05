using System;
using System.Linq;
using Color = System.Drawing.Color;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;

namespace _HTTF_Riven
{
    public static class MiscLogic
    {
        public static bool IsJungleMinion(this Obj_AI_Base unit)
        {
            return EntityManager.MinionsAndMonsters.Monsters.Any(a => a.NetworkId == unit.NetworkId);
        }
        public static bool IsLaneMinion(this Obj_AI_Base unit)
        {
            return EntityManager.MinionsAndMonsters.EnemyMinions.Any(a => a.NetworkId == unit.NetworkId);
        }

    }
}
 