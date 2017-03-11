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

namespace _HTTF_Nidalee
{
    class Logic
    {
        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static bool Q1Ready = true, Q2Ready = true, W1Ready = true, W2Ready = true, E2Ready = true;
        private static Spell.Targeted ignt { get; set; }
        private static Spell.Targeted smite { get; set; }
        public static void Load()
        {          

            Drawing.OnDraw += OnDraw;
            
            Obj_AI_Base.OnProcessSpellCast += OnProcess;
            Game.OnTick += OnTick;


            SpellData.Q1.AllowedCollisionCount = 0;

            if (EloBuddy.SDK.Spells.SummonerSpells.PlayerHas(EloBuddy.SDK.Spells.SummonerSpellsEnum.Ignite))
            {
                ignt = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);
            }
            else if (EloBuddy.SDK.Spells.SummonerSpells.PlayerHas(EloBuddy.SDK.Spells.SummonerSpellsEnum.Smite))
            {
                smite = new Spell.Targeted(myhero.GetSpellSlotFromName("summonersmite"), 500);
            }
        }

        private static void OnTick(EventArgs args)
        {
            if (myhero.IsDead) return;

            var flags = Orbwalker.ActiveModesFlags;

            if (flags.HasFlag(Orbwalker.ActiveModes.Combo)) Combo();


            if (flags.HasFlag(Orbwalker.ActiveModes.LaneClear) && myhero.ManaPercent > slider(NidaleeMenu.laneclear, "LaneCMIN")) Laneclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.JungleClear) && myhero.ManaPercent > slider(NidaleeMenu.jungleclear, "JungMIN")) Jungleclear();

            if (flags.HasFlag(Orbwalker.ActiveModes.Flee) && check(NidaleeMenu.misc, "W2FLEE") && myhero.Spellbook.GetSpell(SpellSlot.W).IsLearned)
            {
                if (IsCougar())
                {
                    SpellData.W2.Cast(myhero.Position.Extend(Game.CursorPos, SpellData.W2.Range - 1.0f).To3D());
                }
                else
                {
                    if (W2Ready && SpellData.R.Cast())
                    {
                        SpellData.W2.Cast(myhero.Position.Extend(Game.CursorPos, SpellData.W2.Range - 1.0f).To3D());
                        return;
                    }
                }
            }

            Misc();
        }

        private static bool check(Menu submenu, string sig)
        {
            return submenu[sig].Cast<CheckBox>().CurrentValue;
        }

        private static int slider(Menu submenu, string sig)
        {
            return submenu[sig].Cast<Slider>().CurrentValue;
        }

        private static int comb(Menu submenu, string sig)
        {
            return submenu[sig].Cast<ComboBox>().CurrentValue;
        }

        private static bool key(Menu submenu, string sig)
        {
            return submenu[sig].Cast<KeyBind>().CurrentValue;
        }


        private static float QDamage(AIHeroClient target)
        {
            int index = myhero.Spellbook.GetSpell(SpellSlot.Q).Level - 1;
            var dist = target.Distance(myhero.Position);
            var BaseDamage = new[] { 60, 77, 95, 112, 130 }[index];

            float QDamage = 0;

            if (dist <= 525)
            {
                QDamage = BaseDamage + (0.4f * myhero.FlatMagicDamageMod);
            }
            else if (dist > 525 && dist < 1300)
            {
                QDamage = ((((dist - 525) / 3.875f) / 100) * BaseDamage) + (0.4f * myhero.FlatMagicDamageMod);
            }
            else if (dist >= 1300)
            {
                QDamage = new[] { 210, 255, 300, 345, 390 }[index] + (1.2f * myhero.FlatMagicDamageMod);
            }

            return myhero.CalculateDamageOnUnit(target, DamageType.Magical, QDamage);
        }

        private static bool IsCougar()
        {
            return myhero.GetAutoAttackRange() < 300;
        }

        private static bool IsHunted(AIHeroClient target)
        {
            return target.HasBuff("NidaleePassiveHunted");
        }

        private static void RLogic(AIHeroClient target)
        {
            if (IsCougar())
            {
                if (Q1Ready && (SpellData.Q2.IsOnCooldown || myhero.CountEnemiesInRange(300) == 0) &&
                    (SpellData.W2.IsOnCooldown || myhero.CountEnemiesInRange(SpellData.W2.Range) == 0) &&
                    (SpellData.E2.IsOnCooldown || myhero.CountEnemiesInRange(SpellData.E2.Range + 100) == 0) && !myhero.IsFleeing)
                {
                    SpellData.R.Cast();
                }
            }
            else if (!IsCougar())
            {
                if (SpellData.Q1.IsOnCooldown && ((W2Ready && (IsHunted(target) ? myhero.CountEnemiesInRange(SpellData.W2E.Range) : myhero.CountEnemiesInRange(SpellData.W2.Range)) >= 1) ||
                                                  (Q2Ready && myhero.CountEnemiesInRange(SpellData.Q2.Range) >= 1) ||
                                                  (E2Ready && myhero.CountEnemiesInRange(SpellData.E2.Range) >= 1)))
                {
                    SpellData.R.Cast();
                }
            }
            return;
        }

        private static void CastW2(AIHeroClient target)
        {
            if (SpellData.W2.IsReady())
            {
                switch (IsHunted(target))
                {
                    case true:
                        if (target.IsValidTarget(SpellData.W2E.Range))
                        {
                            var wpred = SpellData.W2E.GetPrediction(target);

                            if (wpred.HitChancePercent >= slider(NidaleeMenu.misc, "W2Pred")) SpellData.W2E.Cast(wpred.CastPosition);
                        }
                        break;
                    case false:
                        if (target.IsValidTarget(SpellData.W2.Range))
                        {
                            var wpred = SpellData.W2.GetPrediction(target);

                            if (wpred.HitChancePercent >= slider(NidaleeMenu.misc, "W2Pred")) SpellData.W2.Cast(wpred.CastPosition);
                        }
                        break;
                }
            }
            return;
        }

        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1600, DamageType.Physical, Player.Instance.Position);

            if (target != null)
            {
                if (IsCougar())
                {
                    if (check(NidaleeMenu.combo, "ComboQ2") && SpellData.Q2.CanCast(target) && SpellData.Q2.Cast())
                    {
                        return;
                    }

                    if (check(NidaleeMenu.combo, "ComboW2") && SpellData.W2.CanCast(target))
                    {
                        CastW2(target);
                    }

                    if (check(NidaleeMenu.combo, "CobmoE2") && SpellData.E2.CanCast(target))
                    {
                        if (Prediction.Position.PredictUnitPosition(target, SpellData.E2.CastDelay).Distance(myhero.Position) <= SpellData.E2.Range)
                        {
                            SpellData.E2.Cast(target.Position);
                            return;
                        }
                    }
                }
                else if (!IsCougar())
                {
                    if (check(NidaleeMenu.combo, "ComboQ1") && SpellData.Q1.CanCast(target))
                    {
                        var qpred = SpellData.Q1.GetPrediction(target);

                        if (qpred.HitChancePercent >= slider(NidaleeMenu.misc, "QPred")) SpellData.Q1.Cast(qpred.CastPosition); return;
                    }

                    if (check(NidaleeMenu.combo, "ComboW1") && SpellData.W1.CanCast(target))
                    {
                        var wpred = SpellData.W1.GetPrediction(target);

                        if (wpred.HitChancePercent >= slider(NidaleeMenu.misc, "W1Pred")) SpellData.W1.Cast(wpred.CastPosition); return;
                    }
                }

                if (check(NidaleeMenu.combo, "ComboR") && SpellData.R.IsReady())
                {
                    RLogic(target);
                }
            }
        }

        

        private static void Laneclear()
        {
            var minions = EntityManager.MinionsAndMonsters.GetLaneMinions(EntityManager.UnitTeam.Enemy, myhero.Position, SpellData.Q1.Range).ToList();

            if (minions != null)
            {
                if (!IsCougar())
                {
                    if (check(NidaleeMenu.laneclear, "LaneCQ1") && SpellData.Q1.IsReady())
                    {
                        foreach (var minion in minions.Where(x => x.IsValidTarget(SpellData.Q1.Range) && x.Health > 40).OrderBy(x => x.Distance(myhero.Position)))
                        {
                            var qpred = SpellData.Q1.GetPrediction(minion);

                           
                            if (qpred.HitChancePercent >= slider(NidaleeMenu.misc, "QPred")) SpellData.Q1.Cast(qpred.CastPosition);
                        }
                    }
                }
                else if (IsCougar())
                {
                    if (check(NidaleeMenu.laneclear, "LaneCQ2") && SpellData.Q2.IsReady())
                    {
                        if (minions.Where(x => x.IsValidTarget(SpellData.Q2.Range) && x.Health > 30).Count() >= slider(NidaleeMenu.laneclear, "LQ2MIN")) SpellData.Q2.Cast();
                    }

                    if (check(NidaleeMenu.laneclear, "LaneCW2") && SpellData.W2.IsReady())
                    {
                        SpellData.W2.CastOnBestFarmPosition(slider(NidaleeMenu.laneclear, "LaneCW2MIN"));
                    }

                    if (check(NidaleeMenu.laneclear, "LaneCE2") && SpellData.E2.IsReady())
                    {
                        foreach (var minion in minions.Where(x => x.IsValidTarget(SpellData.E2.Range) && x.Health > 40))
                        {
                          
                            var epred = SpellData.E2.GetPrediction(minion);

                            if (epred.HitChancePercent >= slider(NidaleeMenu.misc, "EPred")) SpellData.E2.Cast(epred.CastPosition);
                        }
                    }
                }

                if (check(NidaleeMenu.laneclear, "LaneR") && SpellData.R.IsReady())
                {
                    if (IsCougar())
                    {
                        if (Q1Ready && (SpellData.Q2.IsOnCooldown || minions.Where(x => x.IsValidTarget(SpellData.Q2.Range)).Count() == 0) &&
                            (SpellData.W2.IsOnCooldown || minions.Where(x => x.IsValidTarget(SpellData.W2.Range)).Count() == 0) &&
                            (SpellData.E2.IsOnCooldown || minions.Where(x => x.IsValidTarget(SpellData.E2.Range)).Count() == 0) && !myhero.IsFleeing)
                        {
                            SpellData.R.Cast();
                        }
                    }
                    if (!IsCougar())
                    {
                        if (SpellData.Q1.IsOnCooldown && ((W2Ready && minions.Where(x => x.IsValidTarget(SpellData.W2.Range)).Count() >= 1) ||
                                         (Q2Ready && minions.Where(x => x.IsValidTarget(SpellData.Q2.Range)).Count() >= 1) ||
                                         (E2Ready && minions.Where(x => x.IsValidTarget(SpellData.E2.Range)).Count() >= 1)))
                        {
                            SpellData.R.Cast();
                        }
                    }
                }
            }
        }

        private static void Jungleclear()
        {
            var Monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(myhero.Position, 1600).Where(x => !x.IsDead && x.IsValid);

            if (Monsters != null)
            {
                if (!IsCougar())
                {
                    if (check(NidaleeMenu.jungleclear, "JungQ1") && SpellData.Q1.IsReady())
                    {
                        foreach (var monster in Monsters.Where(x => x.IsValidTarget(SpellData.Q1.Range) && x.Health > 40).OrderBy(x => x.Distance(myhero.Position)))
                        {
                            var qpred = SpellData.Q1.GetPrediction(monster);

                            
                            if (qpred.HitChancePercent >= slider(NidaleeMenu.misc, "QPred")) SpellData.Q1.Cast(qpred.CastPosition);
                        }
                    }
                }
                else if (IsCougar())
                {
                    if (check(NidaleeMenu.jungleclear, "JungQ2") && SpellData.Q2.IsReady())
                    {
                        if (Monsters.Where(x => x.IsValidTarget(SpellData.Q2.Range) && x.Health > 30).Count() >= slider(NidaleeMenu.jungleclear, "JQ2MIN")) SpellData.Q2.Cast();
                    }

                    if (check(NidaleeMenu.jungleclear, "JungW2") && SpellData.W2.IsReady())
                    {
                        var pred = EntityManager.MinionsAndMonsters.GetCircularFarmLocation(Monsters, 75, 375);

                        if (pred.HitNumber >= slider(NidaleeMenu.jungleclear, "JungW2MIN")) SpellData.W2.Cast(pred.CastPosition);
                    }

                    if (check(NidaleeMenu.jungleclear, "JungE2") && SpellData.E2.IsReady())
                    {
                        foreach (var monster in Monsters.Where(x => x.IsValidTarget(SpellData.E2.Range) && x.Health > 40))
                        {
                            
                            var epred = SpellData.E2.GetPrediction(monster);

                            if (epred.HitChancePercent >= slider(NidaleeMenu.misc, "EPred")) SpellData.E2.Cast(epred.CastPosition);
                        }
                    }
                }

                if (check(NidaleeMenu.jungleclear, "JungR") && SpellData.R.IsReady())
                {
                    if (IsCougar())
                    {
                        if (Q1Ready && (SpellData.Q2.IsOnCooldown || Monsters.Where(x => x.IsValidTarget(SpellData.Q2.Range)).Count() == 0) &&
                            (SpellData.W2.IsOnCooldown || Monsters.Where(x => x.IsValidTarget(SpellData.W2.Range)).Count() == 0) &&
                            (SpellData.E2.IsOnCooldown || Monsters.Where(x => x.IsValidTarget(SpellData.E2.Range)).Count() == 0) && !myhero.IsFleeing)
                        {
                            SpellData.R.Cast();
                        }
                    }
                    if (!IsCougar())
                    {
                        if (SpellData.Q1.IsOnCooldown && ((W2Ready && Monsters.Where(x => x.IsValidTarget(SpellData.W2.Range)).Count() >= 1) ||
                                         (Q2Ready && Monsters.Where(x => x.IsValidTarget(SpellData.Q2.Range)).Count() >= 1) ||
                                         (E2Ready && Monsters.Where(x => x.IsValidTarget(SpellData.E2.Range)).Count() >= 1)))
                        {
                            SpellData.R.Cast();
                        }
                    }
                }
            }
        }

        private static void Misc()
        {
            var target = TargetSelector.GetTarget(1600, DamageType.Magical, Player.Instance.Position);

            if (target != null && target.IsValidTarget() && !target.IsInvulnerable)
            {
                if (check(NidaleeMenu.misc, "ksQ") && !IsCougar() && SpellData.Q1.IsReady() && target.IsValidTarget(SpellData.Q1.Range) && QDamage(target) > target.Health)
                {
                    var qpred = SpellData.Q1.GetPrediction(target);

                    if (qpred.HitChancePercent >= slider(NidaleeMenu.misc, "QPred")) SpellData.Q1.Cast(qpred.CastPosition);
                }

                if (ignt != null && check(NidaleeMenu.misc, "autoign") && ignt.IsReady() && target.IsValidTarget(ignt.Range) &&
                    myhero.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health)
                {
                    ignt.Cast(target);
                }
            }

            if (key(NidaleeMenu.misc, "EKEY") && !IsCougar() && SpellData.E1.IsReady() && myhero.ManaPercent >= slider(NidaleeMenu.misc, "EMINM") && !myhero.IsRecalling())
            {
                var ClosestAlly = EntityManager.Heroes.Allies.Where(x => !x.IsDead && x.IsValidTarget(SpellData.E1.Range) && x.HealthPercent <= slider(NidaleeMenu.misc, "EMINA"))
                                                                     .OrderBy(x => x.Health)
                                                                     .FirstOrDefault();
                switch (comb(NidaleeMenu.misc, "EMODE"))
                {
                    case 0:
                        if (myhero.HealthPercent <= slider(NidaleeMenu.misc, "EMINH"))
                        {
                            SpellData.E1.Cast(myhero);
                        }
                        break;
                    case 1:
                        if (ClosestAlly != null) SpellData.E1.Cast(ClosestAlly.Position);
                        break;
                    case 2:
                        if (ClosestAlly != null)
                        {
                            switch (myhero.Health > ClosestAlly.Health)
                            {
                                case true:
                                    SpellData.E1.Cast(ClosestAlly.Position);
                                    break;
                                case false:
                                    if (myhero.HealthPercent <= slider(NidaleeMenu.misc, "EMINH") && myhero.CountEnemiesInRange(1000) >= slider(NidaleeMenu.misc, "EMINE"))
                                    {
                                        SpellData.E1.Cast(myhero.Position);
                                    }
                                    break;
                            }
                        }
                        else goto case 0;
                        break;
                }
            }

            

            if (smite != null && smite.IsReady() && key(NidaleeMenu.misc, "SMITEKEY"))
            {
                var Monsters = EntityManager.MinionsAndMonsters.GetJungleMonsters(myhero.Position, 500).Where(x => x.IsValidTarget(500) && !x.IsDead &&
                                                                                                                   !x.Name.ToLower().Contains("mini"));

                if (Monsters != null)
                {
                    foreach (var monster in Monsters)
                    {
                        var SmiteDamage = myhero.GetSummonerSpellDamage(monster, DamageLibrary.SummonerSpells.Smite);

                        if (smite.CanCast(monster) && monster.Health < SmiteDamage && smite.Cast(monster)) return;
                    }
                }
            }

        }

        private static void OnDraw(EventArgs args)
        {
            if (myhero.IsDead || check(NidaleeMenu.draw, "nodraw")) return;

            if (check(NidaleeMenu.draw, "drawQ") && myhero.Spellbook.GetSpell(SpellSlot.Q).Level > 0 && !myhero.IsDead && !check(NidaleeMenu.draw, "nodraw"))
            {
                if (check(NidaleeMenu.draw, "drawonlyrdy"))
                {
                    Circle.Draw(
                        (IsCougar() ? SpellData.Q2.IsOnCooldown : SpellData.Q1.IsOnCooldown) ? SharpDX.Color.Transparent : SharpDX.Color.Green,
                        IsCougar() ? SpellData.Q2.Range : SpellData.Q1.Range,
                        myhero.Position);
                }

                else if (!check(NidaleeMenu.draw, "drawonlyrdy"))
                {
                    Circle.Draw(
                        SharpDX.Color.Green,
                        IsCougar() ? SpellData.Q2.Range : SpellData.Q1.Range,
                        myhero.Position);
                }
            }

            if (check(NidaleeMenu.draw, "drawW") && myhero.Spellbook.GetSpell(SpellSlot.W).Level > 0 && !myhero.IsDead && !check(NidaleeMenu.draw, "nodraw"))
            {
                if (check(NidaleeMenu.draw, "drawonlyrdy"))
                {
                    Circle.Draw(
                        (IsCougar() ? SpellData.W2.IsOnCooldown : SpellData.W1.IsOnCooldown) ? SharpDX.Color.Transparent : SharpDX.Color.Green,
                        IsCougar() ? SpellData.W2.Range : SpellData.W1.Range,
                        myhero.Position);
                }
                else if (!check(NidaleeMenu.draw, "drawonlyrdy"))
                {
                    Circle.Draw(
                        SharpDX.Color.Blue,
                        IsCougar() ? SpellData.W2.Range : SpellData.W1.Range,
                        myhero.Position);
                }
            }

            if (check(NidaleeMenu.draw, "drawE") && myhero.Spellbook.GetSpell(SpellSlot.E).Level > 0 && !myhero.IsDead && !check(NidaleeMenu.draw, "nodraw"))
            {
                if (check(NidaleeMenu.draw, "drawonlyrdy"))
                {
                    Circle.Draw(
                        (IsCougar() ? SpellData.E2.IsOnCooldown : SpellData.E1.IsOnCooldown) ? SharpDX.Color.Transparent : SharpDX.Color.Green,
                        IsCougar() ? SpellData.E2.Range : SpellData.E1.Range,
                        myhero.Position);
                }

                else if (!check(NidaleeMenu.draw, "drawonlyrdy"))
                {
                    Circle.Draw(
                        SharpDX.Color.Blue,
                        IsCougar() ? SpellData.E2.Range : SpellData.E1.Range,
                        myhero.Position);
                }
            }

   


        }

        private static void OnProcess(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                switch (IsCougar())
                {
                    case false:
                        switch (args.Slot)
                        {
                            case SpellSlot.Q:
                                Q1Ready = false;
                                Core.DelayAction(() => { Q1Ready = true; }, (int)(myhero.Spellbook.GetSpell(SpellSlot.Q).Cooldown) * 1000);
                                break;
                            case SpellSlot.W:
                                W1Ready = false;
                                Core.DelayAction(() => { W1Ready = true; }, (int)(myhero.Spellbook.GetSpell(SpellSlot.W).Cooldown) * 1000);
                                break;
                        }
                        break;
                    case true:
                        switch (args.Slot)
                        {
                            case SpellSlot.Q:
                                Q2Ready = false;
                                Core.DelayAction(() => { Q2Ready = true; }, (int)(myhero.Spellbook.GetSpell(SpellSlot.Q).Cooldown) * 1000);
                                Orbwalker.ResetAutoAttack();
                                break;
                            case SpellSlot.W:
                                W2Ready = false;
                                Core.DelayAction(() => { W2Ready = true; }, (int)(myhero.Spellbook.GetSpell(SpellSlot.W).Cooldown) * 1000);
                                break;
                            case SpellSlot.E:
                                E2Ready = false;
                                Core.DelayAction(() => { E2Ready = true; }, (int)(myhero.Spellbook.GetSpell(SpellSlot.E).Cooldown) * 1000);
                                break;
                        }
                        break;
                }
            }
        }

  
}
    }

