using System;
using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK.Utils;
using EloBuddy.SDK.Constants;

namespace HTTF_Riven_v2
{
    class Riven
    {


        public static int LastCastW;
        public static int LastCastQ;
        public static int CountQ;
        public static int LastQ;
        public static int LastW;
        public static int LastE;

        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Skillshot E;
        public static Spell.Active R;
        public static Spell.Skillshot R2;
        public static Spell.Targeted Flash;

        public static Item Hydra;
        public static Item Tiamat;
        public static Item Youmu;
        public static Item Qss;
        public static Item Mercurial;

        public static AIHeroClient FocusTarget;

        public static void Load()
        {
            Q = new Spell.Skillshot(SpellSlot.Q, 275, SkillShotType.Circular, 250, 2200, 100);
            W = new Spell.Active(SpellSlot.W, 250);
            E = new Spell.Skillshot(SpellSlot.E, 310, SkillShotType.Linear);
            R = new Spell.Active(SpellSlot.R);
            R2 = new Spell.Skillshot(SpellSlot.R, 900, SkillShotType.Cone, 250, 1600, 125);

            if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner1).Name == "SummonerFlash")
            {
                Flash = new Spell.Targeted(SpellSlot.Summoner1, 425);
            }
            else if (Player.Instance.Spellbook.GetSpell(SpellSlot.Summoner2).Name == "SummonerFlash")
            {
                Flash = new Spell.Targeted(SpellSlot.Summoner2, 425);
            }
            var target = TargetSelector.GetTarget(Riven.E.Range + 200, DamageType.Physical);

            Hydra = new Item((int)ItemId.Ravenous_Hydra, 300);
            Tiamat = new Item((int)ItemId.Tiamat, 300);
            Youmu = new Item((int)ItemId.Youmuus_Ghostblade, 0);
            Qss = new Item((int)ItemId.Quicksilver_Sash, 0);
            Mercurial = new Item((int)ItemId.Mercurial_Scimitar, 0);

            ItemLogic.Init();
            DamageIndicator.Initialize(DamageTotal);
            Game.OnUpdate += Game_OnUpdate;
            Game.OnWndProc += Game_OnWndProc;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
            Obj_AI_Base.OnPlayAnimation += Reset;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;

