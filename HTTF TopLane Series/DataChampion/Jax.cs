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
using Circle = EloBuddy.SDK.Rendering.Circle;
using Color = System.Drawing.Color;

namespace HTTF_TopLane_Series.DataChampion
{
    class Jax
    {

        public static Menu Menu, ComboMenu, HarassMenu, LaneClearMenu, JungleClearMenu, Drawings;
        public static Item Botrk;
        public static Item Bil;
        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }
        public static Spell.Targeted Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Active R;
        public static Spell.Targeted Ignite;

        public static void JaxLoading()
        {
            Loading.OnLoadingComplete += OnLoadingComplete;
        }

        static void OnLoadingComplete(EventArgs args)
        {
            if (!_Player.ChampionName.Contains("Jax")) return;
            Q = new Spell.Targeted(SpellSlot.Q, 700);
            W = new Spell.Active(SpellSlot.W);
            E = new Spell.Active(SpellSlot.E, 350);
            R = new Spell.Active(SpellSlot.R);            



            Menu = MainMenu.AddMenu("HTTF Jax", "Jax");
            Menu.AddSeparator();
            ComboMenu = Menu.AddSubMenu("Combo Settings", "Combo");
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("ComboQ", new CheckBox("Combo [Q]"));
            ComboMenu.Add("ComboW", new CheckBox("Combo [W]"));
            ComboMenu.Add("ComboE", new CheckBox("Combo [E]"));
            ComboMenu.AddSeparator();
            ComboMenu.Add("ComboR", new CheckBox("Combo [R]"));
            ComboMenu.Add("MinR", new Slider("Min Enemies Use [R]", 2, 1, 5));
            ComboMenu.AddSeparator();
            ComboMenu.AddGroupLabel("E Setting");
            ComboMenu.Add("antiGap", new CheckBox("Use [E] AntiGapcloser"));
            ComboMenu.Add("minE", new Slider("Min Enemies Auto [E]", 2, 1, 5));
            ComboMenu.AddSeparator();


            HarassMenu = Menu.AddSubMenu("Harass Settings", "Harass");
            HarassMenu.AddGroupLabel("Harass Settings");
            HarassMenu.Add("HarassQ", new CheckBox("Harass [Q]", false));
            HarassMenu.Add("HarassW", new CheckBox("Harass [W]"));
            HarassMenu.Add("HarassE", new CheckBox("Harass [E]"));
            HarassMenu.Add("ManaQ", new Slider("Min Mana For Harass", 30));

            LaneClearMenu = Menu.AddSubMenu("LaneClear Settings", "LaneClear");
            LaneClearMenu.AddGroupLabel("LaneClear Settings");
            LaneClearMenu.Add("WCQ", new CheckBox("Lane Clear [Q]", false));
            LaneClearMenu.Add("WCW", new CheckBox("Lane Clear [W]"));
            LaneClearMenu.Add("WCE", new CheckBox("Lane Clear [E]", false));
            LaneClearMenu.Add("ManaWC", new Slider("Min Mana LaneClear [Q]", 60));


            JungleClearMenu = Menu.AddSubMenu("JungleClear Settings", "JungleClear");
            JungleClearMenu.AddGroupLabel("JungleClear Settings");
            JungleClearMenu.Add("QJngl", new CheckBox("Spell [Q]"));
            JungleClearMenu.Add("WJngl", new CheckBox("Spell [W]"));
            JungleClearMenu.Add("EJngl", new CheckBox("Spell [E]"));
            JungleClearMenu.Add("MnJngl", new Slider("Min Mana For JungleClear", 30));

            Drawings = Menu.AddSubMenu("Draw Settings", "Draw");
            Drawings.AddGroupLabel("Drawing Settings");
            Drawings.Add("DrawQ", new CheckBox("Q Range"));
            Drawings.Add("DrawE", new CheckBox("E Range", false));

            Drawing.OnDraw += Drawing_OnDraw;
            Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnPostAttack += ResetAttack;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Drawings["DrawQ"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = Q.Range }.Draw(_Player.Position);
            }

            if (Drawings["DrawE"].Cast<CheckBox>().CurrentValue)
            {
                new Circle() { Color = Color.GreenYellow, BorderWidth = 1, Radius = E.Range }.Draw(_Player.Position);
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                Harass();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                JungleClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                LaneClear();
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
            {
                Flee();
            }

            Ecast();
        }

