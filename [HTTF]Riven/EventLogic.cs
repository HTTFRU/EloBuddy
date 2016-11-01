using System;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Menu.Values;
using System.Linq;

namespace _HTTF_Riven
{ 
    
    internal class EventLogic

    {
        public static int LastCastQ;
        public static int LastCastW;
        public static int QCount;

        public static void Init()
        {
            Obj_AI_Base.OnSpellCast += Obj_AI_Base_OnSpellCast;
            Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            if (LastCastQ + 3600 < Environment.TickCount)
            {
                QCount = 0;
            }
            if (Riven.ComboMenu["Alive.Q"].Cast<CheckBox>().CurrentValue && !Player.Instance.IsRecalling() && QCount < 3 && LastCastQ + 3480 < Environment.TickCount && Player.Instance.HasBuff("RivenTriCleaveBuff"))
            {
                Player.CastSpell(SpellSlot.Q,
                    Orbwalker.LastTarget != null && Orbwalker.LastAutoAttack - Environment.TickCount < 3000
                        ? Orbwalker.LastTarget.Position
                        : Game.CursorPos);
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
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
                if (!Riven.ComboMenu["Alive.Q"].Cast<CheckBox>().CurrentValue) return;
                Core.DelayAction(() =>
                {
                    if (!Player.Instance.IsRecalling() && QCount < 2)
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

        private static void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
        {
            if (!sender.IsMe) return;
            var t = 0;
            switch (args.Animation)
            {
                case "Spell1a":
                    t = 111;
                    QCount = 1;
                    break;
                case "Spell1b":
                    t = 111;
                    QCount = 2;
                    break;
                case "Spell1c":
                    t = 111;
                    QCount = 0;
                    break;
                case "Spell2":
                    t = 111;
                    break;
                case "Spell3":
                    break;
                case "Spell4a":
                    t = 0;
                    break;
                case "Spell4b":
                    t = 100;
                    break;
            }
            if (t != 0 && (Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None))
            {
                Orbwalker.ResetAutoAttack();
                Core.DelayAction(CancelAnimation, t - Game.Ping);
            }
        }

        private static void CancelAnimation()
        {
            Player.DoEmote(Emote.Laugh);
            Orbwalker.ResetAutoAttack();
        }

        private static void Obj_AI_Base_OnSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            var target = args.Target as Obj_AI_Base;

            // Hydra
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

            //W
            if (args.SData.Name.ToLower().Contains(Riven.W.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && Riven.R2.IsReady() &&
                        Riven.ComboMenu["Combo.R2"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(Riven.R2.Range, DamageType.Physical);
                        if (target2 != null &&
                            (target2.Distance(Player.Instance) < Riven.W.Range &&
                             target2.Health >
                             Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical, DamageLogic.WDamage()) ||
                             target2.Distance(Player.Instance) > Riven.W.Range) &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                DamageLogic.RDamage(target2) + DamageLogic.WDamage()) > target2.Health)
                        {
                            Riven.R2.Cast(target2);
                        }
                    }
                }

                target = (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo) ||
                          Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    ? TargetSelector.GetTarget(Riven.E.Range + Riven.W.Range, DamageType.Physical)
                    : (Obj_AI_Base)Orbwalker.LastTarget;
                if (Riven.Q.IsReady() && Orbwalker.ActiveModesFlags != Orbwalker.ActiveModes.None && target != null)
                {
                    Player.CastSpell(SpellSlot.Q, target.Position);
                    return;
                }
                return;
            }

            //E
            if (args.SData.Name.ToLower().Contains(Riven.E.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && Riven.R2.IsReady() &&
                        Riven.ComboMenu["Combo.R2"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(Riven.R2.Range, DamageType.Physical);
                        if (target2 != null &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                (DamageLogic.RDamage(target2))) > target2.Health)
                        {
                            Riven.R2.Cast(target2);
                            return;
                        }
                    }
                    if ((Riven.IsRActive || StateLogic.EnableR) && Riven.R1.IsReady() &&
                        !Player.Instance.HasBuff("RivenFengShuiEngine") &&
                        Riven.ComboMenu["Combo.R"].Cast<CheckBox>().CurrentValue)
                    {
                        Player.CastSpell(SpellSlot.R);
                    }
                    target = TargetSelector.GetTarget(Riven.W.Range, DamageType.Physical);
                    if (target != null && Player.Instance.Distance(target) < Riven.W.Range)
                    {
                        Player.CastSpell(SpellSlot.W);
                        return;
                    }
                }
            }

            //Q
            if (args.SData.Name.ToLower().Contains(Riven.Q.Name.ToLower()))
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    if (Player.Instance.HasBuff("RivenFengShuiEngine") && Riven.R2.IsReady() &&
                        Riven.ComboMenu["Combo.R2"].Cast<CheckBox>().CurrentValue)
                    {
                        var target2 = TargetSelector.GetTarget(Riven.R2.Range, DamageType.Physical);
                        if (target2 != null &&
                            (target2.Distance(Player.Instance) < 300 &&
                             target2.Health >
                             Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical, DamageLogic.QDamage()) ||
                             target2.Distance(Player.Instance) > 300) &&
                            Player.Instance.CalculateDamageOnUnit(target2, DamageType.Physical,
                                DamageLogic.RDamage(target2) + DamageLogic.QDamage()) > target2.Health)
                        {
                            Riven.R2.Cast(target2);
                        }
                    }
                }
                return;
            }

            if (args.SData.IsAutoAttack() && target != null)
            {
                if (Riven.ComboMenu["HumanizerDelay"].Cast<Slider>().CurrentValue == 0)
                {
                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                    {
                        StateLogic.ComboAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                    {
                        StateLogic.HarassAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) && target.IsJungleMinion())
                    {
                        StateLogic.JungleAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && target.IsLaneMinion())
                    {
                        StateLogic.LastHitAfterAa(target);
                    }

                    if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && target.IsLaneMinion())
                    {
                        StateLogic.LaneClearAfterAa(target);
                    }
                }
                else
                {
                    Core.DelayAction(() =>
                    {
                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                        {
                            StateLogic.ComboAfterAa(target);
                        }

                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                        {
                            StateLogic.HarassAfterAa(target);
                        }

                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear) &&
                            target.IsJungleMinion())
                        {
                            StateLogic.JungleAfterAa(target);
                        }

                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit) && target.IsLaneMinion())
                        {
                            StateLogic.LastHitAfterAa(target);
                        }

                        if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear) && target.IsLaneMinion())
                        {
                            StateLogic.LaneClearAfterAa(target);
                        }
                    }, Riven.ComboMenu["HumanizerDelay"].Cast<Slider>().CurrentValue);
                }
            }
        }
    }
}