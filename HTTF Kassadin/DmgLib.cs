using EloBuddy;
using EloBuddy.SDK;

namespace HTTF_Kassadin
{
    internal class DmgLib
    {

        private static readonly AIHeroClient _Player = ObjectManager.Player;
        public static float QCalc(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 65, 95, 125, 155, 185 }[Program.Q.Level] + 0.7f * _Player.FlatMagicDamageMod
                    ));
        }

        public static float WCalc(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 40, 65, 90, 115, 140 }[Program.W.Level] + 0.6f * _Player.FlatMagicDamageMod
                    ));
        }

        public static float ECalc(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 105, 130, 155, 180 }[Program.E.Level] + 0.7f * _Player.FlatMagicDamageMod
                    ));
        }

        public static float RCalc(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 0, 80, 100, 120 }[Program.R.Level] + 0.2f * _Player.MaxMana + 0.1f * _Player.FlatMagicDamageMod
                    ));
        }
        public static float DmgCalc(AIHeroClient target)
        {
            var damage = 0f;
            if (Program.Q.IsReady() && target.IsValidTarget(Program.Q.Range))
                damage += QCalc(target);
            if (Program.W.IsReady())
                damage += WCalc(target);
            if (Program.E.IsReady() && target.IsValidTarget(Program.E.Range))
                damage += ECalc(target);
            if (Program.R.IsReady() && target.IsValidTarget(Program.R.Range))
                damage += RCalc(target);
            damage += _Player.GetAutoAttackDamage(target, true) * 2;
            return damage;
        }


    }
}