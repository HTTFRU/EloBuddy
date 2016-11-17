using System.Linq;
using System.Net;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using HTTF_Yasuo.Utils;

namespace HTTF_Yasuo
{
    internal class SpellDataBase
    {
        public static Spell.Targeted E = new Spell.Targeted(SpellSlot.E, 475);
        public static Spell.Active R = new Spell.Active(SpellSlot.R, 1200);
        public static readonly Spell.Skillshot Q1 = new Spell.Skillshot(SpellSlot.Q, 450, SkillShotType.Linear, 250, GetNewQSpeed(), 1)
        {
            AllowedCollisionCount = int.MaxValue
        };
       public static readonly Spell.Skillshot Q2 = new Spell.Skillshot(SpellSlot.Q, 900, SkillShotType.Linear, 300, 1200, 50)
        {
            AllowedCollisionCount = int.MaxValue
        };
        public static Spell.Skillshot Q { get { return Player.Instance.HasWhirlwind() ? Q2 : Q1; } }

        public static void StackQ()
        {
            if (!Q.IsReady() || Player.Instance.HasWhirlwind()) return;
            var targets = EntityManager.Heroes.Enemies.Where(a => a.Distance(Player.Instance.Position) < 475 && !a.IsDead && a.Health > 0).Select(a => a as Obj_AI_Base);
            foreach (var target in targets.ToList())
            {
                if (target == null) continue;
                if (!Player.Instance.IsDashing())
                {
                    Q.Cast(target);
                    break;
                }
                if (!(ForDash.GetPlayerPosition(300)
                    .Distance(Prediction.Position.PredictUnitPosition(target, 300)) < 400)) continue;
                Player.CastSpell(SpellSlot.Q);
                break;
            }
            targets = EntityManager.MinionsAndMonsters.CombinedAttackable.Where(a => a.Distance(Player.Instance.Position) <= 475 && !a.IsDead && a.Health > 0).Select(a => a as Obj_AI_Base);
            foreach (var target in targets.ToList())
            {
                if (target != null)
                {
                    if (!Player.Instance.IsDashing())
                    {
                        Q.Cast(target);
                        break;
                    }
                    if (
                        ForDash.GetPlayerPosition(300).Distance(Prediction.Position.PredictUnitPosition(target, 300)) < 400)
                    {
                        Player.CastSpell(SpellSlot.Q);
                        break;
                    }
                }
            }
        }

        public static int GetKnockedUpTargets()
        {
            return EntityManager.Heroes.Enemies.Count(a => a.IsKnockedUp() && a.IsValidTarget(R.Range));
        }

        public static int EDelay()
        {
            return new[] { 10000, 9000, 8000, 7000, 6000 }[E.Level - 1];
        }

        public static int GetLowestKnockupTime()
        {
            return (int)EntityManager.Heroes.Enemies.Where(a => a.IsKnockedUp() && a.IsValidTarget(R.Range)).Select(a =>
            {
                var buff = a.Buffs.FirstOrDefault(b => b.IsKnockup || b.IsKnockback);
                return buff != null ? (buff.EndTime - Game.Time) * 1000 : 0;
            }).OrderBy(a => a).FirstOrDefault();
        }

        public static int GetNewQSpeed()
        {
            return (int)(1 / (1 / 0.5 * Player.Instance.AttackSpeedMod));
        }
    }
}