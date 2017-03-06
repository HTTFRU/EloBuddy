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
    class sion
    {
        public static Menu Menu, ComboMenu, ClearMenu, Drawings;
        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        public static AIHeroClient _Player;


        public static void SionLoad()
        {
            Loading.OnLoadingComplete += OnLoaded;
        }

       private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Sion")
                return;


            Q = new Spell.Skillshot(SpellSlot.Q, 600, SkillShotType.Cone, 250, 1600, 140);
            W = new Spell.Active(SpellSlot.W, 550);
            E = new Spell.Skillshot(SpellSlot.E, 725, SkillShotType.Linear, 250, 1600, 60);
            R = new Spell.Skillshot(SpellSlot.R, 550, SkillShotType.Cone, 300, 900, 140);

            Menu = MainMenu.AddMenu("HTTF Sion", "Sion");

            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Use Q Combo"));
            ComboMenu.Add("ComboW", new CheckBox("Use W Combo"));
            ComboMenu.Add("ComboE", new CheckBox("Use E Combo"));
            ComboMenu.Add("ComboR", new CheckBox("Use R Combo"));
            ComboMenu.AddGroupLabel("Ultimate Settings");
            ComboMenu.Add("REnemies", new Slider("Min Hit Enemies Use R :", 3, 1, 5));
            ComboMenu.AddGroupLabel("FLEE");
            ComboMenu.Add("FleeR", new CheckBox("Use R "));

            ComboMenu.AddGroupLabel("Harass Settings");
            ComboMenu.Add("HQ", new CheckBox("Use Q Harass"));
            ComboMenu.Add("HW", new CheckBox("Use W Harass"));
            ComboMenu.Add("HE", new CheckBox("Use E Harass"));
            ComboMenu.Add("HarassWEQ", new CheckBox("Harass is W->E->Q"));




            ClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            ClearMenu.AddGroupLabel("JungleClear Settings");
            ClearMenu.Add("QJungle", new CheckBox("Use Q JungleClear"));
            
            ClearMenu.Add("WJungle", new CheckBox("Use W JungleClear"));
            ClearMenu.Add("EJungle", new CheckBox("Use E JungleClear"));

            ClearMenu.AddGroupLabel("Lane Clear Settings");
            ClearMenu.Add("LaneClearQ", new CheckBox("Use Q LaneClear"));
            ClearMenu.Add("QMinions", new Slider("Min Hit Enemies Use R :", 3, 1, 6));
            ClearMenu.Add("LaneClearW", new CheckBox("Use W LaneClear"));
            ClearMenu.Add("LaneClearE", new CheckBox("Use E LaneClear"));


            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("Q Range"));
            Drawings.Add("DrawE", new CheckBox("E Range"));
            Drawings.Add("DrawW", new CheckBox("W Range"));
            Drawings.Add("Draw_Disabled", new CheckBox("Disabled Drawings"));



            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue && Q.IsReady())
            {
                Circle.Draw(SharpDX.Color.Red, Q.Range, Player.Instance);
            }
            if (Drawings["DrawW"].Cast<CheckBox>().CurrentValue && W.IsReady())
            {
                Circle.Draw(SharpDX.Color.Red, W.Range, Player.Instance);
            }
            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue && E.IsReady())
            {
                Circle.Draw(SharpDX.Color.Goldenrod, E.Range, Player.Instance);
            }

        }
    
    public static void OnUpdate(EventArgs args)
    {
        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
        {
            LaneClearns();
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

       

    }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if ((target == null) || target.IsInvulnerable)
                return;

            eSkill(target);
            qSkill(target);
            wSkill(target);
            rSkill(target);


        }

        public static void qSkill(AIHeroClient target)
        {
            var CheckQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            if (target.IsInRange(Player.Instance, Q.Range) && !Q.IsOnCooldown && CheckQ)
            {
                Q.Cast(target);
            }
        }



        public static void wSkill(AIHeroClient target)
        {
            var CheckW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            if (target.IsInRange(Player.Instance, W.Range) && !W.IsOnCooldown && CheckW)
            {
                W.Cast(target);
            }
        }

        public static void eSkill(AIHeroClient target)
        {
            var CheckE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            if (target.IsInRange(Player.Instance, E.Range) && !E.IsOnCooldown && CheckE)
            {
                E.Cast(target);
            }
        }

        public static void rSkill(AIHeroClient target)
        {
            var CheckREnemies = ComboMenu["REnemies"].Cast<Slider>().CurrentValue;
            var CheckR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.R) == SpellState.Ready;
            if (target.CountEnemiesInRange(500) >= CheckREnemies && target.HealthPercent < 80)
            {
                R.Cast(target);
            }
        }

        static float TargetMinionS()
        {
            float B = 0;
            if (Q.IsReady() | W.IsReady())
            {
                B = 600;
            }
            else
            {
                B = R.Range;
            }
            return B;
        }
        private static void LaneClearns()
    {
        var LaneQ = ClearMenu["LaneClearQ"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.Q) == SpellState.Ready;
        var LaneQC = ClearMenu["QMinions"].Cast<Slider>().CurrentValue;
        var LaneW = ClearMenu["LaneClearW"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.W) == SpellState.Ready;
        var LaneE = ClearMenu["LaneClearE"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.E) == SpellState.Ready;

        if (LaneQ)
        {
            var Qminion = EntityManager.MinionsAndMonsters.GetLaneMinions().Where(x => x.IsValidTarget(TargetMinionS()));
            if (Qminion != null)
            {
                if (Q.IsReady())
                {
                    var position = Q.GetBestLinearCastPosition(Qminion);
                    if (position.HitNumber >= LaneQC)
                    {
                        Q.Cast(position.CastPosition);
                    }
                }
            }
        }
        if (LaneW)
        {
            var Wminion = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.ServerPosition, W.Range).OrderBy(minion => minion.Distance(Player.Instance.Position.To2D()));
            if (LaneW && Wminion.First().IsInRange(Player.Instance.Position, W.Range))
            {
                W.Cast(Wminion.First());
            }
        }

        if (LaneE)
        {
            var Eminion = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.ServerPosition, E.Range).OrderBy(minion => minion.Distance(Player.Instance.Position.To2D()));
            if (Eminion != null && LaneE)
            {
                var position = E.GetBestLinearCastPosition(Eminion);
                if (position.HitNumber >= 1)
                {
                    E.Cast(position.CastPosition);
                }
            }
        }
    }



