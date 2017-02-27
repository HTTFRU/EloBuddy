using System;
using System.Collections.Generic;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
namespace HTTF_TopLane_Series.DataChampion
{
    class Nautilus
    {
        private static AIHeroClient _myHero = Player.Instance;
        private static Spell.Skillshot Q;
        private static Spell.Active W;
        private static Spell.Active E;
        private static Spell.Targeted R;
        private static Spell.Targeted Ignite;
        private static Menu MainMenu, ComboMenu, WaweCleanMenu, DrawingsMenu;
        private static List<Spell.SpellBase> SpellList = new List<Spell.SpellBase>();

        public static void NautilusLoad()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Game.OnTick += Game_OnTick;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
        }
        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            {
            Q = new Spell.Skillshot(SpellSlot.Q, 1000, SkillShotType.Linear, 250, 2000, 60)
            { AllowedCollisionCount = 0 };
            W = new Spell.Active(SpellSlot.W, 175);
            E = new Spell.Active(SpellSlot.E, 400, DamageType.Magical);
            R = new Spell.Targeted(SpellSlot.R, 825, DamageType.Magical);

                if (Player.Instance.ChampionName != "Nautilus")
                    return;
            }

       
        {
            MainMenu = EloBuddy.SDK.Menu.MainMenu.AddMenu("HTTF Nautilus", "HTTF Nautilus");
            
            
            
            DrawingsMenu = MainMenu.AddSubMenu("Drawings");


            ComboMenu = MainMenu.AddSubMenu("Combo");
            ComboMenu.Add("Q", new CheckBox("Use Q"));
            ComboMenu.Add("W", new CheckBox("Use W"));
            ComboMenu.Add("E", new CheckBox("Use E"));
            ComboMenu.Add("R", new CheckBox("Use R"));
            ComboMenu.AddLabel("Use Auto R on target.");
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                ComboMenu.Add("Use R " + enemy.ChampionName, new CheckBox("Use R " + enemy.ChampionName));
            }
                ComboMenu.AddGroupLabel("Harass Setting");
                ComboMenu.Add("Q", new CheckBox("Use Q"));
                ComboMenu.Add("E", new CheckBox("Use E"));
                ComboMenu.AddGroupLabel("Misc");
                ComboMenu.Add("autoW", new CheckBox("Use Auto W"));
                ComboMenu.Add("interruptq", new CheckBox("Use Q to Interrupt."));
                ComboMenu.Add("interruptr", new CheckBox("Use R to Interrupt."));
                ComboMenu.AddLabel("Use Auto Q on target.");
                foreach (var enemy in EntityManager.Heroes.Enemies)
                {
                    ComboMenu.Add("Use auto Q " + enemy.ChampionName, new CheckBox("Use Auto Q " + enemy.ChampionName, false));

                }


            WaweCleanMenu = MainMenu.AddSubMenu("WaweandJung");
            WaweCleanMenu.AddGroupLabel("WaweCLean");
            WaweCleanMenu.Add("Q", new CheckBox("Use Q"));
            WaweCleanMenu.Add("W", new CheckBox("Use W"));
            WaweCleanMenu.Add("E", new CheckBox("Use E"));
            WaweCleanMenu.AddGroupLabel("WaweCLean");
            WaweCleanMenu.Add("E", new CheckBox("Use E"));



