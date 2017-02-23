using System.Collections.Generic;
using System.Linq;

using SharpDX;


using Color = System.Drawing.Color;
using EloBuddy;

using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace HTTF_Zed
{
    class Zed
    {
        private static GameObject deathMark;

        private static int lastW;

        private static bool wCasted, rCasted;

        private static MissileClient wMissile;

        private static Obj_AI_Minion wShadow, rShadow;

        private static int wShadowT, rShadowT;
        



        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Targeted R;

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
            E = new Spell.Active(SpellSlot.E, 310);
            R = new Spell.Targeted(SpellSlot.R, 625);
            Q.MinimumHitChance = HitChance.High;

            Hydra = new Item((int)ItemId.Ravenous_Hydra, 300);
            Tiamat = new Item((int)ItemId.Tiamat, 300);
            Youmu = new Item((int)ItemId.Youmuus_Ghostblade, 0);
            Qss = new Item((int)ItemId.Quicksilver_Sash, 0);
            Mercurial = new Item((int)ItemId.Mercurial_Scimitar, 0);

            Evade.Evading += Evading;
            Evade.TryEvading += TryEvading;
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += OnDraw;
            Obj_AI_Base.OnProcessSpellCast += (sender, args) =>
            {
                if (!sender.IsMe)
                {
                    return;
                }
                if (args.Slot == SpellSlot.W && args.SData.Name == "ZedW")
                {
                    rCasted = false;
                }
                else if (args.Slot == SpellSlot.R && args.SData.Name == "ZedR")
                {
                    wCasted = false;
                    rCasted = true;
                }
            };
            GameObject.OnCreate += (sender, args) =>
            {
                if (sender.IsEnemy)
                {
                    return;
                }
                var shadow = sender as Obj_AI_Minion;
                if (shadow == null || !shadow.IsAlly || shadow.CharData.BaseSkinName != "ZedUltMissile" || shadow.CharData.BaseSkinName != "ZedShadowDashMissile" || shadow.CharData.BaseSkinName != "zedshadow")
                {
                    return;
                }
                if (wCasted)   
                {
                    wShadowT = Core.GameTickCount;
                    wShadow = shadow;
                    wCasted = rCasted = false;
                }
                else if (rCasted)
                {
                    rShadowT = Core.GameTickCount;
                    rShadow = shadow;
                    wCasted = rCasted = false;
                }
            };
            Obj_AI_Base.OnBuffGain += (sender, args) =>
            {
                if (sender.IsEnemy || !args.Buff.Caster.IsMe)
                {
                    return;
                }
                var shadow = sender as Obj_AI_Base;
                if (shadow != null && shadow.IsAlly && shadow.BaseSkinName == "ZedShadow" && args.Buff.Caster.IsMe)
                {
                    switch (args.Buff.Name)
                    {
                        case "zedwshadowbuff":
                            if (!wShadow.com(shadow))
                            {
                                wShadowT = Core.GameTickCount;
                                wShadow = shadow;
                            }
                            break;
                        case "zedrshadowbuff":
                            if (!rShadow.Compare(shadow))
                            {
                                rShadowT = Core.GameTickCount;
                                rShadow = shadow;
                            }
                            break;
                    }
                }
            };
            Obj_AI_Base.OnPlayAnimation += (sender, args) =>
            {
                if (sender.IsMe || sender.IsEnemy || args.Animation != "Death")
                {
                    return;
                }
                if (sender.Compare(wShadow))
                {
                    wShadow = null;
                }
                else if (sender.Compare(rShadow))
                {
                    rShadow = null;
                }
            };
            GameObject.OnCreate += (sender, args) =>
            {
                var missile = sender as MissileClient;
                if (missile != null)
                {
                    if (missile.SpellCaster.IsMe && missile.SData.Name == "ZedWMissile")
                    {
                        wMissile = missile;
                    }
                    return;
                }
                if (sender.Name != "Zed_Base_R_buf_tell.troy")
                {
                    return;
                }
                var target = EntityManager.Heroes.Enemies.FirstOrDefault(i => i.IsValidTarget() && HaveR(i));
                if (target != null && target.Distance(sender) < 150)
                {
                    deathMark = sender;
                }
            };
            GameObject.OnCreate += (sender, args) =>
            {
                if (sender.Compare(wMissile))
                {
                    wMissile = null;
                }
                else if (sender.Compare(deathMark))
                {
                    deathMark = null;
                }
            };

        }

    }




}

    }
