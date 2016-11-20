using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using Color = System.Drawing.Color;
using SharpDX;


namespace HTTF_Kassadin
{
    internal class Program
    {
        public const string ChampionName = "Kassadin";

        public static Menu Menu, ComboMenu, FarmMenu, DrawMenu, Misc;

        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Skillshot R;
        private static readonly float _barLength = 104;
        private static readonly float _xOffset = 2;
        private static readonly float _yOffset = 9;
        private static readonly Vector2 Offset = new Vector2(1, 0);
        public static Item ZhonyaHourglass;

        public static AIHeroClient PlayerInstance
        {
            get { return Player.Instance; }
        }
        private static float HealthPercent()
        {
            return (PlayerInstance.Health / PlayerInstance.MaxHealth) * 100;
        }

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        private static int EBuffCount
        {
            get { return _Player.GetBuffCount("forcepulsecounter"); }
        }
        public static float RMana
        {
            get { return _Player.Spellbook.GetSpell(SpellSlot.R).SData.Mana; }
        }


        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Game_OnStart;
            Drawing.OnDraw += Game_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Gapcloser.OnGapcloser += KGapCloser;
            Interrupter.OnInterruptableSpell += KInterrupter;
            Orbwalker.OnPostAttack += Reset;
            Drawing.OnEndScene += Drawing_OnEndScene;

        }






        static void Game_OnStart(EventArgs args)
        {
            try
            {
                if (ChampionName != PlayerInstance.BaseSkinName)
                {
                    return;
                }

                Bootstrap.Init(null);
                
                Q = new Spell.Targeted(SpellSlot.Q, 650);
                W = new Spell.Active(SpellSlot.W);
                E = new Spell.Skillshot(SpellSlot.E, 600, SkillShotType.Cone, (int)0.5f, int.MaxValue, 10);
                R = new Spell.Skillshot(SpellSlot.R, 500, SkillShotType.Circular, (int)0.5f, int.MaxValue, 190);
                         if (_Player.GetSpellSlotFromName("summonerdot") != SpellSlot.Unknown)
                    ZhonyaHourglass = new Item(ItemId.Zhonyas_Hourglass);


                Menu = MainMenu.AddMenu("HTTF Kassadin", "kassadin");
                Menu.AddSeparator();
                Menu.AddLabel("Your comments and questions to the forum ");
                Menu.AddLabel("HELP ME , PM ME. AND MY SKYPE Bynoob_01 ");


                ComboMenu = Menu.AddSubMenu("Combo/Harass", "HTTFKassadin");
                ComboMenu.AddSeparator();
                ComboMenu.AddLabel("Combo Configs");
                ComboMenu.Add("ComboQ", new CheckBox("Use Q ", true));
                ComboMenu.Add("ComboW", new CheckBox("Use W ", true));
                ComboMenu.Add("ComboE", new CheckBox("Use E ", true));
                ComboMenu.Add("ComboR", new CheckBox("Use R ", true));
                ComboMenu.Add("MaxR", new Slider("Don't use R if more than Eminies on range :", 2, 1, 5));
                ComboMenu.AddSeparator();
                ComboMenu.AddLabel("Harass Configs");
                ComboMenu.Add("ManaH", new Slider("Dont use Skills if Mana <=", 40));
                ComboMenu.Add("HarassQ", new CheckBox("Use Q ", true));
                ComboMenu.Add("HarassW", new CheckBox("Use W ", true));
                ComboMenu.Add("HarassE", new CheckBox("Use E ", true));
                ComboMenu.Add("HarassR", new CheckBox("Use R ", true));
                FarmMenu = Menu.AddSubMenu("Lane/LastHit", "Modes2Kassadin");
                FarmMenu.AddLabel("LastHit Configs");
                FarmMenu.Add("ManaL", new Slider("Dont use Skills if Mana <=", 40));
                FarmMenu.Add("LastQ", new CheckBox("Use Q ", true));
                FarmMenu.Add("LastW", new CheckBox("Use W ", true));
                FarmMenu.Add("LastE", new CheckBox("Use E ", true));
                FarmMenu.AddLabel("Lane Cler Config");
                FarmMenu.Add("ManaF", new Slider("Dont use Skills if Mana <=", 40));
                FarmMenu.Add("FarmQ", new CheckBox("Use Q ", true));
                FarmMenu.Add("FarmW", new CheckBox("Use W ", true));
                FarmMenu.Add("FarmE", new CheckBox("Use E ", true));
                FarmMenu.Add("MinionE", new Slider("Use E when count minions more than :", 3, 1, 5));
                FarmMenu.Add("FarmR", new CheckBox("Use R ", true));



                DrawMenu = Menu.AddSubMenu("Draws", "DrawKassadin");
                DrawMenu.Add("drawAA", new CheckBox("Draw do AA(White)", true));
                DrawMenu.Add("drawQ", new CheckBox(" Draw do Q(Aqua)", true));
                DrawMenu.Add("drawW", new CheckBox(" Draw do W(Green)", true));
                DrawMenu.Add("drawE", new CheckBox(" Draw do E(Red)", true));
                DrawMenu.Add("drawR", new CheckBox(" Draw do R(Blue)", true));
                DrawMenu.Add("DamageIndicator", new CheckBox("Draw Damage"));



                Misc = Menu.AddSubMenu("MiscMenu", "Misc");
                Misc.Add("aarest", new CheckBox("Reset AA with w"));
                Misc.Add("useQGapCloser", new CheckBox("Q on GapCloser", true));
                Misc.Add("eInterrupt", new CheckBox("use E to Interrupt", true));
                Misc.AddLabel("• SkinHack •");
                Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
                Misc.Add("Skinid", new Slider("Skin ID", 0, 0, 11));
                Misc.AddLabel("• Activator •");
                Misc.Add("Zhonyas", new CheckBox("Use Zhonyas"));
                Misc.Add("ZhonyasHp", new Slider("Use Zhonyas If Your HP%", 20));





            }

            catch (Exception e)
            {
                Chat.Print("Kassadin: Error: " + e.Message);
            }

        }