            DrawingsMenu = MainMenu.AddSubMenu("Drawings");
            DrawingsMenu.Add("Q", new CheckBox("Draw Q"));
            DrawingsMenu.Add("W", new CheckBox("Draw W"));
            DrawingsMenu.Add("E", new CheckBox("Draw E"));
            DrawingsMenu.Add("R", new CheckBox("Draw R"));
        }


        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            foreach (var Spell in SpellList.Where(spell => DrawingsMenu[spell.Slot.ToString()].Cast<CheckBox>().CurrentValue))

            {
                Circle.Draw(Spell.IsReady() ? Color.White : Color.Red, Spell.Range, _myHero);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
            if (target == null)
            {
                return;
            }


            var qpred = Q.GetPrediction(target);
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (_myHero.HealthPercent > 200 || target.HasBuffOfType(BuffType.SpellShield) ||
                    target.HasBuffOfType(BuffType.SpellImmunity))
                {
                    return;
                }
                if (target.IsValidTarget(Q.Range) && Q.IsReady() &&
                    ComboMenu["Use auto Q " + enemy.ChampionName].Cast<CheckBox>().CurrentValue && qpred.HitChance == HitChance.Impossible)
                {
                    Q.Cast(qpred.CastPosition);
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (sender.IsEnemy && e.DangerLevel <= DangerLevel.High)
            {
                var qpred = Q.GetPrediction(target);
                if (ComboMenu["interruptq"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.IsValidTarget(Q.Range))

                {
                    Q.Cast(qpred.CastPosition);
                }
                if (ComboMenu["interruptr"].Cast<CheckBox>().CurrentValue && R.IsReady() && target.IsValidTarget(R.Range))

                {
                    R.Cast(target);
                }
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                Jungle();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }
        }

       



        

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);



            if (ComboMenu["Q"].Cast<CheckBox>().CurrentValue)
            {
                if (target == null)
                {
                    return;
                }

                var Qpred = Q.GetPrediction(target);
                if (target.IsValidTarget(Q.Range) && Q.IsReady() && Qpred.HitChance >= HitChance.High)
                {

                    Q.Cast(target);
                }
            }

            if (ComboMenu["W"].Cast<CheckBox>().CurrentValue && W.IsReady() && _myHero.CountEnemiesInRange(E.Range) >
                0)
            {
                W.Cast();
            }

            if (ComboMenu["E"].Cast<CheckBox>().CurrentValue)
            {

                if (target == null)
                {
                    return;
                }

                if (target.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.Cast(target);
                }
            }
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                if (ComboMenu["R"].Cast<CheckBox>().CurrentValue)
                {
                    if (target == null)
                    {
                        return;
                    }

                    if (target.IsValidTarget(R.Range) && R.IsReady())
                    {
                        if (ComboMenu["Use R " + enemy.ChampionName].Cast<CheckBox>().CurrentValue)
                        {
                            R.Cast(target);
                        }
                    }
                }
            }
        }

        private static void Jungle()
        {
            var target =
                EntityManager.MinionsAndMonsters.GetJungleMonsters()
                    .OrderByDescending(a => a.MaxHealth)
                    .FirstOrDefault(a => a.IsValidTarget(900));

            if (target == null)
            {
                return;
            }
            if (WaweCleanMenu["Q"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                Q.Cast(target);
            }
            if (WaweCleanMenu["W"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                W.Cast();
            }
            if (WaweCleanMenu["E"].Cast<CheckBox>().CurrentValue && E.IsReady() && target.IsValidTarget(E.Range))
            {
                E.Cast();
            }
        }

        private static void LaneClear()
        {
            var target = EntityManager.MinionsAndMonsters.EnemyMinions.FirstOrDefault(a => !a.IsDead &&
            E.IsInRange(a));

            if (target == null)
            {
                return;
            }

            if (WaweCleanMenu["E"].Cast<CheckBox>().CurrentValue && E.IsReady() && target.IsValidTarget(E.Range))
            {
                E.Cast();
            }
        }

        private static void Harass()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);

            if (target == null)
            {
                return;
            }
            if (WaweCleanMenu["Q"].Cast<CheckBox>().CurrentValue)
            {

                var Qpred = Q.GetPrediction(target);
                if (target.IsValidTarget(Q.Range) && Q.IsReady() && Qpred.HitChance >= HitChance.High)
                {

                    Q.Cast(target);
                }
            }
            if (WaweCleanMenu["E"].Cast<CheckBox>().CurrentValue)
            {



                if (target.IsValidTarget(E.Range) && E.IsReady())
                {
                    E.Cast(target);
                }
            }
        }

    }
}