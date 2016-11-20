using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using HTTF_Yasuo.Utils;
using SharpDX;

namespace HTTF_Yasuo
{
    class StateLogic
    {
        public static Obj_AI_Base ForcedMinion;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static void Combo()
        {
            var target = TargetSelector.SeletedEnabled && Yasuo.Combo["combo.leftclickRape"].Cast<CheckBox>().CurrentValue && TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget(1375)
                    ? TargetSelector.SelectedTarget
                    : TargetSelector.GetTarget(SpellDataBase.Q.Range + 475 + 100, DamageType.Physical);
            if (target == null) return;

            if (SpellDataBase.R.IsReady() && SpellDataBase.GetLowestKnockupTime() <= 250 + Game.Ping &&
                Yasuo.Combo["UseRCombo"].Cast<CheckBox>().CurrentValue &&
                (Yasuo.Combo["combo.RTarget"].Cast<CheckBox>().CurrentValue && target.IsKnockedUp() && TargetSelector.SelectedTarget == target ||
                Yasuo.Combo["combo.RKillable"].Cast<CheckBox>().CurrentValue && target.Health <= DamageInfo.RDamage(target) && target.Health > DamageInfo.QDamage(target) ||
                SpellDataBase.GetKnockedUpTargets() >= Yasuo.Combo["combo.MinTargetsR"].Cast<Slider>().CurrentValue))
            {
                SpellDataBase.R.Cast();
            }

            if (target.IsValidTarget(SpellDataBase.Q.Range) && Yasuo.Combo["UseQCombo"].Cast<CheckBox>().CurrentValue)
            {
                if (Player.Instance.IsDashing())
                {       
                    var pos = ForDash.GetPlayerPosition(300);
                    if (SpellDataBase.Q.IsReady() && (target.Distance(pos) < 400))
                    {
                        Player.CastSpell(SpellSlot.Q);
                    }
                }
                else if (SpellDataBase.Q.IsReady())
                {
                    SpellDataBase.Q.Cast(target);
                    return;
                }
            }

            if (Yasuo.Combo["UseECombo"].Cast<CheckBox>().CurrentValue && target.GetDashPos().Distance(target) < 400 && target.CanDash() && target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target))
            {
                SpellDataBase.E.Cast(target);
            }

            if (Yasuo.Combo["UseECombo"].Cast<CheckBox>().CurrentValue && target.Distance(Player.Instance) > Player.Instance.GetAutoAttackRange(target) && !Player.Instance.IsDashing())
            {
                foreach (var unit in EntityManager.MinionsAndMonsters.CombinedAttackable)
                {
                    if (!(unit.GetDashPos().Distance(target) < Player.Instance.Distance(target)) || (unit.GetDashPos().IsUnderTower() && TargetSelector.SelectedTarget != target)) continue;
                    SpellDataBase.E.Cast(unit);
                    return;
                }
                foreach (var unit in EntityManager.Heroes.Enemies)
                {
                    if (!(unit.GetDashPos().Distance(target) < Player.Instance.Distance(target)) || (unit.GetDashPos().IsUnderTower() && TargetSelector.SelectedTarget != target)) continue;
                    SpellDataBase.E.Cast(unit);
                    return;
                }
            }
            if (Player.Instance.HasWhirlwind()) return;

            if (Yasuo.Combo["stack.combo"].Cast<CheckBox>().CurrentValue) SpellDataBase.StackQ();
        }

