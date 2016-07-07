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
            Menu.AddLabel("Best Riven Addon Patch +6.13 ");
            Menu.AddLabel("Your comments and questions to the forum ");

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
            MiscMenu.Add("Skin", new CheckBox("Skinhack ?", false));
            MiscMenu.Add("SkinID", new Slider("Skin ID: {0}", 4, 0, 6));

            ShieldMenu = Menu.AddSubMenu("AutoShield", "AutoShield");
            ShieldMenu.AddLabel("• Write to me in forum what (so I added them to the database) auto skills Shield •");



            ItemLogic.Init();
            EventLogic.Init();

            ignite = new Spell.Targeted(ObjectManager.Player.GetSpellSlotFromName("summonerdot"), 600);


            Drawing.OnEndScene += Drawing_OnEndScene;
            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnTick += Game_OnTick;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Game.OnUpdate += Game_OnUpdate;
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



                        var pos = new Vector2(aiHeroClient.HPBarPosition.X + _xOffset, aiHeroClient.HPBarPosition.Y + _yOffset);
                        var fullbar = (_barLength) * (aiHeroClient.HealthPercent / 100);
                        var damage = (_barLength) *
                                         ((DamageLogic.FastComboDamage(aiHeroClient) / aiHeroClient.MaxHealth) > 1
                                             ? 1
                                             : (DamageLogic.FastComboDamage(aiHeroClient) / aiHeroClient.MaxHealth));
                        Line.DrawLine(System.Drawing.Color.Green, 9f, new Vector2(pos.X, pos.Y),
                            new Vector2(pos.X + (damage > fullbar ? fullbar : damage), pos.Y));
                        Line.DrawLine(System.Drawing.Color.Black, 9, new Vector2(pos.X + (damage > fullbar ? fullbar : damage) - 2, pos.Y), new Vector2(pos.X + (damage > fullbar ? fullbar : damage) + 2, pos.Y));
                    }
                }
            }
        }


        private static void Game_OnUpdate(EventArgs args)
        {
            if (MiscMenu["Skin"].Cast<CheckBox>().CurrentValue)
            {
                Player.Instance.SetSkinId(MiscMenu["skinid"].Cast<Slider>().CurrentValue);
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

