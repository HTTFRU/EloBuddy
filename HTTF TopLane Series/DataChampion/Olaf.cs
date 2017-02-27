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
using Color = System.Drawing.Color;

namespace HTTF_TopLane_Series.DataChampion
{
    class Olaf
    {
        private static Menu MainMenu, comboMenu, Clear, Drawings, harass;
        internal class Axe
        {
            public GameObject Object { get; set; }
            public float NetworkId { get; set; }
            public Vector3 AxePos { get; set; }
            public double ExpireTime { get; set; }
        }
        public static void OlafLoading()
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;

        }


        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.Equals(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                WaveClearFunc();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                HarassFunc();
            }
        }

        private static AIHeroClient User = Player.Instance;
        private static Spell.Skillshot Q;
        private static Spell.Active W;
        private static Spell.Targeted E;
        private static Spell.Active R;
        private static readonly Axe oAxe = new Axe();
        public static int LastTickTime;
        private static void Loading_OnLoadingComplete(EventArgs args)
        {

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Q = new Spell.Skillshot(SpellSlot.Q, 1000, skillShotType: SkillShotType.Linear, castDelay: 250, spellSpeed: 1550, spellWidth: 75);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Targeted(SpellSlot.E, 325);
            R = new Spell.Active(SpellSlot.R);
            if (User.ChampionName != "Olaf")
            {
                return;
            }



            MainMenu = EloBuddy.SDK.Menu.MainMenu.AddMenu("HTTF Olaf", "HTTF Olaf");
            comboMenu = MainMenu.AddSubMenu("Combo");
            comboMenu.Add("Q", new CheckBox("Use Q"));
            comboMenu.Add("W", new CheckBox("Use W"));
            comboMenu.Add("E", new CheckBox("Use E"));
            comboMenu.Add("RCast", new CheckBox("Use R to Flee -Cast when under CC"));
            comboMenu.Add("HPR", new Slider("Use R when under  HP", 25));
            comboMenu.Add("delay", new Slider("Humanizer Delay Cast R", 0, 0, 1000));
            comboMenu.Add("QStun", new CheckBox("Use [Q] If Enemy Has CC", false));
            Clear = MainMenu.AddSubMenu("Wave Clear / Jungle Clear");
            Clear.Add("QWC", new CheckBox("Use Q"));
            Clear.Add("WWC", new CheckBox("Use W"));
            Clear.Add("EWC", new CheckBox("Use E"));
            Clear.Add("manawc", new Slider("Mana manager", 0));
            harass = MainMenu.AddSubMenu("Harass hit");
            harass.Add("QHarass", new CheckBox("Use Q to Harass"));
            harass.Add("EHarass", new CheckBox("Use E to Harass"));
            harass.Add("manaharass", new Slider("Mana manager", 0));
            Drawings = MainMenu.AddSubMenu("Drawings", "Drawings");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("QDraw", new CheckBox("Draw Q Range"));
            Drawings.Add("EDraw", new CheckBox("Draw E Range", false));
            Drawings.Add("Axepos", new CheckBox("Draw Axe position"));

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["QDraw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.DarkBlue, BorderWidth = 1, Radius = Q.Range }.Draw(User.Position);
            }

            if (Drawings["EDraw"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.DarkGreen, BorderWidth = 1, Radius = E.Range }.Draw(User.Position);
            }
            var exTime = TimeSpan.FromSeconds(oAxe.ExpireTime - Game.Time).TotalSeconds;
            var color = exTime > 4 ? System.Drawing.Color.Black : System.Drawing.Color.OrangeRed;
            if (Drawings["Axepos"].Cast<CheckBox>().CurrentValue)
            {
                if (oAxe.Object != null)
                {
                    new Circle() { Color = Color.GreenYellow, BorderWidth = 6, Radius = 100 }.Draw(oAxe.Object.Position);
                    var line = new Geometry.Polygon.Line(
                    User.Position,
                    oAxe.AxePos,
                    User.Distance(oAxe.AxePos));
                    line.Draw(color, 2);
                }
            }
        }

        public static void QinStun()
        {
            var Rstun = Drawings["QStun"].Cast<CheckBox>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (Rstun && Q.IsReady())
            {

                if (target != null)
                {
                    if (target.HasBuffOfType(BuffType.Taunt) || target.HasBuffOfType(BuffType.Stun) || target.HasBuffOfType(BuffType.Snare) || target.HasBuffOfType(BuffType.Knockup))
                    {
                        Q.Cast(target.Position);
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var dangerHP = comboMenu["HPR"].Cast<Slider>().CurrentValue;
            if (target == null) return;

            if (comboMenu["Q"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && target.IsValidTarget(Q.Range))
                {
                    var castPosition = Q.GetPrediction(target).CastPosition.Extend(Player.Instance.Position, -100);

                    if (User.Distance(target) > 300)
                    {
                        Q.Cast(castPosition.To3DWorld());
                    }
                    else
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (comboMenu["W"].Cast<CheckBox>().CurrentValue)
            {
                if (W.IsReady() && target.IsValidTarget(E.Range))
                {
                    W.Cast();
                }
            }
            if (comboMenu["E"].Cast<CheckBox>().CurrentValue)
            {
                if (E.IsReady() && User.Distance(target.ServerPosition) <= E.Range)
                {
                    E.Cast(target);
                }
            }
            if (comboMenu["RCast"].Cast<CheckBox>().CurrentValue)
            {
                if (R.IsReady() && Player.HasBuffOfType(BuffType.Knockback) || Player.HasBuffOfType(BuffType.Blind) || Player.HasBuffOfType(BuffType.Charm) || Player.HasBuffOfType(BuffType.Silence) || Player.HasBuffOfType(BuffType.Suppression)  || Player.HasBuffOfType(BuffType.Polymorph) || Player.HasBuffOfType(BuffType.Disarm) || Player.HasBuffOfType(BuffType.Poison) || Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Taunt) || Player.HasBuffOfType(BuffType.Fear) || Player.HasBuffOfType(BuffType.Slow) || Player.HasBuffOfType(BuffType.Knockup) || Player.HasBuffOfType(BuffType.NearSight) || User.IsRooted && Player.Instance.HealthPercent <= dangerHP)
                {
                    R.Cast();
                }
            }
        }



        private static void HarassFunc()
        {
            var mana = harass["manaharass"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (target == null) return;

            if (harass["QHarass"].Cast<CheckBox>().CurrentValue)
            {
                if (Q.IsReady() && target.IsValidTarget(Q.Range) && Player.Instance.ManaPercent > mana)
                {
                    var castPosition = Q.GetPrediction(target).CastPosition.Extend(Player.Instance.Position, -100);

                    if (User.Distance(target) > 300)
                    {
                        Q.Cast(castPosition.To3DWorld());
                    }
                    else
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }
            if (harass["EHarass"].Cast<CheckBox>().CurrentValue)
            {
                if (E.IsReady() && target.IsValidTarget(E.Range))
                {
                    E.Cast(target);
                }
            }
        }

        private static void WaveClearFunc()
        {

            var monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(User.Position, Q.Range).OrderByDescending(a => a.MaxHealth).FirstOrDefault();
            var cstohit = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(a => a.Distance(Player.Instance) <= Q.Range).OrderBy(a => a.Health).FirstOrDefault();
            if (monsters != null)
            {
                if (Clear["QWC"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Q.IsInRange(monsters) && Player.Instance.ManaPercent >= Clear["manawc"].Cast<Slider>().CurrentValue)
                {
                    Q.Cast(monsters);
                }
                if (Clear["WWC"].Cast<CheckBox>().CurrentValue && W.IsReady() && monsters.IsValidTarget(325) && Player.Instance.ManaPercent >= Clear["manawc"].Cast<Slider>().CurrentValue)
                {
                    W.Cast();
                }

                if (Clear["EWC"].Cast<CheckBox>().CurrentValue && E.IsReady() && E.IsInRange(monsters))
                {
                    E.Cast(monsters);
                }
            }
            if (cstohit != null)
            {
                if (Clear["QWC"].Cast<CheckBox>().CurrentValue && Q.IsReady() && Q.IsInRange(cstohit) && Player.Instance.ManaPercent >= Clear["manawc"].Cast<Slider>().CurrentValue)
                {
                    var objAiHero = from x1 in ObjectManager.Get<Obj_AI_Minion>()
                                    where x1.IsValidTarget() && x1.IsEnemy
                                    select x1
                    into h
                                    orderby h.Distance(User) descending
                                    select h
                        into x2
                                    where x2.Distance(User) < Q.Range - 20 && !x2.IsDead
                                    select x2;
                    var aiMinions = objAiHero as Obj_AI_Minion[] ?? objAiHero.ToArray();
                    var lastMinion = aiMinions.First();
                    Q.Cast(lastMinion.Position);
                }
                if (Clear["WWC"].Cast<CheckBox>().CurrentValue && W.IsReady() && cstohit.IsValidTarget(325) && Player.Instance.ManaPercent >= Clear["manawc"].Cast<Slider>().CurrentValue)
                {
                    W.Cast();
                }

                if (Clear["EWC"].Cast<CheckBox>().CurrentValue && E.IsReady() && E.IsInRange(cstohit))
                {
                    E.Cast(cstohit);
                }
            }
        }

        private static void GameObject_OnCreate(GameObject obj, EventArgs args)
        {

            if (obj.Name.ToLower().Contains("olaf_base_q_axe") && obj.Name.ToLower().Contains("ally"))
            {
                oAxe.Object = obj;
                oAxe.ExpireTime = Game.Time + 8;
                oAxe.NetworkId = obj.NetworkId;
                oAxe.AxePos = obj.Position;
            }
        }

        private static void GameObject_OnDelete(GameObject obj, EventArgs args)
        {
            if (obj.Name.ToLower().Contains("olaf_base_q_axe") && obj.Name.ToLower().Contains("ally"))
            {
                oAxe.Object = null;
                LastTickTime = 0;
            }
        }

    }
}