using System;
using EloBuddy;
using EloBuddy.SDK;
using System.Linq;

namespace _HTTF_Riven
{
    internal class DamageLogic
    {
        public static float FastComboDamage(Obj_AI_Base enemy)
        {
            if (enemy != null)
            {
                float damage = 0;
                float passivenhan;
                if (Riven.myHero.Level >= 18)
                {
                    passivenhan = 0.5f;
                }
                else if (Riven.myHero.Level >= 15)
                {
                    passivenhan = 0.45f;
                }
                else if (Riven.myHero.Level >= 12)
                {
                    passivenhan = 0.4f;
                }
                else if (Riven.myHero.Level >= 9)
                {
                    passivenhan = 0.35f;
                }
                else if (Riven.myHero.Level >= 6)
                {
                    passivenhan = 0.3f;
                }
                else if (Riven.myHero.Level >= 3)
                {
                    passivenhan = 0.25f;
                }
                else
                {
                    passivenhan = 0.2f;
                }
                if (Riven.W.IsReady()) damage = damage + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.W);
                if (Riven.Q.IsReady())
                {
                    var qnhan = 4 - EventLogic.QCount;
                    damage = damage + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.Q) * qnhan +
                             Riven.myHero.GetAutoAttackDamage(enemy) * qnhan * (1 + passivenhan);
                }
                damage = damage + Riven.myHero.GetAutoAttackDamage(enemy) * (1 + passivenhan);
                if (Riven.R1.IsReady())
                {
                    return damage * 1.2f + ObjectManager.Player.GetSpellDamage(enemy, SpellSlot.R);
                }

                return damage;
            }
            return 0;
        }
    
 public static double ComboDamage(Obj_AI_Base target, bool noR = false)
        {
            double dmg = 0;
            var passiveStacks = 0;

            dmg += Riven.Q.IsReady()
                ? QDamage(!noR) * (3 - EventLogic.QCount)
                : 0;
            passiveStacks += Riven.Q.IsReady()
                ? (3 - EventLogic.QCount)
                : 0;

            dmg += Riven.W.IsReady()
                ? WDamage()
                : 0;
            passiveStacks += Riven.W.IsReady()
                ? 1
                : 0;
            passiveStacks += Riven.E.IsReady()
                ? 1
                : 0;

            dmg += PassiveDamage() * passiveStacks;
            dmg += (Riven.R1.IsReady() && !noR && !Player.Instance.HasBuff("RivenFengShuiEngine")
                ? Player.Instance.TotalAttackDamage * 1.2
                : Player.Instance.TotalAttackDamage) * passiveStacks;

            if (dmg < 10)
            {
                return 3 * Player.Instance.TotalAttackDamage;
            }

            dmg += Riven.R1.IsReady() && !noR
                ? RDamage(target, Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, (float)dmg))
                : 0;
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, (float)dmg);
        }

        public static float QDamage(bool useR = false)
        {
            return (float)(new double[] { 10, 30, 50, 70, 90 }[Riven.Q.Level - 1] +
                            ((Riven.R2.IsReady() && useR && !Player.Instance.HasBuff("RivenFengShuiEngine")
                                ? Player.Instance.TotalAttackDamage * 1.2
                                : Player.Instance.TotalAttackDamage) / 100) *
                            new double[] { 40, 45, 50, 55, 60 }[Riven.Q.Level - 1]);
        }

        public static float WDamage()
        {
            return (float)(new double[] { 50, 80, 110, 140, 170 }[Riven.W.Level - 1] +
                            1 * ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static double PassiveDamage()
        {
            return ((20 + ((Math.Floor((double)ObjectManager.Player.Level / 3)) * 5)) / 100) *
                   (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static float RDamage(Obj_AI_Base target, double healthMod = 0)
        {
            if (!Riven.R1.IsLearned) return 0;
            var hpPercent = (target.Health - healthMod > 0 ? 1 : target.Health - healthMod) / target.MaxHealth;
            return (float)((new double[] { 100, 150, 200 }[Riven.R1.Level - 1]
                             + 0.6 * Player.Instance.FlatPhysicalDamageMod) *
                            (hpPercent < 25 ? 3 : (((100 - hpPercent) * 2.67) / 100) + 1));
        }
    }
}