        public static bool ECast
        {
            get { return Player.Instance.HasBuff("JaxCounterStrike"); }
        }

        public static void Ecast()
        {
            var MinE = ComboMenu["minE"].Cast<Slider>().CurrentValue;
            var ComboE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;



            if (ComboE && E.IsReady() && _Player.Position.CountEnemyChampionsInRange(E.Range) >= MinE)
            {
                E.Cast();
            }
        }

        public static void Combo()
        {
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            var useQ = ComboMenu["ComboQ"].Cast<CheckBox>().CurrentValue;
            var useE = ComboMenu["ComboE"].Cast<CheckBox>().CurrentValue;
            var useR = ComboMenu["ComboR"].Cast<CheckBox>().CurrentValue;
            var MinR = ComboMenu["MinR"].Cast<Slider>().CurrentValue;
            {
                if (target != null)
                {


                    if (target != null)
                    {
                        if (useQ && Q.IsReady() && Player.Instance.GetAutoAttackRange() < target.Distance(Player.Instance))
                        {
                            if (target.IsValidTarget(Q.Range))
                            {
                                Q.Cast(target);
                            }
                        }

                        if (useE && E.IsReady())
                        {
                            if (!ECast && target.IsValidTarget(E.Range))
                            {
                                E.Cast();
                            }

                            if (ECast && target.IsValidTarget(E.Range))
                            {
                                E.Cast();
                            }
                        }
                    }
                }

                if (useR && R.IsReady())
                {
                    if (_Player.Position.CountEnemyChampionsInRange(Q.Range) >= MinR)
                    {
                        R.Cast();
                    }
                }
            }
        }

        public static void ResetAttack(AttackableUnit e, EventArgs args)
        {
            if (!(e is AIHeroClient)) return;
            var target = TargetSelector.GetTarget(300, DamageType.Physical);
            var champ = (AIHeroClient)e;
            var useW = ComboMenu["ComboW"].Cast<CheckBox>().CurrentValue;
            var HasW = HarassMenu["HarassW"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            if (champ == null || champ.Type != GameObjectType.AIHeroClient || !champ.IsValid) return;
            if (target != null)
            {
                if (useW && W.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }

                if (HasW && W.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && _Player.ManaPercent >= mana)
                {
                    W.Cast();
                    Orbwalker.ResetAutoAttack();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, target);
                }
            }
        }

        public static void JungleClear()
        {
            var useW = JungleClearMenu["WJngl"].Cast<CheckBox>().CurrentValue;
            var useQ = JungleClearMenu["QJngl"].Cast<CheckBox>().CurrentValue;
            var useE = JungleClearMenu["EJngl"].Cast<CheckBox>().CurrentValue;
            var mana = JungleClearMenu["MnJngl"].Cast<Slider>().CurrentValue;
            var jungleMonsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(j => j.Health).FirstOrDefault(j => j.IsValidTarget(Q.Range));
            if (jungleMonsters != null && Player.Instance.ManaPercent >= mana)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(jungleMonsters))
                {
                    Q.Cast(jungleMonsters);
                }