        public static void Harass()
        {
            var target = TargetSelector.SeletedEnabled && TargetSelector.SelectedTarget != null && TargetSelector.SelectedTarget.IsValidTarget(1375)
                    ? TargetSelector.SelectedTarget
                    : TargetSelector.GetTarget(SpellDataBase.Q.Range + 475 + 100, DamageType.Physical);
            if (target == null) return;
            if (target.IsValidTarget(SpellDataBase.Q.Range) && Yasuo.Combo["harass.Q"].Cast<CheckBox>().CurrentValue)
            {
                if (Player.Instance.IsDashing())
                {
                    var pos = ForDash.GetPlayerPosition(300);
                    if (SpellDataBase.Q.IsReady() && (target.Distance(pos) < 400))
                    {
                        Player.CastSpell(SpellSlot.Q);
                    }
                }
                else if (SpellDataBase.Q.IsReady())
                {
                    SpellDataBase.Q.Cast(target);
                    return;
                }
            }

            var unit = target.GetClosestEUnit();
            if (Yasuo.Combo["harass.E"].Cast<CheckBox>().CurrentValue && unit != null && unit.GetDashPos().Distance(target) < Player.Instance.Distance(target) && (!unit.GetDashPos().IsUnderTower() || TargetSelector.SelectedTarget == target))
            {
                SpellDataBase.E.Cast(unit);
            }
            if (Player.Instance.HasWhirlwind()) return;
            if (Yasuo.Combo["harass.stack"].Cast<CheckBox>().CurrentValue) SpellDataBase.StackQ();
        }

        public static void Flee()
        {
            var unit = ForDash.GetClosestEUnit(Game.CursorPos);
            if (Yasuo.Flee["FleeE"].Cast<CheckBox>().CurrentValue && unit != null && unit.GetDashPos().Distance(Game.CursorPos) < Player.Instance.Distance(Game.CursorPos))
            {
                SpellDataBase.E.Cast(unit);
                return;
            }
            if (Yasuo.Flee["Flee.stack"].Cast<CheckBox>().CurrentValue && Player.Instance.HasWhirlwind() && !Player.Instance.IsDashing())
            {
                var target =
                    EntityManager.Heroes.Enemies.Where(a => a.IsValidTarget(SpellDataBase.Q.Range) && a.Health > 0 && !a.IsDead)
                        .OrderBy(a => a.Distance(Player.Instance))
                        .FirstOrDefault();
                if (target == null) return;
                SpellDataBase.Q.Cast(target);
                return;
            }
            if (Yasuo.Flee["Flee.stack"].Cast<CheckBox>().CurrentValue) SpellDataBase.StackQ();
        }

