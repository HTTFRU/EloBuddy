using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace _HTTF_Riven
{
    class StateLogic
    {
        public static bool EnableR;
        private static bool ssfl;
        public static void Combo()
        {
            EnableR = false;
            if (Orbwalker.IsAutoAttacking) return;

            var target = TargetSelector.GetTarget(Riven.E.Range + Riven.W.Range + 200, DamageType.Physical);

            if (Riven.R2.IsReady() && Player.Instance.HasBuff("RivenFengShuiEngine") &&
                Riven.ComboMenu["Combo.R2"].Cast<CheckBox>().CurrentValue)
            {
                if (
                    EntityManager.Heroes.Enemies.Where(
                        enemy =>
                            enemy.IsValidTarget(Riven.R2.Range) &&
                            enemy.Health <
                            Player.Instance.CalculateDamageOnUnit(enemy, DamageType.Physical,
                                DamageLogic.RDamage(enemy))).Any(enemy => Riven.R2.Cast(enemy)))
                {
                    return;
                }
            }

            if (target == null) return;

            if (Riven.ComboMenu["Combo.R"].Cast<CheckBox>().CurrentValue && Riven.R1.IsReady() && !Player.Instance.HasBuff("RivenFengShuiEngine"))
            {
                if (Riven.ComboMenu["Combo.RCombo"].Cast<CheckBox>().CurrentValue &&
                    target.Health > DamageLogic.ComboDamage(target, true)
                    && target.Health < DamageLogic.ComboDamage(target)
                    && target.Health > Player.Instance.GetAutoAttackDamage(target, true) * 2 ||
                    Riven.ComboMenu["Combo.RPeople"].Cast<CheckBox>().CurrentValue &&
                    Player.Instance.CountEnemiesInRange(1000) > 1 || Riven.IsRActive)
                {
                    if (Riven.E.IsReady())
                    {
                        EnableR = true;
                        ForceR();
                        Player.CastSpell(SpellSlot.R);
                    }
                }
            }

            if (Riven.ComboMenu["Combo.E"].Cast<CheckBox>().CurrentValue && (target.Distance(Player.Instance) > Riven.W.Range || Riven.IsRActive && Riven.R2.IsReady()) && Riven.E.IsReady())
            {
                Player.CastSpell(SpellSlot.E, target.Position);
                return;
            }

            if (Riven.ComboMenu["Combo.W"].Cast<CheckBox>().CurrentValue &&
                target.Distance(Player.Instance) <= Riven.W.Range && Riven.W.IsReady())
            {
                if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
                {
                    ItemLogic.Hydra.Cast();
                    return;
                }
                Player.CastSpell(SpellSlot.W);
            }
        }

        public static void Harass()
        {
            if (Orbwalker.IsAutoAttacking) return;

            var target = TargetSelector.GetTarget(Riven.E.Range + Riven.W.Range, DamageType.Physical);

            if (target == null) return;

            if (Riven.ComboMenu["Harass.E"].Cast<CheckBox>().CurrentValue &&
                (target.Distance(Player.Instance) > Riven.W.Range &&
                 target.Distance(Player.Instance) < Riven.E.Range + Riven.W.Range ||
                 Riven.IsRActive && Riven.R2.IsReady() &&
                 target.Distance(Player.Instance) < Riven.E.Range + 265 + Player.Instance.BoundingRadius) &&
                Riven.E.IsReady())
            {
                Player.CastSpell(SpellSlot.E, target.Position);
                return;
            }

            if (Riven.ComboMenu["Harass.W"].Cast<CheckBox>().CurrentValue &&
                target.Distance(Player.Instance) <= Riven.W.Range && Riven.W.IsReady())
            {
                if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
                {
                    ItemLogic.Hydra.Cast();
                    return;
                }
                Player.CastSpell(SpellSlot.W);
            }
        }

        public static void LaneClear()
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.IsAutoAttacking || EventLogic.LastCastQ + 260 > Environment.TickCount) return;
            foreach (
                var minion in EntityManager.MinionsAndMonsters.EnemyMinions.Where(a => a.IsValidTarget(Riven.W.Range)))
            {
                if (Riven.FarmMenu["WaveClear.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady() &&
                    minion.Health <=
                    Player.Instance.CalculateDamageOnUnit(minion, DamageType.Physical, DamageLogic.QDamage()))
                {
                    Player.CastSpell(SpellSlot.Q, minion.Position);
                    return;
                }
                if (Riven.FarmMenu["WaveClear.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                    minion.Health <=
                    Player.Instance.CalculateDamageOnUnit(minion, DamageType.Physical, DamageLogic.WDamage()))
                {
                    Player.CastSpell(SpellSlot.W);
                    return;
                }
            }
        }

        public static void LastHit()
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.IsAutoAttacking) return;

            foreach (
                var minion in EntityManager.MinionsAndMonsters.EnemyMinions.Where(a => a.IsValidTarget(Riven.W.Range)))
            {
                if (Riven.FarmMenu["LastHit.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady() &&
                    minion.Health <=
                    Player.Instance.CalculateDamageOnUnit(minion, DamageType.Physical, DamageLogic.QDamage()))
                {
                    Player.CastSpell(SpellSlot.Q, minion.Position);
                    return;
                }
                if (Riven.FarmMenu["LastHit.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                    minion.Health <=
                    Player.Instance.CalculateDamageOnUnit(minion, DamageType.Physical, DamageLogic.WDamage()))
                {
                    Player.CastSpell(SpellSlot.W);
                    return;
                }
            }
        }

        public static void Jungle()
        {
            var minion =
                EntityManager.MinionsAndMonsters.Monsters.OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault(a => a.Distance(Player.Instance) < Player.Instance.GetAutoAttackRange(a));

            if (minion == null) return;

            if (Riven.FarmMenu["Jungle.E"].Cast<CheckBox>().CurrentValue && (!Riven.W.IsReady() && !Riven.Q.IsReady() || Player.Instance.HealthPercent < 20) && Riven.E.IsReady() &&
                EventLogic.LastCastW + 750 < Environment.TickCount)
            {
                Player.CastSpell(SpellSlot.E, minion.Position);
            }
        }

        public static void ComboAfterAa(Obj_AI_Base target)
        {
            if (Player.Instance.HasBuff("RivenFengShuiEngine") && Riven.R2.IsReady() &&
                Riven.ComboMenu["Combo.R2"].Cast<CheckBox>().CurrentValue)
            {
                if (Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, DamageLogic.RDamage(target)) + Player.Instance.GetAutoAttackDamage(target, true) > target.Health)
                {
                    Riven.R2.Cast(target);
                    return;
                }
            }
            if (Riven.ComboMenu["Combo.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                Riven.W.IsInRange(target))
            {
                if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
                {
                    ItemLogic.Hydra.Cast();
                    return;
                }
                Player.CastSpell(SpellSlot.W);
                return;
            }
            if (Riven.ComboMenu["Combo.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady())
            {
                Player.CastSpell(SpellSlot.Q, target.Position);
                return;
            }
            if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
            {
                ItemLogic.Hydra.Cast();
            }
        }

        public static void HarassAfterAa(Obj_AI_Base target)
        {
            if (Riven.ComboMenu["Harass.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                Riven.W.IsInRange(target))
            {
                if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
                {
                    ItemLogic.Hydra.Cast();
                    return;
                }
                Player.CastSpell(SpellSlot.W);
                return;
            }
            if (Riven.ComboMenu["Harass.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady())
            {
                Player.CastSpell(SpellSlot.Q, target.Position);
                return;
            }
            if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
            {
                ItemLogic.Hydra.Cast();
            }
        }

        public static void LastHitAfterAa(Obj_AI_Base target)
        {
            var unitHp = target.Health - Player.Instance.GetAutoAttackDamage(target, true);
            if (unitHp > 0)
            {
                if (Riven.FarmMenu["LastHit.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady() &&
                    Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, DamageLogic.QDamage()) >
                    unitHp)
                {
                    Player.CastSpell(SpellSlot.Q, target.Position);
                    return;
                }
                if (Riven.FarmMenu["LastHit.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                    Riven.W.IsInRange(target) &&
                    Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, DamageLogic.WDamage()) >
                    unitHp)
                {
                    Player.CastSpell(SpellSlot.W);
                }
            }
        }

        public static void LaneClearAfterAa(Obj_AI_Base target)
        {
            var unitHp = target.Health - Player.Instance.GetAutoAttackDamage(target, true);
            if (unitHp > 0)
            {
                if (Riven.FarmMenu["WaveClear.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady())
                {
                    Player.CastSpell(SpellSlot.Q, target.Position);
                    return;
                }
                if (Riven.FarmMenu["WaveClear.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                    Riven.W.IsInRange(target))
                {
                    Player.CastSpell(SpellSlot.W);
                    return;
                }
            }
            else
            {
                List<Obj_AI_Minion> minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy,
                    Player.Instance.Position, Riven.Q.Range).Where(a => a.NetworkId != target.NetworkId).ToList();
                if (Riven.FarmMenu["WaveClear.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady() && minions.Any())
                {
                    Player.CastSpell(SpellSlot.Q, minions[0].Position);
                    return;
                }
                minions = minions.Where(a => a.Distance(Player.Instance) < Riven.W.Range).ToList();
                if (Riven.FarmMenu["WaveClear.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                    Riven.W.IsInRange(target) && minions.Any())
                {
                    Player.CastSpell(SpellSlot.W);
                    return;
                }
            }
        }
        private static void ForceR()
        {
            ssfl = (Riven.R1.IsReady() && Riven.R1.Name == "RivenFengShuiEngine" && Riven.R1.Cast());
            Core.DelayAction(() => ssfl = true, 500);
        }
        public static void JungleAfterAa(Obj_AI_Base target)
        {
            if (Riven.FarmMenu["Jungle.W"].Cast<CheckBox>().CurrentValue && Riven.W.IsReady() &&
                Riven.W.IsInRange(target))
            {
                if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
                {
                    ItemLogic.Hydra.Cast();
                    return;
                }
                Player.CastSpell(SpellSlot.W);
                return;
            }
            if (Riven.FarmMenu["Jungle.Q"].Cast<CheckBox>().CurrentValue && Riven.Q.IsReady())
            {
                Player.CastSpell(SpellSlot.Q, target.Position);
                return;
            }
            if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
            {
                ItemLogic.Hydra.Cast();
            }
        }
    }
}