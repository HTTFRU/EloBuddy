using System;
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
    class Shen3
    {
        private static readonly AIHeroClient Shen = ObjectManager.Player;
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Targeted R;
        public static Spell.Skillshot Flash;


        public static Menu Menu, ComboMenu, JungleLaneMenu, MiscMenu, DrawMenu;

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
        }
        private static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }
        public static Vector3 PosEflash(Vector3 fetarget)
        {
            return fetarget + (Shen.Position - fetarget) / 2;
        }
        public static void ShenLoading()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }


        public static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Shen")
                return;


            Q = new Spell.Active(SpellSlot.Q, 200);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 610, SkillShotType.Linear, 500, 1600, 50);
            R = new Spell.Targeted(SpellSlot.R, 31000);
            var flashSlot = Shen.GetSpellSlotFromName("summonerflash");
            Flash = new Spell.Skillshot(flashSlot, 32767, SkillShotType.Linear);




            Menu = MainMenu.AddMenu("Shen HTTF", "Shen");


            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("QCast", new CheckBox("Use Q"));
            ComboMenu.Add("autow", new CheckBox("Use W"));
            ComboMenu.Add("useECombo", new CheckBox("Use E"));

            ComboMenu.AddGroupLabel("E Setting");
            ComboMenu.Add("eslider", new Slider("Minimum Enemy to Taunt", 1, 1, 5));
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team != obj.Team))
            {
                ComboMenu.Add("taunt" + obj.ChampionName.ToLower(), new CheckBox("Taunt " + obj.ChampionName));
            }
            ComboMenu.Add("ekey", new KeyBind("use e key", false, KeyBind.BindTypes.HoldActive, 'N'));
            ComboMenu.Add("flashe", new KeyBind("Flash E", false, KeyBind.BindTypes.HoldActive, 'N'));

            ComboMenu.AddGroupLabel("R Setting");

            ComboMenu.Add("autoult", new CheckBox("Auto R On press key?"));
            ComboMenu.Add("rslider", new Slider("Health Percent for Ult", 20));
            ComboMenu.Add("ult", new KeyBind("ULT", false, KeyBind.BindTypes.HoldActive, 'R'));
            foreach (var obj in ObjectManager.Get<AIHeroClient>().Where(obj => obj.Team == obj.Team))
            {
                ComboMenu.Add("ult" + obj.ChampionName.ToLower(), new CheckBox("Ult" + obj.ChampionName));
            }


            JungleLaneMenu = Menu.AddSubMenu("Clear Settings", "FarmSettings");
            JungleLaneMenu.AddGroupLabel("Lane Clear");
            JungleLaneMenu.Add("LCQ", new CheckBox("Use Q"));
            JungleLaneMenu.AddLabel("Jungle Clear");
            JungleLaneMenu.Add("LCQ", new CheckBox("Use Q"));



            MiscMenu = Menu.AddSubMenu("Misc Settings", "MiscSettings");
            MiscMenu.Add("TUT", new CheckBox("Auto Taunt Under Turret"));
            MiscMenu.Add("inte", new CheckBox("Interrupt E"));
            MiscMenu.AddGroupLabel("Flee Settings");
            MiscMenu.Add("fleee", new CheckBox("Use E"));

            DrawMenu = Menu.AddSubMenu("Draw Settings", "DrawSettings");
            DrawMenu.Add("drawe", new CheckBox("Draw E Range"));
            DrawMenu.Add("drawfe", new CheckBox("Draw Flash+E range"));

            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Game.OnUpdate += OnUpdate;
            AttackableUnit.OnDamage += OnDamage;
            Drawing.OnDraw += OnDraw;
            Core.DelayAction(FlashE, 1);
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            if (E.IsReady() && sender.IsValidTarget(E.Range) && MiscMenu["inte"].Cast<CheckBox>().CurrentValue)
                E.Cast(sender);
        }


        private static void OnDraw(EventArgs args)
        {
            if (Shen.IsDead) return;
            { 
            if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && R.IsLearned)
            {
                Circle.Draw(Color.LightBlue, E.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawfe"].Cast<CheckBox>().CurrentValue && E.IsLearned && Flash.IsReady() && E.IsReady())
            {
                Circle.Draw(Color.DarkBlue, E.Range + 425, Player.Instance.Position);
            }
            {
                DrawAllyHealths();
            }
            {
                Danger();
            }
            }
        }

        private static void Flee
            ()
        {
            if (!MiscMenu["fleee"].Cast<CheckBox>().CurrentValue) return;
            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast(MousePos);
        }
        private static void HighestAuthority()
        {
            var autoult = ComboMenu["autoult"].Cast<CheckBox>().CurrentValue;
            if (!autoult) return;
            if (Shen.CountEnemiesInRange(800) < 1 && Shen.HealthPercent >= 25)
                foreach (var ally in EntityManager.Heroes.Allies.Where(
                    x =>
                        ComboMenu["ult" + x.ChampionName].Cast<CheckBox>().CurrentValue && x.IsValidTarget(R.Range) &&
                        x.HealthPercent < 9)) if (R.IsReady() && ally.CountEnemiesInRange(650) >= 1)
                        R.Cast(ally);
        }
        private static void Ee()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var etarget = TargetSelector.GetTarget(600, DamageType.Magical);
            if (etarget == null) return;
            var predepos = E.GetPrediction(etarget).CastPosition;
            if (!ComboMenu["ekey"].Cast<KeyBind>().CurrentValue) return;
            if (!E.IsReady()) return;
            if (etarget.IsValidTarget(600))
            {
                E.Cast(predepos);
            }
        }

        private static void Ult()
        {
            var autoult = ComboMenu["autoult"].Cast<CheckBox>().CurrentValue;
            var rslider = ComboMenu["rslider"].Cast<Slider>().CurrentValue;
            if (!autoult || (!ComboMenu["ult"].Cast<KeyBind>().CurrentValue)) return;
            foreach (var ally in EntityManager.Heroes.Allies.Where(
                x =>
                    ComboMenu["ult" + x.ChampionName].Cast<CheckBox>().CurrentValue && x.IsValidTarget(R.Range) &&
                    x.HealthPercent < rslider))
                if (R.IsReady() && ally.CountEnemiesInRange(600) >= 1)
                    R.Cast(ally);
        }
        private static void OnUpdate(EventArgs args)
        {
            if (ComboMenu["flashe"].Cast<KeyBind>().CurrentValue)
            {
                FlashE();
            }

            
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            { 
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (ComboMenu["ekey"].Cast<KeyBind>().CurrentValue)
            {
                Ee();
            }
            if (ComboMenu["ult"].Cast<KeyBind>().CurrentValue)
            {
                Ult();
            }
        }
        private static void FlashE()
        {
            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var fetarget = TargetSelector.GetTarget(1025, DamageType.Magical);
            if (fetarget == null) return;
            var xpos = fetarget.Position.Extend(fetarget, E.Range);
            var predepos = E.GetPrediction(fetarget).CastPosition;
            {
                if (!E.IsReady() || !Flash.IsReady() && fetarget.Distance(Shen) > 1025) return;
                if (fetarget.IsValidTarget(1025))
                {
                    Flash.Cast((Vector3)xpos);
                    E.Cast(predepos);
                }
            }
        }

        private static
            void Combo()
        {
            var target = TargetSelector.GetTarget(200, DamageType.Magical);
            { 
            if (Q.IsReady())
                if (ComboMenu["QCast"].Cast<CheckBox>().CurrentValue && Q.IsReady() && target.Distance(Shen) < 190)
                {
                    Q.Cast();
                }
            
                if(ComboMenu["autoW"].Cast<CheckBox>().CurrentValue && W.IsReady())
                {
                    W.Cast();
                }
            }
                          
        }
        private static void DrawAllyHealths()
        {
            {
                float i = 0;
                foreach (
                    var hero in EntityManager.Heroes.Allies.Where(hero => hero.IsAlly && !hero.IsMe && !hero.IsDead))
                {
                    var champion = hero.ChampionName;
                    if (champion.Length > 12)
                    {
                        champion = champion.Remove(7) + "..";
                    }
                    var percent = (int)(hero.Health / hero.MaxHealth * 100);
                    var color = System.Drawing.Color.Red;
                    if (percent > 25)
                    {
                        color = System.Drawing.Color.Orange;
                    }
                    if (percent > 50)
                    {
                        color = System.Drawing.Color.Yellow;
                    }
                    if (percent > 75)
                    {
                        color = System.Drawing.Color.LimeGreen;
                    }
                    Drawing.DrawText(
                        Drawing.Width * 0.8f, Drawing.Height * 0.1f + i, color, " (" + champion + ") ");
                    Drawing.DrawText(
                        Drawing.Width * 0.9f, Drawing.Height * 0.1f + i, color,
                        ((int)hero.Health) + " (" + percent + "%)");
                    i += 20f;
                }
            }
        }

        private static void Danger()
        {
            var rslider = ComboMenu["rslider"].Cast<Slider>().CurrentValue;
            foreach (
                var ally in
                    EntityManager.Heroes.Allies.Where(
                        x => x.IsValidTarget(R.Range) && x.HealthPercent < rslider)
                )
            {
                const float i = 0;
                {
                    var champion = ally.ChampionName;
                    if (champion.Length > 12)
                    {
                        champion = champion.Remove(7) + "..";
                    }
                    var percent = (int)(ally.Health / ally.MaxHealth * 100);
                    var color = System.Drawing.Color.Red;
                    if (percent > 25)
                    {
                        color = System.Drawing.Color.Orange;
                    }
                    if (percent > 50)
                    {
                        color = System.Drawing.Color.Yellow;
                    }
                    if (percent > 75)
                    {
                        color = System.Drawing.Color.LimeGreen;
                    }
                    if (ally.CountEnemiesInRange(850) >= 1 && R.IsReady())
                    {
                        Drawing.DrawText(
                            Drawing.Width * 0.4f, Drawing.Height * 0.4f + i, color, " (" + champion + ")"
                                                                                + "-   (IS DYING - PRESS R TO AUTO ULT)");
                    }
                }
            }
        }

        private static void OnDamage(AttackableUnit sender, AttackableUnitDamageEventArgs args)
        {
            var valcheck = MiscMenu["TUT"].Cast<CheckBox>().CurrentValue;
            var t = EntityManager.Heroes.AllHeroes.FirstOrDefault(h => h.NetworkId == args.Source.NetworkId);
            var s = EntityManager.Heroes.Enemies.FirstOrDefault(h => h.NetworkId == args.Target.NetworkId);
            if (valcheck && t != null && s != null &&
                (t.IsMe &&
                 ObjectManager.Get<Obj_AI_Turret>()
                     .FirstOrDefault(x => x.Distance(t) < 750 && x.Distance(s) < 750 && x.IsAlly) != null))
            {
                {
                    E.Cast(s);
                }
            }
        }
        private static void LaneClear()
        {

            var qcheck = JungleLaneMenu["LCQ"].Cast<CheckBox>().CurrentValue;
            var qready = Q.IsReady();
            if (!qcheck || !qready) return;
            {
                Q.Cast(Shen);
            }
        }

    }
}
