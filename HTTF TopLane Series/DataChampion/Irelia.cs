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
    static class Irelia
    {
        public static Menu Menu, ComboMenu, Ultimate, HarassMenu, ClearMenu, LaneClearMenu, KillStealMenu, Misc;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }


        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Targeted E;
        public static Spell.Skillshot R;
        public static Spell.Targeted Ignite;
        public static Font thm;
        public static Item Botrk;
        public static Item Bil;
        public static Item Sheen;
        public static Item Tryn;


        public static void IreliaLoading()
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Irelia")) return;
            Chat.Print("HTTF TopLane Series Loaded!", Color.Orange);
            Q = new Spell.Targeted(SpellSlot.Q, 625);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 425);
            R = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Linear, 250, 1600, 120);
            R.AllowedCollisionCount = int.MaxValue;
            Botrk = new Item(ItemId.Blade_of_the_Ruined_King);
            Sheen = new Item(ItemId.Sheen);
            Tryn = new Item(ItemId.Trinity_Force);
            Bil = new Item(3144, 475f);
            Menu = MainMenu.AddMenu("HTTF Irelia", "Irelia");
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use Q Combo"));
            ComboMenu.Add("ComboQ2", new CheckBox("Use [Q] on minions to get closer to the target"));
            ComboMenu.Add("ComboW", new CheckBox("Use[W Combo"));
            ComboMenu.Add("useRCombo", new CheckBox("Use R Combo"));
            ComboMenu.Add("DisQ", new Slider("Use [Q] If Enemy Distance >", 125, 0, 625));
            ComboMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
            ComboMenu.Add("ComboE", new CheckBox("Use [E] Combo"));
            ComboMenu.Add("AlwaysE", new CheckBox("Only Use [E] If Can Stun Target", false));
            ComboMenu.Add("CTurret", new KeyBind("Dont Use [Q] UnderTurret", false, KeyBind.BindTypes.PressToggle, 'I'));
            ComboMenu.AddGroupLabel("Interrupt Settings");
            ComboMenu.Add("interQ", new CheckBox("Use [E] Interrupt"));

            ComboMenu.AddGroupLabel("Harass Settings");
            ComboMenu.Add("HarassQ", new CheckBox("Use [Q] Harass"));
            ComboMenu.Add("HarassQ2", new CheckBox("Use [Q] on minions to get closer to the target"));
            ComboMenu.Add("HarassW", new CheckBox("Use [W] Harass"));
            ComboMenu.Add("DisQ2", new Slider("Use [Q] If Enemy Distance >", 125, 0, 625));
            ComboMenu.AddLabel("[Q] Distance < 125 = Always [Q]");
            ComboMenu.Add("HarassE", new CheckBox("Use E Harass"));
            ComboMenu.Add("AlwaysEH", new CheckBox("Only Use E If Can Stun Target"));
            ComboMenu.Add("ManaQ", new Slider("Mana Harass", 40));


            ComboMenu.AddGroupLabel("R Setting");
            ComboMenu.AddLabel("Use [R] Low Hp");
            ComboMenu.Add("RHeatlh", new CheckBox("Use R If MyHP <"));
            ComboMenu.Add("MauR", new Slider("MyHP Use [R] <", 50));
            ComboMenu.Add("Rminion", new CheckBox("Use R On Minion If No Enemies Around"));

            ComboMenu.AddGroupLabel("KillSteal Settings");
            ComboMenu.Add("KsQ", new CheckBox("Use Q KillSteal"));
            ComboMenu.Add("KsE", new CheckBox("Use E KillSteal"));
            ComboMenu.Add("KsR", new CheckBox("Use R KillSteal"));




            ClearMenu = Menu.AddSubMenu("Clear Settings", "JungleClear");
            ClearMenu.AddGroupLabel("JungleClear Settings");
            ClearMenu.Add("QJungle", new CheckBox("Use [Q] JungleClear"));
            ClearMenu.Add("WJungle", new CheckBox("Use [W] JungleClear"));
            ClearMenu.Add("EJungle", new CheckBox("Use [E] JungleClear"));
            ClearMenu.Add("JungleMana", new Slider("Mana JungleClear", 20));

            ClearMenu.AddGroupLabel("Lane Clear Settings");
            ClearMenu.Add("LaneClearQ", new CheckBox("Use [Q] LaneClear"));
            ClearMenu.Add("LaneClearW", new CheckBox("Use [W] LaneClear"));
            ClearMenu.Add("MauW", new Slider("Use [W] If HealthPercent <", 50));
            ClearMenu.Add("LaneClearE", new CheckBox("Use [E] LaneClear"));
            ClearMenu.Add("ManaLC", new Slider("Mana LaneClear", 50));
            ClearMenu.AddGroupLabel("LastHit Settings");
            ClearMenu.Add("LastHitQ", new CheckBox("Use [Q] LastHit"));
            ClearMenu.Add("LastHitE", new CheckBox("Use [E] LastHit"));
            ClearMenu.Add("ManaLH", new Slider("Mana LastHit", 50));


            Misc = Menu.AddSubMenu("Misc Changer", "Misc");
            Misc.AddGroupLabel("Flee Settings");
            Misc.Add("FleeQ", new CheckBox("Only Use [Q] Flee If Killalble Minion"));
            Misc.AddGroupLabel("Drawing Settings");
            Misc.Add("DrawQ", new CheckBox("[Q] Range"));
            Misc.Add("DrawR", new CheckBox("[R] Range"));
            Misc.Add("DrawTR", new CheckBox("Status Under Turret"));
            Misc.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Interrupter.OnInterruptableSpell += Interupt;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (_Player.IsDead) return;

            if (Misc["Draw_Disabled"].Cast<CheckBox>().CurrentValue) return;

            if (Misc["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = Q.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Orange, BorderWidth = 2f, Radius = R.Range }.Draw(_Player.Position);
            }

            if (Misc["DrawTR"].Cast<CheckBox>().CurrentValue)
            {
                Vector2 ft = Drawing.WorldToScreen(_Player.Position);
                if (ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue)
                {
                    DrawFont(thm, "Combo & Farm Under Turret : Disable", (float)(ft[0] - 110), (float)(ft[1] + 50), SharpDX.Color.White);
                }
                else
                {
                    DrawFont(thm, "Combo & Farm Under Turret : Enable", (float)(ft[0] - 110), (float)(ft[1] + 50), SharpDX.Color.Red);
                }
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

            KillSteal();
            Item();
            RMinions();
            RLogic();
        }

        public static bool UnderTuret(this Vector3 position)
        {
            return EntityManager.Turrets.Enemies.Where(a => a.Health > 0 && !a.IsDead).Any(a => a.Distance(position) < 950);
        }

        private static void Combo()
        {
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useQ2 = ComboMenu["ComboQ2"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var disQ = ComboMenu["DisQ"].Cast<Slider>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var HealthE = ComboMenu["AlwaysE"].Cast<CheckBox>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(1200)))
            {
                var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.Distance(Player.Instance) < Q.Range && Player.Instance.GetSpellDamage(m, SpellSlot.Q) > m.TotalShieldHealth()).OrderBy(m => m.Health).FirstOrDefault();
                if (useQ2 && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.Mana > Q.Handle.SData.Mana * 2 && _Player.Distance(target) > Q.Range && minion.Distance(target) < _Player.Distance(target))
                {
                    if (turret)
                    {
                        if (!minion.Position.UnderTuret())
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }

                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ <= target.Distance(Player.Instance))
                {
                    if (turret)
                    {
                        if (!target.Position.UnderTuret())
                        {
                            Q.Cast(target);
                        }
                    }
                    else
                    {
                        Q.Cast(target);
                    }
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (HealthE)
                    {
                        if (target.HealthPercent > _Player.HealthPercent || target.HealthPercent < 30)
                        {
                            E.Cast(target);
                        }
                    }
                    else
                    {
                        E.Cast(target);
                    }
                }

                if (useW && W.IsReady() && target.IsValidTarget(E.Range) && Player.Instance.GetAutoAttackRange() > target.Distance(Player.Instance))
                {
                    W.Cast();
                }
            }
        }

        private static void RLogic()
        {
            var target = TargetSelector.GetTarget(R.Range, DamageType.Physical);
            if (target != null)
            {
                var Rhealth = ComboMenu["RHeatlh"].Cast<CheckBox>().CurrentValue;
                var mauR = ComboMenu["MauR"].Cast<Slider>().CurrentValue;
                var RSheen = ComboMenu["RShen"].Cast<CheckBox>().CurrentValue;
                var useR = ComboMenu["useRCombo"].Cast<CheckBox>().CurrentValue;
                if (useR)
                {
                    if (Rhealth && R.IsReady() && target.IsValidTarget(R.Range))
                    {
                        if (_Player.HealthPercent < mauR)
                        {
                            R.Cast(target);
                        }
                    }

                    if (RSheen)
                    {
                        if (target.IsValidTarget(R.Range) && R.IsReady() && Player.Instance.GetAutoAttackRange() < target.Distance(Player.Instance) && !_Player.HasBuff("Sheen") && (Sheen.IsOwned() || Tryn.IsOwned()))
                        {
                            R.Cast(target);
                        }
                    }
                    else
                    {
                        if (target.IsValidTarget(R.Range) && _Player.HasBuff("IreliaTranscendentBlades"))
                        {
                            R.Cast(target);
                        }
                    }

                    if (!Sheen.IsOwned() && !Tryn.IsOwned())
                    {
                        if (target.IsValidTarget(R.Range) && _Player.HasBuff("IreliaTranscendentBlades"))
                        {
                            R.Cast(target);
                        }
                    }
                }
            }
        }

        private static void RMinions()
        {
            var useR = ComboMenu["Rminion"].Cast<CheckBox>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (minion == null) return;
            if (useR && _Player.HasBuff("IreliaTranscendentBlades") && _Player.Position.CountEnemiesInRange(1200) == 0)
            {
                R.Cast(minion);
            }
        }

        private static void LaneClear()
        {
            var laneQMN =  ClearMenu["ManaLC"].Cast<Slider>().CurrentValue;
            var minW =  ClearMenu["MauW"].Cast<Slider>().CurrentValue;
            var useQ =  ClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue;
            var useW =  ClearMenu["LaneClearW"].Cast<CheckBox>().CurrentValue;
            var useE =  ClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < laneQMN) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    if (turret)
                    {
                        if (!minion.Position.UnderTuret())
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }

                if (useW && W.IsReady() && minion.IsValidTarget(W.Range) && _Player.HealthPercent <= minW)
                {
                    W.Cast();
                }

                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
                }
            }
        }

        private static void LastHit()
        {
            var useQ =  ClearMenu["LastHitQ"].Cast<CheckBox>().CurrentValue;
            var useE =  ClearMenu["LastHitE"].Cast<CheckBox>().CurrentValue;
            var laneQMN =  ClearMenu["ManaLH"].Cast<Slider>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.IsValidTarget(Q.Range)).OrderBy(m => m.Health).FirstOrDefault();
            if (Player.Instance.ManaPercent < laneQMN) return;
            if (minion != null)
            {
                if (useQ && Q.IsReady() && minion.IsValidTarget(Q.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.Q))
                {
                    if (turret)
                    {
                        if (!minion.Position.UnderTuret())
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }

                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && minion.Health < Player.Instance.GetSpellDamage(minion, SpellSlot.E))
                {
                    E.Cast(minion);
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
                    E.Cast(monters);
                }

                if (useW && W.IsReady() && monters.IsValidTarget(E.Range) && Player.Instance.GetAutoAttackRange() <= monters.Distance(Player.Instance))
                {
                    W.Cast();
                }
            }
        }

        private static void Harass()
        {
            var useQ = ComboMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useQ2 = ComboMenu["HarassQ2"].Cast<CheckBox>().CurrentValue;
            var useW = ComboMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var HealthE = ComboMenu["AlwaysEH"].Cast<CheckBox>().CurrentValue;
            var disQ = ComboMenu["DisQ2"].Cast<Slider>().CurrentValue;
            var ManaQ = ComboMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var turret = ComboMenu["CTurret"].Cast<KeyBind>().CurrentValue;
            if (Player.Instance.ManaPercent < ManaQ) return;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(1200)))
            {
                var minion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(m => m.Distance(Player.Instance) < Q.Range && Player.Instance.GetSpellDamage(m, SpellSlot.Q) > m.TotalShieldHealth()).OrderBy(m => m.Health).FirstOrDefault();
                if (useQ2 && Q.IsReady() && minion.IsValidTarget(Q.Range) && Player.Instance.Mana > Q.Handle.SData.Mana * 2 && _Player.Distance(target) > Q.Range && minion.Distance(target) < _Player.Distance(target))
                {
                    if (turret)
                    {
                        if (!minion.Position.UnderTuret())
                        {
                            Q.Cast(minion);
                        }
                    }
                    else
                    {
                        Q.Cast(minion);
                    }
                }

                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && disQ <= target.Distance(Player.Instance))
                {
                    if (turret)
                    {
                        if (!target.Position.UnderTuret())
                        {
                            Q.Cast(target);
                        }
                    }
                    else
                    {
                        Q.Cast(target);
                    }
                }

                if (useE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (HealthE)
                    {
                        if (target.HealthPercent > _Player.HealthPercent || target.HealthPercent < 30)
                        {
                            E.Cast(target);
                        }
                    }
                    else
                    {
                        E.Cast(target);
                    }
                }

                if (useW && W.IsReady() && target.IsValidTarget(E.Range) && Player.Instance.GetAutoAttackRange() > target.Distance(Player.Instance))
                {
                    W.Cast();
                }
            }
        }

        // Use Items

        public static void Item()
        {
            var item = Misc["BOTRK"].Cast<CheckBox>().CurrentValue;
            var Minhp = Misc["ihp"].Cast<Slider>().CurrentValue;
            var Minhpp = Misc["ihpp"].Cast<Slider>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(e => e.IsValidTarget(475) && !e.IsDead))
            {
                if (item && Bil.IsReady() && Bil.IsOwned() && Bil.IsInRange(target))
                {
                    Bil.Cast(target);
                }
                if ((item && Botrk.IsReady() && Botrk.IsOwned() && target.IsValidTarget(475)) && (Player.Instance.HealthPercent <= Minhp || target.HealthPercent < Minhpp))
                {
                    Botrk.Cast(target);
                }
            }
        }

        public static Obj_AI_Base QFlee(Vector3 pos)
        {
            int distance = 250000;
            Obj_AI_Base unit = null;
            foreach (var source in EntityManager.MinionsAndMonsters.CombinedAttackable.Where(e => !e.IsDead && e.Health > 0 && e.Distance(Player.Instance) < 625))
            {
                int dist = (int)source.Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            if (unit != null) return unit;
            foreach (var source in EntityManager.Heroes.Enemies.Where(e => !e.IsDead && e.Health > 0 && e.Distance(Player.Instance) < 625))
            {
                int dist = (int)source.Distance(pos);
                if (dist >= distance) continue;
                distance = dist;
                unit = source;
            }
            return unit;
        }

        private static void Flee()
        {
            var Flee = Misc["FleeQ"].Cast<CheckBox>().CurrentValue;
            var e = QFlee(Game.CursorPos);
            if (Q.IsReady() && e != null && e.Distance(Game.CursorPos) < Player.Instance.Distance(Game.CursorPos))
            {
                if (Flee)
                {
                    if (e.Health < Player.Instance.GetSpellDamage(e, SpellSlot.Q))
                    {
                        Q.Cast(e);
                        return;
                    }
                }
                else
                {
                    Q.Cast(e);
                    return;
                }
            }
        }

        public static void Interupt(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs i)
        {
            var InterQ = ComboMenu["interQ"].Cast<CheckBox>().CurrentValue;
            if (!sender.IsEnemy || !(sender is AIHeroClient) || Player.Instance.IsRecalling())
            {
                return;
            }
            if (InterQ && E.IsReady() && i.DangerLevel == DangerLevel.High && E.IsInRange(sender) && sender.HealthPercent > _Player.HealthPercent)
            {
                E.Cast(sender);
            }
        }

        public static void DrawFont(Font vFont, string vText, float vPosX, float vPosY, ColorBGRA vColor)
        {
            vFont.DrawText(null, vText, (int)vPosX, (int)vPosY, vColor);
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 0, 80, 120, 160 }[R.Level] + 0.5f * _Player.FlatMagicDamageMod + 0.6f * _Player.FlatPhysicalDamageMod));
        }

        private static void KillSteal()
        {
            var KsQ = ComboMenu["KsQ"].Cast<CheckBox>().CurrentValue;
            var KsE = ComboMenu["KsE"].Cast<CheckBox>().CurrentValue;
            var KsR = ComboMenu["KsR"].Cast<CheckBox>().CurrentValue;
            foreach (var target in EntityManager.Heroes.Enemies.Where(hero => hero.IsValidTarget(R.Range) && !hero.HasBuff("JudicatorIntervention") && !hero.HasBuff("kindredrnodeathbuff") && !hero.HasBuff("Undying Rage") && !hero.IsDead && !hero.IsZombie))
            {
                if (KsQ && Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    if (target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.Q))
                    {
                        Q.Cast(target);
                    }
                }

                if (KsE && E.IsReady() && target.IsValidTarget(E.Range))
                {
                    if (target.Health <= Player.Instance.GetSpellDamage(target, SpellSlot.E))
                    {
                        E.Cast(target);
                    }
                }

                if (KsR && R.IsReady())
                {
                    if (target.Health <= RDamage(target) * 4 && (_Player.Distance(target) > 325 || !Q.IsReady()))
                    {
                        R.Cast(target);
                    }
                }


            }
        }
    }
}