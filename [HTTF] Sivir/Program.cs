using System;
using System.Linq;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Events;

namespace HTTF_Sivir
{
    class Sivir
    {
        static Menu Menu;
        static Menu ComboMenu, HarasMenu, LaneClearMenu, Emenu;
        static Spell.Skillshot Q;
        static Spell.Active W;
        static Spell.Active E;
        static Spell.Active R;
        static bool isClear = false;

        static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (W.IsReady() && UseW && target != null)
            {
                if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && target.Distance(ObjectManager.Player.ServerPosition) <= ObjectManager.Player.AttackRange && (target is AIHeroClient)) || (isClear == true && getSliderItem(LaneClearMenu, "mana") <= Player.Instance.ManaPercent) && (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear)))
                {
                    W.Cast();
                }
            }
        }

        static void SetSpells()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 1200, SkillShotType.Linear, 250, 1350, 90);
            W = new Spell.Active(SpellSlot.W, (uint)ObjectManager.Player.AttackRange);
            E = new Spell.Active(SpellSlot.E);
            R = new Spell.Active(SpellSlot.R, 1000);
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            SetSpells();
            SetMana();
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Sivir) return;

            Menu = MainMenu.AddMenu("HTTF Sivir", "HTTF Sivir");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo Settings");
            ComboMenu.Add("useQ", new CheckBox("Use Q"));
            ComboMenu.Add("useW", new CheckBox("Use W"));
            ComboMenu.Add("useR", new CheckBox("Use R"));
            HarasMenu = Menu.AddSubMenu("HARASS Settings", "Harass Settings");
            HarasMenu.Add("useQ", new CheckBox("Use Q"));
            HarasMenu.Add("useW", new CheckBox("Use W"));
            Emenu = Menu.AddSubMenu("Shield Settings", "Shield Settings");
            Emenu.Add("autoE", new CheckBox("Auto E"));
            Emenu.AddGroupLabel("E On : ");
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy))
            {
                for (var i = 0; i < 4; i++)
                {
                    var spell = enemy.Spellbook.Spells[i];
                    if (spell.SData.TargettingType != SpellDataTargetType.Self && spell.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                    {
                        if (spell.SData.TargettingType == SpellDataTargetType.Unit)
                            Emenu.Add("spell" + spell.SData.Name, new CheckBox(spell.Name, true));
                        else
                            Emenu.Add("spell" + spell.SData.Name, new CheckBox(spell.Name, false));
                    }
                }
            }
            LaneClearMenu = Menu.AddSubMenu("Clear Settings", "Clear Settings");
            LaneClearMenu.Add("useQ", new CheckBox("Use Q", false));
            LaneClearMenu.Add("useW", new CheckBox("Use W"));
        }
        static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }
        static bool UseQ { get { return getCheckBoxItem(ComboMenu, "useQ"); } }
        static bool UseW { get { return getCheckBoxItem(ComboMenu, "useW"); } }
        static bool UseE { get { return getCheckBoxItem(Emenu, "autoE"); } }
        static bool UseR { get { return getCheckBoxItem(ComboMenu, "useR"); } }
        static bool ManaManager { get { return getCheckBoxItem(ComboMenu, "manaManager"); } }
        static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }
        static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Mixed();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) || Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Clear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                Mixed();
            }
        }
        static void Combo()
        {
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var targetR = TargetSelector.GetTarget(800, DamageType.Physical);
            if (targetQ == null || !targetQ.IsValidTarget(Q.Range))
            {
                return;
            }
            var Qpos = Q.GetPrediction(targetQ);
            if (Q.IsReady() && UseQ && HitChance.High <= Qpos.HitChance &&  targetQ.CanMove)
            {
                Q.Cast(targetQ);
            }
            if (Q.IsReady() )
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && UseQ && !enemy.CanMove))
                    Q.Cast(enemy);
            }
            if (R.IsReady() && UseR && (ObjectManager.Player.CountEnemiesInRange(800f) > 2 || (ObjectManager.Player.HealthPercent <= 20 && ObjectManager.Player.CountEnemiesInRange(800f) >= 1)))
            {
                R.Cast();
            }
        }
        static void Mixed()
        {
            var targetQ = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (targetQ == null || !targetQ.IsValidTarget(Q.Range))
            {
                return;
            }
            var Qpos = Q.GetPrediction(targetQ);
            if (Q.IsReady() && getSliderItem(HarasMenu, "mana") < ObjectManager.Player.ManaPercent && getCheckBoxItem(HarasMenu, "useQ") && HitChance.High <= Qpos.HitChance && !ObjectManager.Player.Spellbook.IsAutoAttacking && targetQ.CanMove)
            {
                Q.Cast(targetQ);
            }
            if (Q.IsReady() )
            {
                foreach (var enemy in EntityManager.Heroes.Enemies.Where(enemy => enemy.IsValidTarget(Q.Range) && UseQ && !enemy.CanMove))
                    Q.Cast(enemy);
            }
        }
        static List<Obj_AI_Minion> Minions
        {
            get
            {
                return EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => Extensions.IsMinion(m) && Extensions.IsValidTarget(m, Q.Range)).ToList();
            }
        }
        static List<Obj_AI_Minion> JungleMinions
        {
            get
            {
                return EntityManager.MinionsAndMonsters.Monsters.Where(m => Extensions.IsValidTarget(m, ObjectManager.Player.AttackRange)).ToList();
            }
        }
        static List<Obj_AI_Minion> Minions2
        {
            get
            {
                return EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => Extensions.IsMinion(m) && Extensions.IsValidTarget(m, Q.Range)).ToList();
            }
        }
        static void Clear()
        {
            if (W.IsReady() && getCheckBoxItem(LaneClearMenu, "useW"))
            {
                if (Minions.Any())
                {
                    if (Minions.Count() >= 3 && getSliderItem(LaneClearMenu, "mana") <= Player.Instance.ManaPercent)
                    {
                        isClear = true;
                    }
                    else
                        isClear = false;
                }
                else if (JungleMinions.Any())
                {
                    isClear = true;
                }
                else
                    isClear = false;
            }
            var targetQ = TargetSelector.GetTarget(800, DamageType.Physical);
            if (Q.IsReady() && getSliderItem(LaneClearMenu, "mana") <= Player.Instance.ManaPercent && getCheckBoxItem(LaneClearMenu, "useQ"))
            {
                if (Minions.Any())
                {
                    if (Minions.Count() >= 5 && (targetQ == null || !targetQ.IsValidTarget(800)))
                    {
                        Q.Cast(Minions[0].ServerPosition);
                    }
                }
                else if (JungleMinions.Any())
                {
                    Q.Cast(JungleMinions[0].ServerPosition);
                }
            }
        }
        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Emenu["spell" + args.SData.Name] != null && !getCheckBoxItem(Emenu, "spell" + args.SData.Name))
                return;
            if (sender != null && args.Target != null && sender.Type == GameObjectType.AIHeroClient && args.Target.IsMe && sender.IsEnemy && UseE && E.IsReady())
            {
                if (!args.SData.ConsideredAsAutoAttack)
                {
                    if (!args.SData.Name.Contains("summoner") && !args.SData.Name.Contains("TormentedSoil"))
                    {
                        E.Cast();
                    }
                }
                else if (args.SData.Name == "BlueCardAttack" || args.SData.Name == "RedCardAttack" || args.SData.Name == "GoldCardAttack")
                {
                    E.Cast();
                }
            }
            else if (CanHitSkillShot(ObjectManager.Player, args) && !sender.IsMe && sender.Type == GameObjectType.AIHeroClient && E.IsReady())
            {
                E.Cast();
            }
        }
        static bool CanHitSkillShot(Obj_AI_Base target, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target == null && target.IsValidTarget(float.MaxValue))
            {
                int Collide = 0;
                if (args.SData.LineMissileEndsAtTargetPoint)
                {
                    Collide = 0;
                }
                else
                {
                    Collide = 1;
                }
                var pred = Prediction.Position.PredictLinearMissile(target, args.SData.CastRange, (int)args.SData.CastRadius, (int)args.SData.CastTime * 1000, args.SData.MissileSpeed, Collide).CastPosition;
                if (pred == null)

                    return false;
                if (args.SData.LineWidth > 0)
                {
                    var powCalc = Math.Pow(args.SData.LineWidth + target.BoundingRadius, 2);
                    if (pred.To2D().Distance(args.End.To2D(), args.Start.To2D(), true, true) <= powCalc ||
                        target.ServerPosition.To2D().Distance(args.End.To2D(), args.Start.To2D(), true, true) <= powCalc)
                    {
                        return true;
                    }
                }
                else if (target.Distance(args.End) < 50 + target.BoundingRadius ||
                         pred.Distance(args.End) < 50 + target.BoundingRadius)
                {
                    return true;
                }
            }
            return false;
        }

        static void SetMana()
        {
            if ((Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && !ManaManager) || ObjectManager.Player.HealthPercent < 20)
            {

                return;
            }


            if (!R.IsReady()) ;

            else if (R.IsLearned);
                 
        }
    }
}