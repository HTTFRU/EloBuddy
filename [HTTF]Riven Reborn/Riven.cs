using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
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
        private static readonly float _yOffset = 10;
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
        public static Item Youmu;
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Chat.Print("Riven HTTF Active Version 7.7+");
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.Hero != Champion.Riven) return;

            Menu = MainMenu.AddMenu("HTTF Riven", "httfRiven");
            Menu.AddLabel("Best Riven Addon Patch +7.7 ");
            Menu.AddLabel("Your comments and questions to the forum ");
            

            ComboMenu = Menu.AddSubMenu("Combo And Etc", "comboSettings");
            ComboMenu.Add("Combo.Q", new CheckBox("Use Q"));
            ComboMenu.Add("Combo.W", new CheckBox("Use W"));
            ComboMenu.Add("Combo.E", new CheckBox("Use E"));
            ComboMenu.Add("Combo.R2", new CheckBox("Use R (Killable)"));
            ComboMenu.AddLabel("R1 Settings");
            ComboMenu.Add("Combo.R", new CheckBox("Use R"));
            ComboMenu.Add("forcedRKeybind", new KeyBind("Use R in combo?", false, KeyBind.BindTypes.PressToggle, 'T'));
            ComboMenu.AddLabel("R2 Settings");
            ComboMenu.Add("BoxBoxLogicR2", new CheckBox("BoxboxLogicR2"));
            ComboMenu.Add("R2Mode", new ComboBox("R2 Mode:", 0, "Kill Only", "Max Damage"));


            ComboMenu.AddLabel("Burst = Select Target And Burst Key");
            ComboMenu.AddLabel("The flash has usesh");
            ComboMenu.AddLabel("If not perform without a flash");
            ComboMenu.Add("BurstBox", new KeyBind("Shy burst", false, KeyBind.BindTypes.HoldActive, 'G'));
            ComboMenu.Add("BurstKor", new KeyBind("E>R2>Flash>Q>W", false, KeyBind.BindTypes.HoldActive, 'J'));




            ComboMenu.AddLabel("When To use R");
            ComboMenu.Add("Combo.RCombo", new CheckBox("Cant Kill with Combo"));
            ComboMenu.Add("Combo.RPeople", new CheckBox("Have more than 1 person near"));
            ComboMenu.AddLabel("Harass Settings");
            ComboMenu.Add("Harass.Q", new CheckBox("Use Q"));
            ComboMenu.Add("Harass.W", new CheckBox("Use W"));
            ComboMenu.Add("Harass.E", new CheckBox("Use E"));
            var Style = ComboMenu.Add("harassstyle", new Slider("Harass Style", 0, 0, 3));
            Style.OnValueChange += delegate
            {
                Style.DisplayName = "Harass Style: " + new[] { "Q,Q,W,Q and E ", "E,H,Q3,W", "E,H,AA,Q,W", "E,Q,H,AA,Q,AA,W,AA,Q,AA" }[Style.CurrentValue];
            };
            Style.DisplayName = "Harass Style: " + new[] { "Q,Q,W,Q and E ", "E,H,Q3,W", "E,H,AA,Q,W", "E,Q,H,AA,Q,AA,W,AA,Q,AA" }[Style.CurrentValue];
            
            ComboMenu.AddLabel("Misc Settings");
            ComboMenu.AddLabel("Keep Alive Settings");
            ComboMenu.Add("Alive.Q", new CheckBox("Keep Q Alive"));
            ComboMenu.Add("Alive.R", new CheckBox("Use R2 Before Expire"));
            
            ComboMenu.AddLabel("Speed Combo");
            ComboMenu.AddLabel("0 = Challenger Combo 1.5 sec");
            ComboMenu.AddLabel("50 = Master Combo 2 sec");
            ComboMenu.AddLabel("100-200 = Diamond Combo 2.5 sec");
            ComboMenu.AddLabel("200-300 = Plat-Gold Combo 3 sec");
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
            MiscMenu.Add("Youmu", new CheckBox("Use Youmuu?"));
            MiscMenu.AddLabel("• SkinHack •");
            MiscMenu.Add("checkSkin", new CheckBox("Use Skin Changer"));
            MiscMenu.Add("Skinid", new Slider("Skin ID", 0, 0, 11));
            ShieldMenu = Menu.AddSubMenu("AutoShield", "AutoShield");
            ShieldMenu.Add("Shield", new CheckBox("AutoShield"));
            ShieldMenu.Add("AutoDiels", new CheckBox("AutoDelay(Humanizer)"));
            ShieldMenu.AddLabel("•Auto Shield(beta)•");
            foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(enemy => enemy.IsEnemy))
            {
                for (var i = 0; i < 4; i++)
                {
                    var spell = enemy.Spellbook.Spells[i];
                    if (spell.SData.TargettingType != SpellDataTargetType.Self && spell.SData.TargettingType != SpellDataTargetType.SelfAndUnit)
                    {
                        if (spell.SData.TargettingType == SpellDataTargetType.Unit)
                            ShieldMenu.Add("Shield" + spell.SData.Name, new CheckBox(spell.Name, true));
                        else
                            ShieldMenu.Add("Shield" + spell.SData.Name, new CheckBox(spell.Name, false));
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

            Youmu = new Item((int)ItemId.Youmuus_Ghostblade, 0);

            Drawing.OnEndScene += Drawing_OnEndScene;

            Game.OnTick += Game_OnTick;
            
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;

            Game.OnUpdate += OnGameUpdate;

            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;


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
        static bool UseE { get { return getCheckBoxItem(ShieldMenu, "Shield"); } }
        static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (ShieldMenu["Shield" + args.SData.Name] != null && !getCheckBoxItem(ShieldMenu, "Shield" + args.SData.Name))
                return;
            if (sender != null && args.Target != null && sender.Type == GameObjectType.AIHeroClient && args.Target.IsMe && sender.IsEnemy && UseE && E.IsReady())
            {
                if (!args.SData.ConsideredAsAutoAttack)
                {
                    if (!args.SData.Name.Contains("summoner") && !args.SData.Name.Contains("TormentedSoil"))
                    {
                        E.Cast();
                    }
                }
                else if (args.SData.Name == "BlueCardAttack" || args.SData.Name == "RedCardAttack" || args.SData.Name == "GoldCardAttack")
                {
                    E.Cast();
                }
            }
            else if (CanHitSkillShot(ObjectManager.Player, args) && !sender.IsMe && sender.Type == GameObjectType.AIHeroClient && E.IsReady())
            {
                E.Cast();
            }
        }
        static bool CanHitSkillShot(Obj_AI_Base target, GameObjectProcessSpellCastEventArgs args)
        {
            if (args.Target == null && target.IsValidTarget(float.MaxValue))
            {
                int Collide = 0;
                if (args.SData.LineMissileEndsAtTargetPoint)
                {
                    Collide = 0;
                }
                else
                {
                    Collide = 1;
                }
                var pred = Prediction.Position.PredictLinearMissile(target, args.SData.CastRange, (int)args.SData.CastRadius, (int)args.SData.CastTime * 1000, args.SData.MissileSpeed, Collide).CastPosition;
                if (pred == null)

                    return false;
                if (args.SData.LineWidth > 0)
                {
                    var powCalc = Math.Pow(args.SData.LineWidth + target.BoundingRadius, 2);
                    if (pred.To2D().Distance(args.End.To2D(), args.Start.To2D(), true, true) <= powCalc ||
                        target.ServerPosition.To2D().Distance(args.End.To2D(), args.Start.To2D(), true, true) <= powCalc)
                    {
                        return true;
                    }
                }
                else if (target.Distance(args.End) < 50 + target.BoundingRadius ||
                         pred.Distance(args.End) < 50 + target.BoundingRadius)
                {
                    return true;
                }
            }
            return false;
        }

        static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }


        public static bool HasYoumu()
        {
            if (Youmu.IsOwned() && MiscMenu["Youmu"].Cast<CheckBox>().CurrentValue)
                return false;

            if (Youmu.IsReady())
            {
                return true;
            }

            return false;
        }


        private static void Auto()
        {
            var w = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (w.IsValidTarget(W.Range) && MiscMenu["AutoW"].Cast<CheckBox>().CurrentValue)
            {
                W.Cast();
            }
            if (_Player.HasBuffOfType(BuffType.Stun) || _Player.HasBuffOfType(BuffType.Taunt) || _Player.HasBuffOfType(BuffType.Polymorph)  || _Player.HasBuffOfType(BuffType.Fear) || _Player.HasBuffOfType(BuffType.Snare) || _Player.HasBuffOfType(BuffType.Suppression))
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
                                        a.Health < 50 + 20 * Player.Instance.Level - (a.HPRegenRate / 5 * 3)))
                    {
                        ignite.Cast(source);
                        return;
                    }
                }
            }
        }
        private static
            void OnGameUpdate(EventArgs args)
        {
            if (CheckSkin())
            {
                EloBuddy.Player.SetSkinId(SkinId());
            }
        }

        private static int SkinId()
        {
            return MiscMenu["Skinid"].Cast<Slider>().CurrentValue;
        }

        private static bool CheckSkin()
        {
            return MiscMenu["checkSkin"].Cast<CheckBox>().CurrentValue;
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
        private static bool forceR;
        private static void Burst()
        {
            var target = TargetSelector.SelectedTarget;
            Orbwalker.ForcedTarget = target;
            Orbwalker.OrbwalkTo(target.ServerPosition);
            if (target == null || target.IsZombie || target.IsInvulnerable) return;
            if (target.IsValidTarget(800))

            {
                if (E.IsReady())
                {
                    Youmu.Cast(target);
                    Player.CastSpell(SpellSlot.E, target.ServerPosition);
                }
                Youmu.Cast(target);

                if (R1.IsReady() && ComboMenu["BurstBox"].Cast<KeyBind>().CurrentValue && forceR == false)
                {
                    R1.Cast();
                }

                if (Flash.IsReady() && (myHero.Distance(target.Position) <= 600))
                {
                    Flash.Cast(target.ServerPosition);
                }

                ItemLogic.Hydra.Cast(target);

                if (target.IsValidTarget(W.Range))
                {
                    if (W.IsReady())

                    {
                        W.Cast();
                    }

                    if (R2.IsReady())

                    {
                        R2.Cast(target.ServerPosition);
                    }

                }
            }
        }

        private static void BurstFlash()
        {
            
            var target = TargetSelector.SelectedTarget;
            Orbwalker.ForcedTarget = target;
            Orbwalker.OrbwalkTo(target.ServerPosition);
            if (target == null || target.IsZombie || target.IsInvulnerable) return;
            if (target.IsValidTarget(800))

            {
                if (R2.Cast(target.ServerPosition))
                {
                    E.Cast(target.ServerPosition);
                }
               

                if ( ComboMenu["BurstKor"].Cast<KeyBind>().CurrentValue && forceR == false)

                {

                }

                if (Flash.IsReady() && (myHero.Distance(target.Position) <= 600))
                {
                    R2.Cast(target.ServerPosition);
                    Flash.Cast(target.ServerPosition);
                }

                ItemLogic.Hydra.Cast(target);

                if (target.IsValidTarget(Q.Range))
                {
                    if (Q.IsReady())

                    {
                        Q.Cast();
                    }

                    if (W.IsReady())

                    {
                        W.Cast(target.ServerPosition);
                    }

                }
            }
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
            if (ComboMenu["BurstBox"].Cast<KeyBind>().CurrentValue) Burst();
            if (ComboMenu["BurstKor"].Cast<KeyBind>().CurrentValue) BurstFlash();
        }
    }
}

