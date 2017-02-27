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
    class Renekton
    {
        private static readonly AIHeroClient Renek = ObjectManager.Player;
        private const string E2BuffName = "renektonsliceanddicedelay";
        public static Spell.Active Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        private static Spell.Active R;
        public static Item Hydra;
        public static Item Tiamat;
        public static Menu Menu, ComboMenu, JungleLaneMenu, MiscMenu, DrawMenu;
        private static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        public static void RenektonLoading()
        {
            Loading.OnLoadingComplete += OnLoaded;
        }
        public enum AttackSpell
        {
            Q,
            E,
            W,
            Tiamat,
            Hydra
        };
        private static bool HasSpell(string s)
        {
            return Player.Spells.FirstOrDefault(o => o.SData.Name.Contains(s)) != null;
        }
        private static void OnLoaded(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Renekton")
                return;
            Bootstrap.Init(null);
            Q = new Spell.Active(SpellSlot.Q, 225);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Skillshot(SpellSlot.E, 450, SkillShotType.Linear);
            R = new Spell.Active(SpellSlot.R);
            Tiamat = new Item((int)ItemId.Tiamat_Melee_Only, 420);
            Hydra = new Item((int)ItemId.Ravenous_Hydra_Melee_Only, 420);


            Menu = MainMenu.AddMenu("Renekton HTTF", "Renekton");


            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("usecomboq", new CheckBox("Use Q"));
            ComboMenu.Add("usecombow", new CheckBox("Use W"));
            ComboMenu.Add("usecomboe", new CheckBox("Use E"));
            ComboMenu.Add("autoult", new CheckBox("Auto Ult"));
            ComboMenu.Add("rslider", new Slider("Health Percentage to Ult", 11));
            ComboMenu.Add("useitems", new CheckBox("Use Items"));

            JungleLaneMenu = Menu.AddSubMenu("Clear Settings", "FarmSettings");
            JungleLaneMenu.AddGroupLabel("Lane Clear and jungle clean");
            JungleLaneMenu.Add("LCE", new CheckBox("Use E"));
            JungleLaneMenu.Add("LCQ", new CheckBox("Use Q"));
            JungleLaneMenu.Add("LCW", new CheckBox("Use W"));
            JungleLaneMenu.Add("LCI", new CheckBox("Use Items"));


            MiscMenu = Menu.AddSubMenu("Misc Settings", "MiscSettings");
            MiscMenu.Add("intw", new CheckBox("W to Interrupt"));
            MiscMenu.Add("gapclose", new CheckBox("W to Interrupt"));




            DrawMenu = Menu.AddSubMenu("Drawing Settings");
            DrawMenu.AddGroupLabel("Draw Settings");
            DrawMenu.Add("drawq", new CheckBox("Draw Q Range"));
            DrawMenu.Add("drawe", new CheckBox("Draw E Range"));

            Game.OnUpdate += OnTick;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Drawing.OnDraw += OnDraw;
            Gapcloser.OnGapcloser += OnGapClose;
        }
        private static void Flee()
        {
            Orbwalker.MoveTo(Game.CursorPos);
            E.Cast(Game.CursorPos);
        }
        private static
            void OnGapClose
            (AIHeroClient sender, Gapcloser.GapcloserEventArgs gapcloser)
        {
            if (!gapcloser.Sender.IsEnemy)
                return;
            var gapclose = MiscMenu["gapclose"].Cast<CheckBox>().CurrentValue;
            if (!gapclose)
                return;

            if (W.IsReady() && ObjectManager.Player.Distance(gapcloser.Sender, true) <
                Player.Instance.GetAutoAttackRange() && sender.IsValidTarget())
            {
                W.Cast(gapcloser.Sender);
            }
        }
        private static void OnDraw(EventArgs args)
        {
            if (Renek.IsDead) return;
            if (DrawMenu["drawq"].Cast<CheckBox>().CurrentValue && Q.IsLearned)
            {
                Circle.Draw(Color.Red, Q.Range, Player.Instance.Position);
            }
            if (DrawMenu["drawe"].Cast<CheckBox>().CurrentValue && E.IsLearned)
            {
                Circle.Draw(Color.DarkCyan, E.Range, Player.Instance.Position);
            }
        }
        private static void OnTick(EventArgs args)
        {

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
                Items();
                UseE();
                AutoUlt(ComboMenu["autoult"].Cast<CheckBox>().CurrentValue);
            }
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) ||
                    Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    LaneClear();
                    Items();
                }


            }
        }

        private static void AutoUlt(bool useR)
        {
            var autoR = ComboMenu["autoult"].Cast<CheckBox>().CurrentValue;
            var healthAutoR = ComboMenu["rslider"].Cast<Slider>().CurrentValue;
            if (autoR && _Player.HealthPercent < healthAutoR)
            {
               R.Cast();
            }
        }

        public static Obj_AI_Base GetEnemy(float range, GameObjectType t)
        {
            switch (t)
            {
                case GameObjectType.AIHeroClient:
                    return EntityManager.Heroes.Enemies.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
                default:
                    return EntityManager.MinionsAndMonsters.EnemyMinions.OrderBy(a => a.Health).FirstOrDefault(
                        a => a.Distance(Player.Instance) < range && !a.IsDead && !a.IsInvulnerable);
            }
        }


        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender,
            Interrupter.InterruptableSpellEventArgs args)
        {
            {
                if (W.IsReady() && sender.IsValidTarget(Player.Instance.GetAutoAttackRange()) &&
                    MiscMenu["intw"].Cast<CheckBox>().CurrentValue)
                    W.Cast(sender);
            }
        }


        public static void Combo()
        {
            var qcheck = ComboMenu["usecomboq"].Cast<CheckBox>().CurrentValue;
            var qready = Q.IsReady();

            if (!qcheck || !qready) return;
            {
                var enemy = TargetSelector.GetTarget(Q.Range, DamageType.Physical);

                if (enemy != null)
                    Q.Cast();
            }
        }

        public static
            void Items()
        {
            var ienemy = TargetSelector.GetTarget(Player.Instance.GetAutoAttackRange() + 425,
                DamageType.Physical);
            if (ienemy == null) return;
            if (!ienemy.IsValid || ienemy.IsZombie) return;


            if (ComboMenu["useitems"].Cast<CheckBox>().CurrentValue)
            {
                if (Hydra.IsOwned() && Hydra.IsReady() &&
                    Hydra.IsInRange(ienemy))
                    Hydra.Cast();
            }
            if (ComboMenu["useitems"].Cast<CheckBox>().CurrentValue)
            {
                if (Tiamat.IsOwned() && Tiamat.IsReady() &&
                    Tiamat.IsInRange(ienemy))
                    Tiamat.Cast();
            }

        }

        public static void UseE()

        {
            var target = TargetSelector.GetTarget(E.Range, DamageType.Physical);
            var eenemy = (Obj_AI_Minion)GetEnemy(E.Range, GameObjectType.obj_AI_Minion);
            if (target == null || !(Player.Instance.Distance(target.Position) < E.Range)) return;
            if (!ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue) return;
            if (E.IsReady())
            {
                Player.CastSpell(SpellSlot.E, target.Position);
            }
            else if (target != null && Player.Instance.Distance(target.Position) < 800 && eenemy != null &&
                     Player.Instance.Distance(eenemy.Position) < E.Range &&
                     target.Distance(eenemy.Position) < E.Range &&
                     (ComboMenu["usecomboe"].Cast<CheckBox>().CurrentValue))
            {
                Player.CastSpell(SpellSlot.E, eenemy.Position);
                if (Player.HasBuff(E2BuffName))
                    Player.CastSpell(SpellSlot.E, target.Position);
            }
        }

        public static void LaneClear()
        {
            var echeck = JungleLaneMenu["LCE"].Cast<CheckBox>().CurrentValue;
            var eready = E.IsReady();
            var qcheck = JungleLaneMenu["LCQ"].Cast<CheckBox>().CurrentValue;
            var wcheck = JungleLaneMenu["LCW"].Cast<CheckBox>().CurrentValue;
            var qready = Q.IsReady();
            var wready = W.IsReady();

            if (!echeck || !eready) return;
            {
                var aenemy = (Obj_AI_Minion)GetEnemy(E.Range, GameObjectType.obj_AI_Minion);

                if (aenemy != null)
                    E.Cast(aenemy.ServerPosition);                        
            }
            if (!qcheck || !qready) return;
            {
                var qenemy = (Obj_AI_Minion)GetEnemy(Q.Range, GameObjectType.obj_AI_Minion);

                if (qenemy != null)
                    Q.Cast();
            }
            if (!wcheck || !wready) return;
            {
                var wenemy =
                    (Obj_AI_Minion)GetEnemy(Player.Instance.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

                if (wenemy != null && Renek.GetSpellDamage(wenemy, SpellSlot.Q) >= wenemy.Health)
                    W.Cast();
            }
            if (!Orbwalker.CanAutoAttack) return;
            var cenemy = (Obj_AI_Minion)GetEnemy(Renek.GetAutoAttackRange(), GameObjectType.obj_AI_Minion);

            if (cenemy != null)
                Orbwalker.ForcedTarget = cenemy;
        }

        public static void ItemForJung()
        {
            var ienemy =
                (Obj_AI_Minion)GetEnemy(Player.Instance.GetAutoAttackRange() + 335, GameObjectType.obj_AI_Minion);

            if (ienemy == null) return;
            if (!ienemy.IsValid || ienemy.IsZombie) return;
            if (JungleLaneMenu["LCI"].Cast<CheckBox>().CurrentValue)
            {
                if (Hydra.IsOwned() && Hydra.IsReady() &&
                    Hydra.IsInRange(ienemy))
                    Hydra.Cast();
            }
            if (!JungleLaneMenu["LCI"].Cast<CheckBox>().CurrentValue) return;
            if (Tiamat.IsOwned() && Tiamat.IsReady() &&
                Tiamat.IsInRange(ienemy))
                Tiamat.Cast();
        }












    }
    }