                if (useW && W.IsReady() && jungleMonsters.IsValidTarget(275) && jungleMonsters.IsInAutoAttackRange(Player.Instance) && Player.Instance.Distance(jungleMonsters.ServerPosition) <= 225f)
                {
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, jungleMonsters);
                }

                if (useE && E.IsReady())
                {
                    if (!ECast)
                    {
                        E.Cast();
                    }

                    else if (ECast && jungleMonsters.IsValidTarget(E.Range))
                    {
                        Core.DelayAction(() => E.Cast(), 1000);
                    }
                }
            }
        }

        public static void LaneClear()
        {
            var mana = LaneClearMenu["ManaWC"].Cast<Slider>().CurrentValue;
            var useW = LaneClearMenu["WCW"].Cast<CheckBox>().CurrentValue;
            var useQ = LaneClearMenu["WCQ"].Cast<CheckBox>().CurrentValue;
            var useE = LaneClearMenu["WCE"].Cast<CheckBox>().CurrentValue;
            var minions = ObjectManager.Get<Obj_AI_Base>().OrderBy(m => m.Health).Where(m => m.IsMinion && m.IsEnemy && !m.IsDead);
            if (Player.Instance.ManaPercent <= mana) return;
            foreach (var minion in minions)
            {
                if (useQ && Q.IsReady() && Q.IsInRange(minion) && _Player.GetAutoAttackRange() <= minion.Distance(Player.Instance) && Player.Instance.GetSpellDamage(minion, SpellSlot.Q) >= minion.TotalShieldHealth())
                {
                    Q.Cast(minion);
                }

                if (useE && E.IsReady() && minion.IsValidTarget(E.Range) && _Player.CountEnemyMinionsInRange(E.Range) >= 2)
                {
                    E.Cast();
                }
                if (useW && W.IsReady() && minion.IsValidTarget(275) && minion.IsInAutoAttackRange(Player.Instance)
                && Player.Instance.Distance(minion.ServerPosition) <= 225f
                && Player.Instance.GetSpellDamage(minion, SpellSlot.W) + Player.Instance.GetAutoAttackDamage(minion)
                >= minion.TotalShieldHealth())
                {
                    W.Cast();
                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                }
            }
        }

       

        public static void Harass()
        {
            var useQ = HarassMenu["HarassQ"].Cast<CheckBox>().CurrentValue;
            var useE = HarassMenu["HarassE"].Cast<CheckBox>().CurrentValue;
            var mana = HarassMenu["ManaQ"].Cast<Slider>().CurrentValue;
            var target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
            if (Player.Instance.ManaPercent < mana) return;
            if (target != null)
            {
                if (useE && E.IsReady())
                {
                    if (useQ && !ECast && Q.IsReady() && target.IsValidTarget(Q.Range) && !target.IsValidTarget(E.Range))
                    {
                        E.Cast();
                    }

                    if (!ECast && target.IsValidTarget(E.Range))
                    {
                        E.Cast();
                    }

                    if (ECast && target.IsValidTarget(E.Range))
                    {
                        E.Cast();
                    }
                }

                if (useQ && Q.IsReady() && target.IsValidTarget(Q.Range) && (Player.Instance.GetAutoAttackRange() < target.Distance(Player.Instance) || Player.Instance.HealthPercent <= 25))
                {
                    if (ECast)
                    {
                        Core.DelayAction(() => Q.Cast(target), 500);
                    }
                    else
                    {
                        Q.Cast(target);
                    }
                }
            }
        }

        public static void Flee()
        {
            if (Q.IsReady())
            {
                var CursorPos = Game.CursorPos;
                Obj_AI_Base JumpPlace = EntityManager.Heroes.Allies.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && Q.IsInRange(w));
                if (JumpPlace != default(Obj_AI_Base))
                {
                    Q.Cast(JumpPlace);
                }
                else
                {
                    JumpPlace = EntityManager.MinionsAndMonsters.Minions.FirstOrDefault(w => w.Distance(CursorPos) <= 250 && Q.IsInRange(w));

                    if (JumpPlace != default(Obj_AI_Base))
                    {
                        Q.Cast(JumpPlace);
                    }
                    var Ward2 = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(a => a.IsAlly && a.Distance(CursorPos) < 300);
                    if (Ward2 != null)
                    {
                        Q.Cast(Ward2);
                    }
                    else if (JumpWard() != default(InventorySlot))
                    {
                        var Ward = JumpWard();
                        CursorPos = _Player.Position.Extend(CursorPos, 600).To3D();
                        Ward.Cast(CursorPos);
                        Core.DelayAction(() => WardJump(CursorPos), Game.Ping + 100);
                    }
                }
            }
        }

        public static void WardJump(Vector3 cursorpos)
        {
            var jumpPos = Game.CursorPos;
            var Ward = ObjectManager.Get<Obj_AI_Base>().FirstOrDefault(a => a.IsAlly && a.Distance(cursorpos) < 300);
            if (Ward != null)
            {
                Q.Cast(Ward);
            }
        }

        public static ItemId[] WardIds = {ItemId.Warding_Totem_Trinket, ItemId.Greater_Stealth_Totem_Trinket, ItemId.Greater_Vision_Totem_Trinket, ItemId.Sightstone, ItemId.Ruby_Sightstone, (ItemId) 2043, (ItemId)3340, (ItemId)2303,
                (ItemId) 2049, (ItemId) 2045};

        public static InventorySlot JumpWard()
        {
            return WardIds.Select(wardId => Player.Instance.InventoryItems.FirstOrDefault(a => a.Id == wardId)).FirstOrDefault(slot => slot != null && slot.CanUseItem());
        }

       
            
        
    }
}