        public static void WaveClear()
        {
            ForcedMinion = null;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) < SpellDataBase.Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (minion == null) return;
            if (Yasuo.Clean["WC.Q"].Cast<CheckBox>().CurrentValue && Player.Instance.IsDashing())
            {
                var pos = ForDash.GetPlayerPosition(300);
                if (SpellDataBase.Q.IsReady() && (minion.Distance(pos) < 475 &&
                    DamageInfo.QDamage(minion) > minion.Health) || EntityManager.MinionsAndMonsters.GetLaneMinions().Count(a => a.Distance(pos) < 475) > 1)
                {
                    Player.CastSpell(SpellSlot.Q);
                }
            }
            if (Yasuo.Clean["WC.E"].Cast<CheckBox>().CurrentValue && SpellDataBase.E.IsReady() && (!minion.GetDashPos().IsUnderTower() || Yasuo.Clean["LaseEUT"].Cast<CheckBox>().CurrentValue))
            {
                if (DamageInfo.EDamage(minion) > minion.Health)
                {
                    SpellDataBase.E.Cast(minion);
                    return;
                }
                if (DamageInfo.EDamage(minion) + Player.Instance.GetAutoAttackDamage(minion) > minion.Health)
                {
                    Orbwalker.ForcedTarget = minion;
                    ForcedMinion = minion;
                }
            }
            if (Yasuo.Clean["WC.Q"].Cast<CheckBox>().CurrentValue && SpellDataBase.Q.IsReady() && DamageInfo.QDamage(minion) > minion.Health)
            {
                SpellDataBase.Q.Cast(minion);
                return;
            }
            if (Yasuo.Clean["WC.Q"].Cast<CheckBox>().CurrentValue && Yasuo.Clean["WC.E"].Cast<CheckBox>().CurrentValue && SpellDataBase.E.IsReady() && SpellDataBase.Q.IsReady() && DamageInfo.EDamage(minion) + DamageInfo.QDamage(minion) > minion.Health && !minion.GetDashPos().IsUnderTower())
            {
                SpellDataBase.Q.Cast(minion);
                return;
            }
        }

        public static void LastHit()
        {
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) < 475).OrderBy(a => a.Health).FirstOrDefault();
            if (minion == null) return;
            if (Yasuo.Clean["LastE"].Cast<CheckBox>().CurrentValue && SpellDataBase.E.IsReady() && DamageInfo.EDamage(minion) > minion.Health && (!minion.GetDashPos().IsUnderTower() || Yasuo.Clean["LaseEUT"].Cast<CheckBox>().CurrentValue))
            {
                SpellDataBase.E.Cast(minion);
                return;
            }
            if (Yasuo.Clean["WC.Q"].Cast<CheckBox>().CurrentValue && SpellDataBase.Q.IsReady() && DamageInfo.QDamage(minion) > minion.Health)
            {
                SpellDataBase.Q.Cast(minion);
                return;
            }
        }

        public static void AutoQ()
        {
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(SpellDataBase.Q.Range) && !hero.IsDead && !hero.IsZombie))
            {
                if (Player.Instance.HasWhirlwind() && !Yasuo.Combo["Auto.Q3"].Cast<CheckBox>().CurrentValue)
                    return;
                if (SpellDataBase.Q.IsReady() && SpellDataBase.Q.IsInRange(enemy))
                {
                    SpellDataBase.Q.Cast(enemy);              
                }
            }
        }
        public static bool CastQ()
        {
            var besttarget = TargetSelector.GetTarget(SpellDataBase.Q.Range, DamageType.Physical);
            if (besttarget == null) return false;
            var pred = SpellDataBase.Q.GetPrediction(besttarget);
            var predpos = new Vector3();

            if (besttarget != null && pred.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High)
            {
                predpos = pred.CastPosition;
            }

            return predpos.IsValid() && SpellDataBase.Q.Cast(pred.CastPosition);
        }
        public static bool CastQ3()
        {
            var targets = EntityManager.Heroes.Enemies.Where(x => x.Position.Distance(_Player.Position) < SpellDataBase.Q2.Range);
            if (targets.Count() == 0)
            {
                return false;
            }
            var posCast = new Vector3();                                  
            foreach (var pred in
                targets.Select(i => SpellDataBase.Q2.GetPrediction(i))
                    .Where(
                        i =>
                        i.HitChance >= EloBuddy.SDK.Enumerations.HitChance.High || (i.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium && i.CastPosition.CountEnemiesInRange(SpellDataBase.Q2.Width) > 1)).OrderByDescending(i => i.CastPosition.CountEnemiesInRange(SpellDataBase.Q2.Width)))
            {
                posCast = pred.CastPosition;
                break;
            }  
            return posCast.IsValid() && SpellDataBase.Q2.Cast(posCast);
        }
                                                                                                 
        public static void Jungle()
        {
            var minion = EntityManager.MinionsAndMonsters.GetJungleMonsters().Where(a => a.Distance(Player.Instance) < SpellDataBase.Q.Range).OrderByDescending(a => a.Health).FirstOrDefault();
            if (minion == null) return;
            if (Yasuo.Clean["JungE"].Cast<CheckBox>().CurrentValue && SpellDataBase.E.IsReady() && DamageInfo.EDamage(minion) > minion.Health && !minion.GetDashPos().IsUnderTower())
            {
                SpellDataBase.E.Cast(minion);
                return;
            }
            if (Yasuo.Clean["JungQ"].Cast<CheckBox>().CurrentValue && SpellDataBase.Q.IsReady())
            {
                SpellDataBase.Q.Cast(minion);
            }
        }
    }
}