        static void KInterrupter(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs args)
        {

            if (args.DangerLevel == DangerLevel.High && sender.IsEnemy && sender is AIHeroClient && sender.Distance(_Player) < Q.Range && Q.IsReady() && Misc["eInterrupt"].Cast<CheckBox>().CurrentValue)
            {
                Q.Cast(sender);
            }

        }

        private static void AutoHourglass()
        {
            var zhonyas = Misc["Zhonyas"].Cast<CheckBox>().CurrentValue;
            var zhonyasHp = Misc["ZhonyasHp"].Cast<Slider>().CurrentValue;

            if (zhonyas && _Player.HealthPercent <= zhonyasHp && ZhonyaHourglass.IsReady())
            {
                ZhonyaHourglass.Cast();
            }
        }

        static void KGapCloser(Obj_AI_Base sender, Gapcloser.GapcloserEventArgs args)
        {


            if (sender.IsEnemy && sender is AIHeroClient && sender.Distance(_Player) < E.Range && E.IsReady() && Misc["useQGapCloser"].Cast<CheckBox>().CurrentValue)
            {
                E.Cast(sender);
            }
        }


        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (_Player.IsDead)
                return;
            if (!DrawMenu["DamageIndicator"].Cast<CheckBox>().CurrentValue) return;
            foreach (var aiHeroClient in EntityManager.Heroes.Enemies)
            {
                if (!aiHeroClient.IsHPBarRendered || !aiHeroClient.VisibleOnScreen) continue;
                {
                    if (aiHeroClient.Distance(_Player) < 1500)
                    {
                        foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => !a.IsDead && a.IsHPBarRendered))
                        {
                            var damage = DmgLib.DmgCalc(enemy);
                            var damagepercent = (enemy.TotalShieldHealth() - damage > 0 ? enemy.TotalShieldHealth() - damage : 0) /
                                                (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                            var hppercent = enemy.TotalShieldHealth() /
                                            (enemy.MaxHealth + enemy.AllShield + enemy.AttackShield + enemy.MagicShield);
                            var start = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + damagepercent * 104),
                                (int)(enemy.HPBarPosition.Y + Offset.Y) - -9);
                            var end = new Vector2((int)(enemy.HPBarPosition.X + Offset.X + hppercent * 104) + 2,
                                (int)(enemy.HPBarPosition.Y + Offset.Y) - -9);

                            Drawing.DrawLine(start, end, 9, System.Drawing.Color.Chartreuse);
                        }
                    }
                }
            }
        }



        static void Game_OnDraw(EventArgs args)
        {

            if (DrawMenu["drawAA"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.White, Radius = _Player.GetAutoAttackRange(), BorderWidth = 2f }.Draw(_Player.Position);
            }
            if (DrawMenu["drawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Aqua, Radius = 650, BorderWidth = 2f }.Draw(_Player.Position);
            }
            if (DrawMenu["drawW"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Green, Radius = (_Player.GetAutoAttackRange() + 50), BorderWidth = 2f }.Draw(_Player.Position);
            }
            if (DrawMenu["drawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Red, Radius = 600, BorderWidth = 2f }.Draw(_Player.Position);
            }
            if (DrawMenu["drawR"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.Blue, Radius = 500, BorderWidth = 2f }.Draw(_Player.Position);
            }

        }
        private static int SkinId()
        {
            return Misc["Skinid"].Cast<Slider>().CurrentValue;
        }

        private static bool CheckSkin()
        {
            return Misc["checkSkin"].Cast<CheckBox>().CurrentValue;
        }
        static void Game_OnUpdate(EventArgs args)
        {
            if (CheckSkin())
            {
                EloBuddy.Player.SetSkinId(SkinId());
            }

            {


                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    var alvo = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                    var rmax = EntityManager.Heroes.Enemies.Where(t => t.IsInRange(Player.Instance.Position, R.Range) && !t.IsDead && t.IsValid && !t.IsInvulnerable).Count();
                    if (!alvo.IsValid()) return;
                    if (Q.IsReady() && Q.IsInRange(alvo) && ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue)
                    {
                        Q.Cast(alvo);
                    }
                    if (W.IsReady() && _Player.Distance(alvo) <= _Player.GetAutoAttackRange() + 50 && ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();

                    }
                    if (E.IsReady() && E.IsInRange(alvo) && ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast(alvo);

                    }
                    if (R.IsReady() && R.IsInRange(alvo) && ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue && !(rmax >= ComboMenu["MaxR"].Cast<Slider>().CurrentValue))
                    {
                        R.Cast(alvo);

                    }


                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    var alvo = TargetSelector.GetTarget(Q.Range, DamageType.Magical);
                    if (!alvo.IsValid()) return;



                    if ((_Player.ManaPercent <= ComboMenu["ManaH"].Cast<Slider>().CurrentValue))
                    {
                        return;
                    }
                    if (Q.IsReady() && Q.IsInRange(alvo) && ComboMenu["HarassQ"].Cast<CheckBox>().CurrentValue)
                    {
                        Q.Cast(alvo);
                    }
                    if (W.IsReady() && _Player.Distance(alvo) <= _Player.GetAutoAttackRange() + 50 && ComboMenu["HarassW"].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();

                    }
                    if (E.IsReady() && E.IsInRange(alvo) && ComboMenu["HarassE"].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast(alvo);

                    }
                    if (R.IsReady() && R.IsInRange(alvo) && ComboMenu["HarassR"].Cast<CheckBox>().CurrentValue)
                    {
                        R.Cast(alvo);

                    }


                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {



                    var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position, Q.Range).OrderByDescending(x => x.MaxHealth).FirstOrDefault();

                    var minion = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t.IsInRange(Player.Instance.Position, E.Range) && !t.IsDead && t.IsValid && !t.IsInvulnerable).Count();


                    if ((_Player.ManaPercent <= FarmMenu["ManaF"].Cast<Slider>().CurrentValue))
                    {
                        return;
                    }
                    if (Q.IsReady() && Q.IsInRange(minions) && FarmMenu["FarmQ"].Cast<CheckBox>().CurrentValue && minions.Health < DmgLib.QCalc(minions))
                    {
                        Q.Cast(minions);
                    }

                    if (E.IsReady() && E.IsInRange(minions) && FarmMenu["FarmE"].Cast<CheckBox>().CurrentValue && (minion >= FarmMenu["MinionE"].Cast<Slider>().CurrentValue))
                    {

                        E.Cast(minions);

                    }
                    if (R.IsReady() && R.IsInRange(minions) && FarmMenu["FarmR"].Cast<CheckBox>().CurrentValue)
                    {
                        R.Cast(minions);

                    }
                    if (W.IsReady() && _Player.Distance(minions) <= _Player.GetAutoAttackRange() + 50 && FarmMenu["FarmW"].Cast<CheckBox>().CurrentValue)
                    {
                        W.Cast();

                    }
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, _Player.Position, Q.Range).OrderByDescending(x => x.MaxHealth).FirstOrDefault();

                    var minion = EntityManager.MinionsAndMonsters.EnemyMinions.Where(t => t.IsInRange(Player.Instance.Position, E.Range) && !t.IsDead && t.IsValid && !t.IsInvulnerable).Count();

                    if (Q.IsReady() && Q.IsInRange(minions) && FarmMenu["LastQ"].Cast<CheckBox>().CurrentValue && minions.Health < DmgLib.QCalc(minions))
                    {
                        Q.Cast(minions);
                    }
                    if (W.IsReady() && _Player.Distance(minions) <= _Player.GetAutoAttackRange() + 50 && FarmMenu["LastW"].Cast<CheckBox>().CurrentValue && minions.Health < DmgLib.WCalc(minions))
                    {
                        W.Cast();

                    }
                    if (E.IsReady() && E.IsInRange(minions) && FarmMenu["LastE"].Cast<CheckBox>().CurrentValue && (minion >= FarmMenu["MinionE"].Cast<Slider>().CurrentValue && minions.Health < DmgLib.ECalc(minions)))
                    {

                        E.Cast(minions);


                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                    {
                        var Mpos = Game.CursorPos;

                        if (R.IsReady())
                        {

                            R.Cast(Mpos);
                        }
                    }
                }
            }
        }
        private static void Reset(AttackableUnit target, EventArgs args)
        {
            if (!Misc["aareset"].Cast<CheckBox>().CurrentValue) return;
            if (target != null && target.IsEnemy && !target.IsInvulnerable && !target.IsDead && target is AIHeroClient && target.Distance(ObjectManager.Player) <= W.Range)
                if (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) && (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && (!Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit)))) return;
            var e = target as Obj_AI_Base;
            if (!ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue || !e.IsEnemy) return;
            if (target == null) return;
            if (e.IsValidTarget() && W.IsReady())
            {
                W.Cast();
            }




        }
    }
}