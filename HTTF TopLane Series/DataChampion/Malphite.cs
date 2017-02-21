using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using Font = SharpDX.Direct3D9.Font;
using SharpDX.Direct3D9;
using Color = System.Drawing.Color;

namespace HTTF_TopLane_Series.DataChapmion
{
    internal class Malphite
    {
        public static Menu Menu, ComboMenu, ClearMenu, Drawings;
        public static Font Thm;
        public static Font Thn;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;

        public static void MalphiteLoading()
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Malphite")) return;
            Chat.Print("HTTF TopLane Series Loaded!", Color.Orange);
            Bootstrap.Init(null);
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Active(SpellSlot.W, 250);
            E = new Spell.Active(SpellSlot.E, 400);
            R = new Spell.Skillshot(SpellSlot.R, 1000, SkillShotType.Circular, 250, 700, 270);
             Ignite = new Spell.Targeted(_Player.GetSpellSlotFromName("summonerdot"), 600);
            Menu = MainMenu.AddMenu("HTTF Malhite", "Malphite");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use Q Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use W Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use E Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("ComboFQ", new KeyBind("Use R Selected Target", false, KeyBind.BindTypes.HoldActive, 'G'));
            ComboMenu.Add("ComboR", new CheckBox("Use R in multitarget"));
            ComboMenu.Add("MinR", new Slider("Min Hit Enemies Use R :", 3, 1, 5));


            ComboMenu.AddGroupLabel("Harass Settings");
            ComboMenu.Add("HarassQ", new CheckBox("Use Q Harass"));
            ComboMenu.Add("HarassW", new CheckBox("Use W Harass"));
            ComboMenu.Add("HarassE", new CheckBox("Use E Harass"));
            ComboMenu.Add("ManaQ", new Slider("Mana Harass % :", 40));

            ClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            ClearMenu.AddGroupLabel("JungleClear Settings");
            ClearMenu.Add("QJungle", new CheckBox("Use Q JungleClear"));
            ClearMenu.Add("WJungle", new CheckBox("Use W JungleClear"));
            ClearMenu.Add("EJungle", new CheckBox("Use E JungleClear"));
            ClearMenu.Add("JungleMana", new Slider("Mana JungleClear % :", 20));

            ClearMenu.AddGroupLabel("Lane Clear Settings");
            ClearMenu.Add("LaneClearQ", new CheckBox("Use Q LaneClear"));
            ClearMenu.Add("LaneClearW", new CheckBox("Use W LaneClear"));
            ClearMenu.Add("LaneClearE", new CheckBox("Use E LaneClear"));
            ClearMenu.Add("ManaLC", new Slider("Mana LaneClear", 50));
            ClearMenu.AddGroupLabel("LastHit Settings");
            ClearMenu.Add("LastHitQ", new CheckBox("Use Q LastHit"));
            ClearMenu.Add("ManaLH", new Slider("Mana LastHit % : ", 50));


            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("Q Range"));
            Drawings.Add("DrawE", new CheckBox("E Range"));
            Drawings.Add("DrawR", new CheckBox("R Range"));
            Drawings.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            if (Drawings["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;

            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = Q.Range }.Draw(_Player.Position);
            }

            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = E.Range }.Draw(_Player.Position);
            }

            if (Drawings["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = R.Range }.Draw(_Player.Position);
            }

            }
        

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                LastHit();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }

            RSelect();

            if (ComboMenu["ComboFQ"].Cast<KeyBind>().CurrentValue)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
            }
        }

        private static void Combo()
        {
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(R.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast();
                }

                if (useR && R.IsReady() && target.IsValidTarget(R.Range))
                {
                    var RPred = R.GetPrediction(target);
                    if (RPred.CastPosition.CountEnemiesInRange(250) >= MinR && RPred.HitChance >= HitChance.High)
                    {
                        R.Cast(RPred.CastPosition);
                    }
                }
            }
        }

        private static void RSelect()
        {
            var targetF = TargetSelector.SelectedTarget;
            var useFQ = ComboMenu["ComboFQ"].Cast<KeyBind>().CurrentValue;

            if (targetF == null)
            {
                return;
            }

            if (useFQ && R.IsReady())
            {
                if (targetF.IsValidTarget(R.Range))
                {
                    R.Cast(targetF.Position);
                }
            }
        }

        private static void LaneClear()
        {
            var mana = ClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var useQ = ClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue;
            var useW = ClearMenu["LaneClearW"].Cast<CheckBox>().CurrentValue;
            var useE = ClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    Q.Cast(minion);
                }

                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && _Player.Position.CountEnemyMinionsInRange(Q.Range) >= 3)
                {
                    W.Cast();
                }

                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && _Player.Position.CountEnemyMinionsInRange(E.Range) >= 3)
                {
                    E.Cast();
                }
            }
        }

        private static void LastHit()
        {
            var mana = ClearMenu["ManaLH"].Cast<Slider>().CurrentValue;
            var useQ = ClearMenu["LastHitQ"].Cast<CheckBox>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < mana) return;

            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health <= Player.Instance.GetSpellDamage(minion, SpellSlot.Q) && _Player.Distance(minion) > 175)
                {
                    Q.Cast(minion);
                }
            }
        }

        public static void JungleClear()
        {
            var useQ = ClearMenu["QJungle"].Cast<CheckBox>().CurrentValue;
            var useE = ClearMenu["EJungle"].Cast<CheckBox>().CurrentValue;
            var useW = ClearMenu["WJungle"].Cast<CheckBox>().CurrentValue;
            var mana = ClearMenu["JungleMana"].Cast<Slider>().CurrentValue;
            var monters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(R.Range));
            if (Player.Instance.ManaPercent <= mana) return;

            if (monters != null)
            {
                if (useQ && Q.IsReady() && monters.IsValidTarget(Q.Range))
                {
                    Q.Cast(monters);
                }

                if (useE && E.IsReady() && monters.IsValidTarget(E.Range))
                {
                    E.Cast();
                }

                if (useW && W.IsReady() && monters.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useW = ComboMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var ManaQ = ComboMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var useQ = ComboMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            if (Player.Instance.ManaPercent < ManaQ) return;

            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(Q.Range) && !e.IsDead))
            {
                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    Q.Cast(target);
                }

                if (useW && W.IsReady() && target.IsValidTarget(W.Range))
                {
                    W.Cast();
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast();
                }
            }
        }

        private static void Flee()
        {
            if (R.IsReady())
            {
                var cursorPos = Game.CursorPos;
                var castPos = Player.Instance.Position.Distance(cursorPos) <= R.Range ? cursorPos : Player.Instance.Position.Extend(cursorPos, R.Range).To3D();
                R.Cast(castPos);
            }
        }

       


       
        }
    }
