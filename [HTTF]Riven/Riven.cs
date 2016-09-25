using System;
using System.Drawing;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System.Linq;

namespace _HTTF_Riven
{
    internal class Riven
    {
        public static AIHeroClient myHero
        {
            get { return Player.Instance; }
        }
        private static Spell.Targeted ignite;
        public static Spell.Skillshot Flash { get; set; }
        private static readonly AIHeroClient _Player = ObjectManager.Player;
        public static Text Text = new Text("", new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold));
        public static Spell.Active Q = new Spell.Active(SpellSlot.Q, 300);
        public static Spell.Active E = new Spell.Active(SpellSlot.E, 325);
        public static Spell.Active R1 = new Spell.Active(SpellSlot.R);
        public static Spell.Skillshot R2 = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Cone, 250, 1600, 45)


        {
            AllowedCollisionCount = int.MaxValue
        };

        public static Menu Menu, ComboMenu, FarmMenu, MiscMenu, ShieldMenu;
        private static object targetSelector;
        private static readonly float _barLength = 104;
        private static readonly float _xOffset = 2;
        private static readonly float _yOffset = 9;
        private static readonly Vector2 Offset = new Vector2(1, 0);

        public static Spell.Active W
        {
            get
            {
                return new Spell.Active(SpellSlot.W,
                    (uint)
                        (70 + Player.Instance.BoundingRadius +
                         (Player.Instance.HasBuff("RivenFengShuiEngine") ? 195 : 120)));
            }
        }

        public static bool IsRActive
        {
            get
            {
                return ComboMenu["forcedRKeybind"].Cast<KeyBind>().CurrentValue &&
                       ComboMenu["Combo.R"].Cast<CheckBox>().CurrentValue;
            }
        }

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Riven) return;

            Menu = MainMenu.AddMenu("HTTF Riven", "httfRiven");
            Menu.AddLabel("Best Riven Addon Patch +6.19 ");
            Menu.AddLabel("Your comments and questions to the forum ");
            Menu.AddLabel("HELP ME , PM ME. AND MY SKYPE Bynoob_01 ");

            ComboMenu = Menu.AddSubMenu("Combo And Etc", "comboSettings");
            ComboMenu.Add("Combo.Q", new CheckBox("Use Q"));
            ComboMenu.Add("Combo.W", new CheckBox("Use W"));
            ComboMenu.Add("Combo.E", new CheckBox("Use E"));
            ComboMenu.Add("Combo.R2", new CheckBox("Use R (Killable)"));
            ComboMenu.AddLabel("R1 Settings");
            ComboMenu.Add("Combo.R", new CheckBox("Use R"));
            ComboMenu.Add("forcedRKeybind", new KeyBind("Use R in combo?", false, KeyBind.BindTypes.PressToggle, 'T'));
            ComboMenu.AddLabel("When To use R");
            ComboMenu.Add("Combo.RCombo", new CheckBox("Cant Kill with Combo"));
            ComboMenu.Add("Combo.RPeople", new CheckBox("Have more than 1 person near"));
            ComboMenu.AddLabel("Harass Settings");
            ComboMenu.Add("Harass.Q", new CheckBox("Use Q"));
            ComboMenu.Add("Harass.W", new CheckBox("Use W"));
            ComboMenu.Add("Harass.E", new CheckBox("Use E"));
            var Style = ComboMenu.Add("harassstyle", new Slider("Harass Style(Beta)", 0, 0, 3));
            Style.OnValueChange += delegate
            {
                Style.DisplayName = "Harass Style: " + new[] { "Q,Q,W,Q and E ", "E,H,Q3,W", "E,H,AA,Q,W", "E,Q,H,AA,Q,AA,W,AA,Q,AA" }[Style.CurrentValue];
            };
            Style.DisplayName = "Harass Style: " + new[] { "Q,Q,W,Q and E ", "E,H,Q3,W", "E,H,AA,Q,W", "E,Q,H,AA,Q,AA,W,AA,Q,AA" }[Style.CurrentValue];
            
            ComboMenu.AddLabel("Misc Settings");
            ComboMenu.AddLabel("Keep Alive Settings");
            ComboMenu.Add("Alive.Q", new CheckBox("Keep Q Alive"));
            ComboMenu.Add("Alive.R", new CheckBox("Use R2 Before Expire"));
            ComboMenu.AddLabel("Humanizer Settings(BETA)");
            ComboMenu.Add("HumanizerDelay", new Slider("Humanizer Delay (ms)", 0, 0, 300));


            FarmMenu = Menu.AddSubMenu("Clear Settings", "farmSettings");
            FarmMenu.AddLabel("Last Hit");
            FarmMenu.Add("LastHit.Q", new CheckBox("Use Q"));
            FarmMenu.Add("LastHit.W", new CheckBox("Use W"));
            FarmMenu.AddLabel("Wave Clear");
            FarmMenu.Add("WaveClear.Q", new CheckBox("Use Q"));
            FarmMenu.Add("WaveClear.W", new CheckBox("Use W"));
            FarmMenu.AddLabel("Jungle");
            FarmMenu.Add("Jungle.Q", new CheckBox("Use Q"));
            FarmMenu.Add("Jungle.W", new CheckBox("Use W"));
            FarmMenu.Add("Jungle.E", new CheckBox("Use E"));

            MiscMenu = Menu.AddSubMenu("Misc", "Misc");
            MiscMenu.AddLabel("• Draw •");
            MiscMenu.Add("DamageIndicator", new CheckBox("Draw Damage"));
            MiscMenu.AddLabel("• Misc •");
            MiscMenu.Add("gapcloser", new CheckBox("W on enemy gapcloser"));
            MiscMenu.Add("AutoIgnite", new CheckBox("Auto Ignite"));
            MiscMenu.Add("AutoW", new CheckBox("Auto W"));
            MiscMenu.Add("AutoQSS", new CheckBox("Auto QSS"));

            ShieldMenu = Menu.AddSubMenu("AutoShield", "AutoShield");
            ShieldMenu.Add("Shield", new CheckBox("AutoShield"));
            ShieldMenu.Add("Delay", new Slider("Delay For Shield", 0, 0, 500));
            ShieldMenu.AddLabel("•Auto Shield(beta)•");
            foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => a.Team != Player.Instance.Team))
            {
                foreach (
                    var spell in
                        enemy.Spellbook.Spells.Where(
                            a =>
                                a.Slot == SpellSlot.Q || a.Slot == SpellSlot.W || a.Slot == SpellSlot.E ||
                                a.Slot == SpellSlot.R))
                {
                    if (spell.Slot == SpellSlot.Q)
                    {
                        ShieldMenu.Add(spell.SData.Name,
                            new CheckBox(enemy.ChampionName + " - Q - " + spell.Name, false));
                    }
                    else if (spell.Slot == SpellSlot.W)
                    {
                        ShieldMenu.Add(spell.SData.Name,
                            new CheckBox(enemy.ChampionName + " - W - " + spell.Name, false));
                    }
                    else if (spell.Slot == SpellSlot.E)
                    {
                        ShieldMenu.Add(spell.SData.Name,
                            new CheckBox(enemy.ChampionName + " - E - " + spell.Name, false));
                    }
                    else if (spell.Slot == SpellSlot.R)
                    {
                        ShieldMenu.Add(spell.SData.Name,
                            new CheckBox(enemy.ChampionName + " - R - " + spell.Name, false));
                    }
                }
            }


            ItemLogic.Init();
            EventLogic.Init();

            var slot = Player.Instance.GetSpellSlotFromName("summonerflash");

            if (slot != SpellSlot.Unknown)
            {
                Flash = new Spell.Skillshot(slot, 680, SkillShotType.Linear);
            }

            ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);


            Drawing.OnEndScene += Drawing_OnEndScene;
            
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Obj_AI_Base.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;

        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (ComboMenu["Combo.R"].Cast<CheckBox>().CurrentValue)
            {
                var pos = Drawing.WorldToScreen(Player.Instance.Position);
                Text.Draw("Use R in combo?: " + IsRActive, System.Drawing.Color.AliceBlue, (int)pos.X - 45,
                    (int)pos.Y + 40);
            }            
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            if (_Player.IsDead)
                return;
            if (!MiscMenu["DamageIndicator"].Cast<CheckBox>().CurrentValue) return;
            foreach (var aiHeroClient in EntityManager.Heroes.Enemies)
            {
                if (!aiHeroClient.IsHPBarRendered || !aiHeroClient.VisibleOnScreen) continue;
                {
                    if (aiHeroClient.Distance(_Player) < 1500)
                    {
                        foreach (var enemy in EntityManager.Heroes.Enemies.Where(a => !a.IsDead && a.IsHPBarRendered))
                        {
                            var damage = DamageLogic.FastComboDamage(enemy);
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
        private static void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ShieldMenu["Shield"].Cast<CheckBox>().CurrentValue)
                if ((args.Slot == SpellSlot.Q || args.Slot == SpellSlot.W || args.Slot == SpellSlot.E ||
                 args.Slot == SpellSlot.R) && sender.IsEnemy && E.IsReady())
            {
                if (args.SData.TargettingType == SpellDataTargetType.Unit ||
                    args.SData.TargettingType == SpellDataTargetType.SelfAndUnit ||
                    args.SData.TargettingType == SpellDataTargetType.Self)
                {
                    if ((args.Target.NetworkId == Player.Instance.NetworkId && args.Time < 1.5 ||
                         args.End.Distance(Player.Instance.ServerPosition) <= Player.Instance.BoundingRadius * 3) &&
                        ShieldMenu[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast();
                    }
                }
                else if (args.SData.TargettingType == SpellDataTargetType.LocationAoe)
                {
                    var castvector =
                        new Geometry.Polygon.Circle(args.End, args.SData.CastRadius).IsInside(
                            Player.Instance.ServerPosition);

                    if (castvector && ShieldMenu[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                            
                            E.Cast();
                    }
                }

                else if (args.SData.TargettingType == SpellDataTargetType.Cone)
                {
                    var castvector =
                        new Geometry.Polygon.Arc(args.Start, args.End, args.SData.CastConeAngle, args.SData.CastRange)
                            .IsInside(Player.Instance.ServerPosition);

                    if (castvector && ShieldMenu[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast();
                    }
                }

                else if (args.SData.TargettingType == SpellDataTargetType.SelfAoe)
                {
                    var castvector =
                        new Geometry.Polygon.Circle(sender.ServerPosition, args.SData.CastRadius).IsInside(
                            Player.Instance.ServerPosition);

                    if (castvector && ShieldMenu[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast();
                    }
                }
                else
                {
                    var castvector =
                        new Geometry.Polygon.Rectangle(args.Start, args.End, args.SData.LineWidth).IsInside(
                            Player.Instance.ServerPosition);

                    if (castvector && ShieldMenu[args.SData.Name].Cast<CheckBox>().CurrentValue)
                    {
                        E.Cast();
                    }
                }
            }
        }




        private static void Auto()
        {
            var w = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (w.IsValidTarget(W.Range) && MiscMenu["AutoW"].Cast<CheckBox>().CurrentValue)
            {
                W.Cast();
            }
            if (_Player.HasBuffOfType(BuffType.Stun) || _Player.HasBuffOfType(BuffType.Taunt) || _Player.HasBuffOfType(BuffType.Polymorph) || _Player.HasBuffOfType(BuffType.Frenzy) || _Player.HasBuffOfType(BuffType.Fear) || _Player.HasBuffOfType(BuffType.Snare) || _Player.HasBuffOfType(BuffType.Suppression))
            {
                DoQSS();
            }
            {
                if (MiscMenu["AutoIgnite"].Cast<CheckBox>().CurrentValue)
                {
                    if (!ignite.IsReady() || Player.Instance.IsDead) return;
                    foreach (
                        var source in
                            EntityManager.Heroes.Enemies
                                .Where(
                                    a => a.IsValidTarget(ignite.Range) &&
                                        a.Health < 70 + 20 * Player.Instance.Level - (a.HPRegenRate / 5 * 3)))
                    {
                        ignite.Cast(source);
                        return;
                    }
                }
            }
        }

        private static void DoQSS()
        {
            if (!MiscMenu["AutoQSS"].Cast<CheckBox>().CurrentValue) return;

            if (Item.HasItem(3139) && Item.CanUseItem(3139) && ObjectManager.Player.CountEnemiesInRange(1800) > 0)
            {
                Core.DelayAction(() => Item.UseItem(3139), 1);
            }

            if (Item.HasItem(3140) && Item.CanUseItem(3140) && ObjectManager.Player.CountEnemiesInRange(1800) > 0)
            {
                Core.DelayAction(() => Item.UseItem(3140), 1);
            }
        }

        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (myHero.IsDead || !sender.IsEnemy || !sender.IsValidTarget(W.Range) || !W.IsReady() || !MiscMenu["gapcloser"].Cast<CheckBox>().CurrentValue) return;

            W.Cast();
        }
        //Burst


        private static void Game_OnTick(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                StateLogic.Combo();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateLogic.Harass();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                StateLogic.LastHit();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                StateLogic.LaneClear();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                StateLogic.Jungle();
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                StateLogic.Flee();
            }
            Auto();
        }
    }
}