            Obj_AI_Turret.OnBasicAttack += Obj_AI_Turret_OnBasicAttack2;
            Orbwalker.OnPostAttack += Orbwalker_OnPostAttack;
            Drawing.OnDraw += Drawing_OnDraw;

        }

        private static Vector3 MousePos
        {
            get { return Game.CursorPos; }
        }
        public static void Obj_AI_Turret_OnBasicAttack2(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender is Obj_AI_Turret && sender.Distance(Player.Instance) < 800 && sender.IsAlly)
            {
                if (!(args.Target is AIHeroClient) && args.Target != null)
                {



                    var Minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, 150);
                    foreach (var Minion in Minions)

                        if (Minion != null && args.Target == Minion && Orbwalker.CanAutoAttack)

                        {
                            var AMinions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Ally, Minion.Position, 300).ToList();



                            if (Q.IsReady() && Prediction.Health.GetPrediction(Minion, 930 * (int)(Minion.Distance(sender.Position) / 750)) > Player.Instance.TotalAttackDamage && Prediction.Health.GetPrediction(Minion, 930 * (int)(Minion.Distance(sender.Position) / 750)) + (int)(AMinions.Count * -2) <= sender.TotalAttackDamage * 1.25)
                            {

                                Orbwalker.DisableMovement = true;
                                Core.DelayAction(() => Player.IssueOrder(GameObjectOrder.AttackUnit, args.Target), 0);
                                Core.DelayAction(() => Q.Cast(Minion.ServerPosition), 291);
                                Core.DelayAction(() => Orbwalker.DisableMovement = false, 300);

                                Chat.Print("Last Hitting With AA-Q");
                            }
                            else if (W.IsReady() && Prediction.Health.GetPrediction(Minion, 940 * (int)(Minion.Distance(sender.Position) / 750)) > Player.Instance.TotalAttackDamage && Prediction.Health.GetPrediction(Minion, 940 * (int)(Minion.Distance(sender.Position) / 750)) + (int)(AMinions.Count * -2) <= sender.TotalAttackDamage * 1.25)
                            {

                                Orbwalker.DisableMovement = true;
                                Core.DelayAction(() => Player.IssueOrder(GameObjectOrder.AttackUnit, args.Target), 0);
                                Core.DelayAction(() => Orbwalker.DisableMovement = false, 300);

                                Chat.Print("Last Hitting With AA-w");
                            }







                        }
                }
            }
        }


        private static void Drawing_OnDraw(EventArgs args)
        {


            if (RivenMenu.CheckBox(RivenMenu.Draw, "drawjump"))
            {
                foreach (var spot in WallJump.JumpSpots.Where(s => Player.Instance.Distance(s[0]) <= 1500))
                {
                    Circle.Draw(Color.Green, 60f, spot[0]);
                }
            }
        }







        private static bool HasHydra()
        {
            if (!Hydra.IsOwned() && !RivenMenu.CheckBox(RivenMenu.Misc, "Hydra"))
                return false;

            if (Hydra.IsReady())
            {
                return true;
            }

            return false;
        }

        private static bool HasTiamat()
        {
            if (!Tiamat.IsOwned() && !RivenMenu.CheckBox(RivenMenu.Misc, "Tiamat"))
                return false;

            if (Tiamat.IsReady())
            {
                return true;
            }

            return false;
        }

        private static bool HasYoumu()
        {
            if (!Youmu.IsOwned() && !RivenMenu.CheckBox(RivenMenu.Misc, "Youmu"))
                return false;

            if (Youmu.IsReady())
            {
                return true;
            }

            return false;
        }

        private static bool HasQss()
        {
            if (!Qss.IsOwned() && !RivenMenu.CheckBox(RivenMenu.Misc, "Qss"))
                return false;

            if (Qss.IsReady())
            {
                return true;
            }

            return false;
        }

        private static bool HasMercurial()
        {
            if (!Mercurial.IsOwned() && !RivenMenu.CheckBox(RivenMenu.Misc, "Qss"))
                return false;

            if (Mercurial.IsReady())
            {
                return true;
            }

            return false;
        }

        private static bool CheckUlt()
        {
            if (Player.Instance.HasBuff("RivenFengShuiEngine"))
            {
                return true;
            }
            return false;
        }



        private static void Burst()
        {

            Player.IssueOrder(GameObjectOrder.MoveTo, MousePos);
            var etarget = TargetSelector.GetTarget(625, DamageType.Physical);
            if (etarget == null) return;
            if (!E.IsReady()) return;
            if (!R.IsReady()) return;


                if (etarget.IsValidTarget(600))
                if (!E.IsReady() || !Flash.IsReady() || !R.IsReady());
            if (TargetSelector.SelectedTarget.IsValid);
            {
                    {
                      

                                if (E.IsReady())
                                {
                                    Player.CastSpell(SpellSlot.E, FocusTarget.Position);
                                }

                                if (R.IsReady() && !CheckUlt())
                                {
                                    R.Cast();
                                }

                                if (Flash.IsReady())
                                {
                                    Flash.Cast(FocusTarget.Position);
                                }

                                if (FocusTarget.IsValidTarget(Hydra.Range))
                                {
                                    if (HasTiamat())
                                    {
                                        Tiamat.Cast();
                                    }

                                    if (HasHydra())
                                    {
                                        Hydra.Cast();
                                    }
                                }

                                if (W.IsReady())
                                {
                                    if (FocusTarget.IsValidTarget(W.Range))
                                    {
                                        W.Cast();
                                    }
                                }
                                if (Q.IsReady())
                                {
                                    if (FocusTarget.IsValidTarget(Q.Range))
                                    {
                                        Q.Cast();
                                    }
                                }

                                if (R2.IsReady())
                                {
                                    if (FocusTarget.IsValidTarget(R2.Range))
                                    {
                                        R2.Cast();
                                    }
                                }


                        }
                    }
                }
            
        




        private static void Flee()
        {
            if (RivenMenu.CheckBox(RivenMenu.Combo, "UseQFlee"))
            {
                Q.Cast((Game.CursorPos.Distance(Player.Instance) > Q.Range ? Player.Instance.Position.Extend(Game.CursorPos, Q.Range - 1).To3D() : Game.CursorPos));
            }
            var Target = TargetSelector.GetTarget(R2.Range, DamageType.Physical);
            if (RivenMenu.CheckBox(RivenMenu.Combo, "UseEFlee"))
            {
                if (Target == null || !Target.IsValidTarget(W.Range))
                {
                    E.Cast((Game.CursorPos.Distance(Player.Instance) > E.Range ? Player.Instance.Position.Extend(Game.CursorPos, E.Range - 1).To3D() : Game.CursorPos));
                }
                else if (Target.IsValidTarget(W.Range))
                {
                    E.Cast((Game.CursorPos.Distance(Player.Instance) > E.Range ? Player.Instance.Position.Extend(Game.CursorPos, E.Range - 1).To3D() : Game.CursorPos));
                    Core.DelayAction(() => Player.CastSpell(SpellSlot.W), 40);
                }
            }
        }

        private static void JumpWall()
        {
            if (RivenMenu.CheckBox(RivenMenu.Misc, ("JumpFlee")) && Game.MapId == GameMapId.SummonersRift)
            {
                var spot = WallJump.GetJumpSpot();
                if (spot != null && Riven.CountQ == 2 || Q.IsReady())
                {
                    Orbwalker.DisableAttacking = true;
                    Orbwalker.DisableMovement = true;

                    WallJump.JumpWall();
                    return;
                }
            }
        }


        private static void ChooseR(AIHeroClient Target)
        {
            switch (RivenMenu.ComboBox(RivenMenu.Combo, "UseRType"))
            {
                case 0:

                    if (Target.HealthPercent <= 40)
                    {
                        if (RivenMenu.CheckBox(RivenMenu.Combo, "BrokenAnimations"))
                        {
                            if (W.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseWCombo"))
                            {
                                if (Target.IsValidTarget(W.Range))
                                {
                                    R.Cast();
                                    W.Cast();
                                }
                            }
                            else if (E.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseECombo"))
                            {
                                if (Target.IsValidTarget(E.Range))
                                {
                                    Player.CastSpell(SpellSlot.E, Target.Position);
                                    R.Cast();

                                }
                            }
                        }
                        else
                        {
                            R.Cast();
                        }
                    }

                    break;

                case 1:

                    if (DamageTotal(Target) >= Target.Health)
                    {
                        if (RivenMenu.CheckBox(RivenMenu.Combo, "BrokenAnimations"))
                        {
                            if (W.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseWCombo"))
                            {
                                if (Target.IsValidTarget(W.Range))
                                {
                                    R.Cast();
                                    W.Cast();
                                }
                            }
                            else if (E.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseECombo"))
                            {
                                if (Target.IsValidTarget(E.Range))
                                {
                                    Player.CastSpell(SpellSlot.E, Target.Position);
                                    R.Cast();

                                }
                            }
                        }
                        else
                        {
                            R.Cast();
                        }
                    }

                    break;

                case 2:

                    if (RivenMenu.CheckBox(RivenMenu.Combo, "BrokenAnimations"))
                    {
                        if (W.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseWCombo"))
                        {
                            if (Target.IsValidTarget(W.Range))
                            {
                                R.Cast();
                                W.Cast();
                            }
                        }
                        else if (E.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseECombo"))
                        {
                            if (Target.IsValidTarget(E.Range))
                            {
                                Player.CastSpell(SpellSlot.E, Target.Position);
                                R.Cast();

                            }
                        }
                    }
                    else
                    {
                        R.Cast();
                    }

                    break;

                case 3:

                    if (RivenMenu.Keybind(RivenMenu.Combo, "ForceR"))
                    {
                        R.Cast();
                    }

                    break;
            }
        }

        private static void ChooseR2(AIHeroClient Target)
        {
            switch (RivenMenu.ComboBox(RivenMenu.Combo, "UseR2Type"))
            {
                case 0:


                    if (Target.IsValidTarget(R2.Range))
                    {
                        if (RDamage(Target, Target.Health) * 0.95 >= Target.Health)
                        {
                            var RPred = R2.GetPrediction(Target);

                            if (RPred.HitChance >= HitChance.High)
                            {
                                R2.Cast(RPred.UnitPosition);
                            }
                        }
                    }

                    break;

                case 1:

                    if (Target.IsValidTarget(R2.Range))
                    {
                        var RPred = R2.GetPrediction(Target);

                        if (RPred.HitChance >= HitChance.High)
                        {
                            R2.Cast(RPred.UnitPosition);
                        }
                    }

                    break;
            }
        }


        private static void DpsBurst()
        {
            var target = TargetSelector.GetTarget(W.Range, DamageType.Physical);
            if (target != null)
            {
                E.Cast();
                ItemLogic.Hydra.Cast();
                W.Cast();
                Q.Cast();
                AnimateCAnsl();
                Q.Cast();
                AnimateCAnsl();
                Q.Cast();

                {

                }
            }
        }

        private static void Combo()
        {
            

            var target = TargetSelector.GetTarget(Riven.E.Range  + 200, DamageType.Physical);
            if (target != null)
            {
                if (R.IsReady())
                {
                    if (CheckUlt() == false)
                    {
                        if (target.HealthPercent >= RivenMenu.Slider(RivenMenu.Combo, "DontR1"))
                        
                            {
                                Player.CastSpell(SpellSlot.E, target.Position);
                                ChooseR(target);
                            }
                        
                    }
                }

                if (RivenMenu.CheckBox(RivenMenu.Combo, "UseR2Combo"))
                {
                    if (CheckUlt() == true)
                    {
                        ChooseR2(target);
                    }
                }

                if (target.Distance(Player.Instance) <= E.Range && E.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseECombo"))
                {
                    Player.CastSpell(SpellSlot.E, target.Position);
                    return;
                }
                if (target.Distance(Player.Instance) <= Hydra.Range && Hydra.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseHT"))
                {
                    ItemLogic.Hydra.Cast();
                    return;                        

                }
                {


                    if (HasYoumu())
                    {
                        if (RivenMenu.CheckBox(RivenMenu.Misc, "Youmu"))
                        {
                            Youmu.Cast();
                        }
                    }


                    if (target.Distance(Player.Instance) <= W.Range && W.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseWCombo"))
                    {
                        if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
                        {
                            ItemLogic.Hydra.Cast();
                            return;
                        }
                        Player.CastSpell(SpellSlot.W);
                    }





                }

            }
          }
        



        //New Animation Cansel Function.
        private static void NewComboAnimation()
        {
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "BrokenAnimon"))
            {
                var Target = TargetSelector.GetTarget(Q.Range, DamageType.Physical);
                //q1Hydra
                if (Target != null)
                    if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "Q1Hydra"))

                        if (Q.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseQCombo"))
                        {
                            if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())

                            {

                                ItemLogic.Hydra.Cast();
                                Q.Cast();
                                return;
                            }
                        }
                {
                    if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "EQall"))
                        if (E.IsReady() || Q.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseQCombo"))
                        {
                            E.Cast();
                            Q.Cast();
                            return;
                        }
                    {
                        if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "EH"))
                            if (E.IsReady() || ItemLogic.Hydra.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseECombo"))

                            {
                                E.Cast();
                                ItemLogic.Hydra.Cast();
                                return;
                            }
                        if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "HydraQ"))
                            if (Q.IsReady() || ItemLogic.Hydra.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseQCombo"))

                            {
                                ItemLogic.Hydra.Cast();
                                Q.Cast();
                                return;
                            }
                        if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "HydraW"))
                            if (W.IsReady() || ItemLogic.Hydra.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseWCombo"))

                            {
                                ItemLogic.Hydra.Cast();
                                W.Cast();
                                return;
                            }
                    }
                    if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "QW"))
                        if (Q.IsReady() || W.IsReady())

                        {
                            Q.Cast();
                            W.Cast();
                            return;
                        }
                }
                if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "ER1"))
                    if (R.IsReady() || E.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseRCombo"))

                    {
                        E.Cast();
                        R.Cast();
                        return;
                    }
            }
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "R1W"))
                if (R.IsReady() || W.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseRCombo"))

                {
                    R.Cast();
                    W.Cast();
                    return;
                }
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "R1Q"))
                if (R.IsReady() || Q.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseRCombo"))

                {
                    R.Cast();
                    Q.Cast();
                    return;
                }
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "R1Hydra"))
                if (R.IsReady() || ItemLogic.Hydra.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseRCombo"))

                {
                    R.Cast();
                    ItemLogic.Hydra.Cast();
                    return;
                }
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "ER2"))
                if (E.IsReady() || R2.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseR2Combo"))

                {
                    E.Cast();
                    R2.Cast();
                    return;
                }
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "R2W"))
                if (W.IsReady() || R2.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseR2Combo"))

                {

                    R2.Cast();
                    W.Cast();
                    return;
                }
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "R2Q"))
                if (Q.IsReady() || R2.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseR2Combo"))

                {

                    R2.Cast();
                    Q.Cast();
                    return;
                }
            if (RivenMenu.CheckBox(RivenMenu.ComboLogic, "R2E"))
                if (Q.IsReady() || R2.IsReady() && RivenMenu.CheckBox(RivenMenu.Combo, "UseR2Combo"))

                {

                    R2.Cast();
                    E.Cast();
                    return;
                }
        }




















        private static void Harass()
        {

            var Target = TargetSelector.GetTarget(R2.Range, DamageType.Physical);

            if (Target != null)
            {
                if (Player.Instance.CountEnemiesInRange(Hydra.Range) > 0)
                {
                    if (HasHydra())
                    {
                        Hydra.Cast();
                    }

                    if (HasTiamat())
                    {
                        Tiamat.Cast();
                    }
                }

                if (HasYoumu())
                {
                    if (RivenMenu.CheckBox(RivenMenu.Misc, "YoumuHealth"))
                    {
                        Youmu.Cast();
                    }
                }
                if (Q.IsReady() && CountQ < 2)
                {
                    if (Target.IsValidTarget(Q.Range + 300) && !Target.IsDead)
                    {

                        if (Player.Instance.IsFacing(Target) && ObjectManager.Player.Position.Distance(Target.ServerPosition) > 300)
                        {

                            Q.Cast(Player.Instance.Position.Extend(Target.ServerPosition, 200).To3D());
                        }
                    }
                }
                var EPos = Player.Instance.ServerPosition + (Player.Instance.ServerPosition - Target.ServerPosition);
                if (Player.Instance.IsFacing(Target) && CountQ == 2 && Q.IsReady() && Target.IsValidTarget(Q.Range))
                {

                    {
                        Player.CastSpell(SpellSlot.Q, Target.Position);
                        if (Target.IsValidTarget(W.Range))
                        {
                            Core.DelayAction(() => Player.CastSpell(SpellSlot.E, Game.CursorPos), 1200);
                            Core.DelayAction(() => Player.CastSpell(SpellSlot.W), 1240);
                        }

                    }
                }

            }
        }

        private static void Laneclear()
        {
            Orbwalker.ForcedTarget = null;
            if (Orbwalker.IsAutoAttacking)

                if (ItemLogic.Hydra != null && ItemLogic.Hydra.IsReady())
                {
                    ItemLogic.Hydra.Cast();
                    return;
                }
            return;

             
                
            
        }

        private static void LastHit()
        {


            {
                var mawah = EntityManager.Heroes.Enemies.FirstOrDefault
                (y => !y.IsDead && y.IsInRange(Player.Instance, 800));
                var tawah2 = EntityManager.Turrets.Allies.FirstOrDefault
                (t => !t.IsDead && t.IsInRange(Player.Instance, 800));
                var Minions2 = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.Position, Q.Range * 2 + 125);

                foreach (var Minion2 in Minions2)
                {

                    if (tawah2 == null && Q.IsReady() && ObjectManager.Player.Position.Distance(Minion2.ServerPosition) < ObjectManager.Player.Position.Distance(mawah.ServerPosition))
                    {
                        if (Minion2.IsValidTarget(Q.Range * 2 + 125) && !Minion2.IsDead)
                        {

                            if (Player.Instance.IsFacing(Minion2) && ObjectManager.Player.Position.Distance(Minion2.ServerPosition) > 409 && Minion2.Health - Player.Instance.TotalAttackDamage * 1.2 <= 0)
                            {

                                Q.Cast(Player.Instance.Position.Extend(Minion2.ServerPosition, 200).To3D());
                            }

                        }
                    }

                }
            }
        }



        private static void Jungleclear()
        {
            {
                var Monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters().OrderByDescending(x => x.MaxHealth).FirstOrDefault(x => x.IsValidTarget(E.Range));

                if (Monsters == null)
                    return;

                if (RivenMenu.CheckBox(RivenMenu.Laneclear, "UseWJG"))
                {
                    if (Monsters.IsValidTarget(W.Range))
                    {
                        W.Cast();
                    }
                }

                if (RivenMenu.CheckBox(RivenMenu.Laneclear, "UseEJG"))
                {
                    if (Monsters.IsValidTarget(E.Range))
                    {
                        Player.CastSpell(SpellSlot.E, Monsters.Position);
                    }

                }
            }
            {
                if (ObjectManager.Player.Level <= 1)
                {

                    var jminions = EntityManager.MinionsAndMonsters.GetJungleMonsters(Player.Instance.ServerPosition, 1000, true);
                    foreach (var jungleMobs in jminions.Where(x => x.IsValidTarget(Player.Instance.AttackRange)))
                    {
                        if (jungleMobs == null)
                        {

                            return;
                        }
                        if (jungleMobs != null)
                        {
                            if (jungleMobs.Name == "SRU_RedMini10.1.3" || jungleMobs.Name == "SRU_BlueMini1.1.2" || jungleMobs.Name == "SRU_BlueMini21.1.3")
                            {

                                Player.IssueOrder(GameObjectOrder.AttackUnit, jungleMobs);
                            }
                            else
                            {

                            }
                        }
                    }

                }
            }

        }

        private static void AnimateCAnsl()
        {
            Player.DoEmote(Emote.Joke);

        }
        private static void Reset(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe)
                return;

            var T = 0;

            switch (args.Animation)
            {
                case "Spell1a":

                    LastQ = Core.GameTickCount;
                    CountQ = 1;
                    T = 291;

                    break;

                case "Spell1b":

                    LastQ = Core.GameTickCount;
                    CountQ = 2;
                    T = 291;

                    break;

                case "Spell1c":

                    LastQ = 0;
                    CountQ = 0;
                    T = 393;

                    break;

                case "Spell2":
                    T = 170;

                    break;

                case "Spell3":

                    break;
                case "Spell4a":
                    T = 0;

                    break;
                case "Spell4b":
                    T = 150;

                    break;
            }

            if (T != 0)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    Orbwalker.ResetAutoAttack();
                    Core.DelayAction(CancelAnimation, T - Game.Ping);
                }
                else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    Orbwalker.ResetAutoAttack();
                    Core.DelayAction(CancelAnimation, T - Game.Ping);
                }
            }
        }

        private static void Game_OnWndProc(WndEventArgs args)
        {
            if (args.Msg != 0x202)
                return;

            FocusTarget = EntityManager.Heroes.Enemies.FindAll(x => x.IsValid || x.Distance(Game.CursorPos) < 3000 || x.IsVisible || x.Health > 0).OrderBy(x => x.Distance(Game.CursorPos)).FirstOrDefault();
        }

        private static void Orbwalker_OnPostAttack(AttackableUnit target, EventArgs args)
        {
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                if (RivenMenu.CheckBox(RivenMenu.Combo, "UseQCombo") && Q.IsReady())
                {
                    if (CountQ == 0 || !Orbwalker.IsAutoAttacking)
                    {

                        Q.Cast(target.Position);

                    }

                    if (CountQ == 1 || !Orbwalker.IsAutoAttacking)
                    {

                        Q.Cast(target.Position);

                    }

                    if (CountQ == 2 || !Orbwalker.IsAutoAttacking)
                    {

                        Q.Cast(target.Position);
                        CancelAnimation();
                    }
                }
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && target.Type == GameObjectType.AIHeroClient)
            {
                if (RivenMenu.CheckBox(RivenMenu.Combo, "UseQCombo") && Q.IsReady())
                {
                    if (CountQ == 0 || !Orbwalker.IsAutoAttacking)
                    {

                        Q.Cast(target.Position);

                    }

                    if (CountQ == 1 || !Orbwalker.IsAutoAttacking)
                    {

                        Q.Cast(target.Position);

                    }

                    if (CountQ == 2 || !Orbwalker.IsAutoAttacking)
                    {

                        Q.Cast(target.Position);

                    }
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                if (RivenMenu.CheckBox(RivenMenu.Laneclear, "UseQLane") && Q.IsReady())
                {
                    if (CountQ == 0 || !Orbwalker.IsAutoAttacking)
                    {
                        Q.Cast(target.Position);
                    }

                    if (CountQ == 1 || !Orbwalker.IsAutoAttacking)
                    {
                        Q.Cast(target.Position);
                    }

                    // if (CountQ == 2 || !Orbwalker.IsAutoAttacking)
                    //{
                    //   Q.Cast(target.Position);
                    //}
                }
            }

            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
            {
                if (RivenMenu.CheckBox(RivenMenu.Laneclear, "UseQJG") && Q.IsReady())
                {
                    if (CountQ == 0 || !Orbwalker.IsAutoAttacking)
                    {
                        Q.Cast(target.Position);
                    }

                    if (CountQ == 1 || !Orbwalker.IsAutoAttacking)
                    {
                        Q.Cast(target.Position);
                    }

                    if (CountQ == 2 || !Orbwalker.IsAutoAttacking)
                    {
                        Q.Cast(target.Position);
                    }
                }
            }
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (Player.Instance.IsDead)
                return;

        

            if (Player.Instance.HasBuffOfType(BuffType.Charm) && RivenMenu.CheckBox(RivenMenu.Misc, "QssCharm"))
            {
                if (HasQss())
                {
                    Qss.Cast();
                }

                if (HasMercurial())
                {
                    Mercurial.Cast();
                }
            }
            else if (Player.Instance.HasBuffOfType(BuffType.Charm) && RivenMenu.CheckBox(RivenMenu.Misc, "QssFear"))
            {
                if (HasQss())
                {
                    Qss.Cast();
                }

                if (HasMercurial())
                {
                    Mercurial.Cast();
                }
            }
            else if (Player.Instance.HasBuffOfType(BuffType.Charm) && RivenMenu.CheckBox(RivenMenu.Misc, "QssTaunt"))
            {
                if (HasQss())
                {
                    Qss.Cast();
                }

                if (HasMercurial())
                {
                    Mercurial.Cast();
                }
            }
            else if (Player.Instance.HasBuffOfType(BuffType.Charm) && RivenMenu.CheckBox(RivenMenu.Misc, "QssSuppression"))
            {
                if (HasQss())
                {
                    Qss.Cast();
                }

                if (HasMercurial())
                {
                    Mercurial.Cast();
                }
            }
            else if (Player.Instance.HasBuffOfType(BuffType.Snare) && RivenMenu.CheckBox(RivenMenu.Misc, "QssSnare"))
            {
                if (HasQss())
                {
                    Qss.Cast();
                }

                if (HasMercurial())
                {
                    Mercurial.Cast();
                }
            }

            if (RivenMenu.CheckBox(RivenMenu.Misc, "Skin"))
            {
                Player.Instance.SetSkinId(RivenMenu.Slider(RivenMenu.Misc, "SkinID"));
            }



            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            
                {
                    Combo();
                }


                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    Flee();
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    Laneclear();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    LastHit();
                }

                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    Jungleclear();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    Harass();
                }
            
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            {
                if (!sender.IsMe) return;

                if (args.SData.Name.ToLower().Contains(Riven.W.Name.ToLower()))
                {
                    LastCastW = Environment.TickCount;
                    return;
                }

                if (args.SData.Name.ToLower().Contains(Riven.Q.Name.ToLower()))
                {
                    LastCastQ = Environment.TickCount;
                    if (RivenMenu.CheckBox(RivenMenu.Misc, "AliveQ")) return;
                    Core.DelayAction(() =>
                    {
                        if (!Player.Instance.IsRecalling() && CountQ < 2)
                        {
                            Player.CastSpell(SpellSlot.Q,
                                Orbwalker.LastTarget != null && Orbwalker.LastAutoAttack - Environment.TickCount < 3000
                                    ? Orbwalker.LastTarget.Position
                                    : Game.CursorPos);
                        }
                    }, 3480);
                    return;
                }
            

        }

            {
                if (sender.IsMe || sender.IsAlly || sender == null)
                    return;

                var EPos = Player.Instance.ServerPosition + (Player.Instance.ServerPosition - sender.ServerPosition);

                if (Player.Instance.IsValidTarget(args.SData.CastRange))
                {
                    if (args.Slot == SpellSlot.Q)
                    {
                        if (RivenMenu.CheckBox(RivenMenu.Shield, "E/" + sender.BaseSkinName + "/Q"))
                        {
                            if (args.SData.TargettingType == SpellDataTargetType.Unit)
                            {
                                if (Player.Instance.NetworkId == args.Target.NetworkId)
                                {
                                    E.Cast(EPos);
                                }
                            }
                            else if (args.SData.TargettingType == SpellDataTargetType.SelfAoe)
                            {
                                E.Cast(EPos);
                            }
                            else
                            {
                                E.Cast(EPos);
                            }
                        }
                    }

                    if (args.Slot == SpellSlot.W)
                    {
                        if (RivenMenu.CheckBox(RivenMenu.Shield, "E/" + sender.BaseSkinName + "/W"))
                        {
                            if (args.SData.TargettingType == SpellDataTargetType.Unit)
                            {
                                if (Player.Instance.NetworkId == args.Target.NetworkId)
                                {
                                    E.Cast(EPos);
                                }
                            }
                            else if (args.SData.TargettingType == SpellDataTargetType.SelfAoe)
                            {
                                E.Cast(EPos);
                            }
                            else
                            {
                                E.Cast(EPos);
                            }
                        }
                    }



                    if (args.Slot == SpellSlot.E)
                    {
                        if (RivenMenu.CheckBox(RivenMenu.Shield, "E/" + sender.BaseSkinName + "/E"))
                        {
                            if (args.SData.TargettingType == SpellDataTargetType.Unit)
                            {
                                if (Player.Instance.NetworkId == args.Target.NetworkId)
                                {
                                    E.Cast(EPos);
                                }
                            }
                            else if (args.SData.TargettingType == SpellDataTargetType.SelfAoe)
                            {
                                E.Cast(EPos);
                            }
                            else
                            {
                                E.Cast(EPos);
                            }
                        }
                    }

                    if (args.Slot == SpellSlot.R)
                    {
                        if (RivenMenu.CheckBox(RivenMenu.Shield, "E/" + sender.BaseSkinName + "/R"))
                        {
                            if (args.SData.TargettingType == SpellDataTargetType.Unit)
                            {
                                if (Player.Instance.NetworkId == args.Target.NetworkId)
                                {
                                    E.Cast(EPos);
                                }
                            }
                            else if (args.SData.TargettingType == SpellDataTargetType.SelfAoe)
                            {
                                E.Cast(EPos);
                            }
                            else
                            {
                                E.Cast(EPos);
                            }
                        }
                    }
                }
            }
        }


        private static void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (sender.IsMe || sender.IsAlly || sender == null)
                return;

            if (!RivenMenu.CheckBox(RivenMenu.Misc, "Gapcloser"))
                return;

            if (RivenMenu.CheckBox(RivenMenu.Misc, "GapcloserW"))
            {
                if (sender.IsValidTarget(W.Range))
                {
                    if (W.IsReady())
                    {
                        W.Cast();
                    }
                }
            }
        }

        private static void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            if (sender.IsMe || sender.IsAlly || sender == null)
                return;

            if (!RivenMenu.CheckBox(RivenMenu.Misc, "Interrupter"))
                return;

            if (RivenMenu.CheckBox(RivenMenu.Misc, "InterrupterW"))
            {
                if (sender.IsValidTarget(W.Range))
                {
                    W.Cast();
                }
            }
        }



        private static void CancelAnimation()
        {
            Player.DoEmote(Emote.Dance);
            Orbwalker.ResetAutoAttack();
        }

        public static float DamageTotal(AIHeroClient target)
        {
            double dmg = 0;
            var passiveStacks = 0;

            dmg += Q.IsReady()
                ? QDamage(!CheckUlt()) * (3 - CountQ)
                : 0;
            passiveStacks += Q.IsReady()
                ? (3 - CountQ)
                : 0;

            dmg += W.IsReady()
                ? WDamage()
                : 0;
            passiveStacks += W.IsReady()
                ? 1
                : 0;
            passiveStacks += E.IsReady()
                ? 1
                : 0;

            dmg += PassiveDamage() * passiveStacks;
            dmg += (R.IsReady() && !CheckUlt() && !Player.Instance.HasBuff("RivenFengShuiEngine")
                ? Player.Instance.TotalAttackDamage * 1.2
                : Player.Instance.TotalAttackDamage) * passiveStacks;

            if (dmg < 10)
            {
                return 0 * Player.Instance.TotalAttackDamage;
            }

            dmg += R.IsReady() && !CheckUlt()
                ? RDamage(target, Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, (float)dmg))
                : 0;
            return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, (float)dmg);
        }

        public static float QDamage(bool useR = false)
        {
            return (float)(new double[] { 10, 30, 50, 70, 90 }[Q.Level - 1] +
                            ((Riven.R.IsReady() && useR && !Player.Instance.HasBuff("RivenFengShuiEngine")
                                ? Player.Instance.TotalAttackDamage * 1.2
                                : Player.Instance.TotalAttackDamage) / 100) *
                            new double[] { 40, 45, 50, 55, 60 }[Q.Level - 1]);
        }


        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            var target = args.Target as Obj_AI_Base;

            {
                Orbwalker.ResetAutoAttack();
                return;
            }

            if (args.SData.Name.ToLower().Contains("itemtiamatcleave"))
            {
                Orbwalker.ResetAutoAttack();
                if (Riven.W.IsReady())
                {
                    var target2 = TargetSelector.GetTarget(Riven.W.Range, DamageType.Physical);
                    if (target2 != null || Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None)
                    {
                        Player.CastSpell(SpellSlot.W);
                    }
                }
                return;
            }
        }

        public static float WDamage()
        {
            return (float)(new double[] { 50, 80, 110, 140, 170 }[W.Level - 1] +
                            1 * ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static double PassiveDamage()
        {
            return ((20 + ((Math.Floor((double)ObjectManager.Player.Level / 3)) * 5)) / 100) *
                   (ObjectManager.Player.BaseAttackDamage + ObjectManager.Player.FlatPhysicalDamageMod);
        }

        public static float RDamage(Obj_AI_Base target, float healthMod = 0f)
        {
            if (target != null)
            {
                float missinghealth = (target.MaxHealth - healthMod) / target.MaxHealth > 0.75f ? 0.75f : (target.MaxHealth - healthMod) / target.MaxHealth;
                float pluspercent = missinghealth * (8f / 3f);
                var rawdmg = new float[] { 100, 150, 200 }[R.Level - 1] + 0.6f * Player.Instance.FlatPhysicalDamageMod;
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, rawdmg * (1 + pluspercent));
            }
            return 0f;
        }
        public static float SpellQDamage(Obj_AI_Base target, float healthMod = 0f)
        {
            if (target != null)
            {
                float missinghealth = (target.MaxHealth - healthMod) / target.MaxHealth > 0.75f ? 0.75f : (target.MaxHealth - healthMod) / target.MaxHealth;
                float pluspercent = missinghealth * (8f / 3f);
                var rawdmg = new float[] { 10, 30, 50, 70, 90 }[Q.Level - 1] + 0.4f * Player.Instance.FlatPhysicalDamageMod;
                return Player.Instance.CalculateDamageOnUnit(target, DamageType.Physical, rawdmg);
            }
            return 0f;
        }
    }
}