public static void Harass()
{
    var CheckQ = ComboMenu["HQ"].Cast<CheckBox>().CurrentValue;
    var CheckW = ComboMenu["HW"].Cast<CheckBox>().CurrentValue;
    var CheckE = ComboMenu["HE"].Cast<CheckBox>().CurrentValue;

    var target = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
    if ((target == null) || target.IsInvulnerable)
        return;
    if (CheckQ)
    {
        qSkill(target);
    }
    if (CheckW)
    {
         wSkill(target);
    }
    if (CheckE)
    {
      eSkill(target);
    }
}



public static void JungleClear()
{
    var LaneQ = ClearMenu["QJungle"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.Q) == SpellState.Ready;
    var LaneW = ClearMenu["WJungle"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.W) == SpellState.Ready;
    var LaneE = ClearMenu["EJungle"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.E) == SpellState.Ready;

    int monsters = EntityManager.MinionsAndMonsters.Monsters.Where(monster => monster.IsValidTarget(W.Range * 2)).Count();
    if (monsters != 0)
    {
        if (LaneQ)
        {
            var targetmonster = EntityManager.MinionsAndMonsters.Monsters.Where(monster => monster.IsValidTarget(Q.Range));
            Q.Cast(targetmonster.FirstOrDefault());
        }
        if (LaneW)
        {
            var targetmonster = EntityManager.MinionsAndMonsters.Monsters.Where(monster => monster.IsValidTarget(W.Range));
            W.Cast(targetmonster.FirstOrDefault());
        }

        if (LaneE)
        {
            var targetmonster = EntityManager.MinionsAndMonsters.Monsters.Where(monster => monster.IsValidTarget(E.Range));
            E.Cast(targetmonster.FirstOrDefault());
        }
    }
}

        public static void Flee()
        {
            var FleeR = ComboMenu["FleeR"].Cast<CheckBox>().CurrentValue && Player.CanUseSpell(SpellSlot.R) == SpellState.Ready;


            if (FleeR)
            {
                var tempPos = Game.CursorPos;
                if (tempPos.IsInRange(Player.Instance.Position, R.Range))
                {
                    R.Cast(tempPos);
                }
                else
                {
                    R.Cast(Player.Instance.Position.Extend(tempPos, 800).To3DWorld());
                }
            }
        }
    }
}

