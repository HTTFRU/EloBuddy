using System.Linq;
using EloBuddy;
using EloBuddy.SDK;
using HTTF_TopLane_Series;

namespace HTTF_TopLane_Series.Util
{
    class ActivatorItem
    {
        public static readonly Item Bilgewater = new Item((int)ItemId.Bilgewater_Cutlass, 550f);
        public static readonly Item BladeKing = new Item((int)ItemId.Blade_of_the_Ruined_King, 550f);
        public static readonly Item Youmuu = new Item((int)ItemId.Youmuus_Ghostblade);
        public static readonly Item Sheen = new Item((int)ItemId.Sheen);             
        public static readonly Item IceGauntlet = new Item((int)ItemId.Iceborn_Gauntlet);  
        public static readonly Item TriniForce = new Item((int)ItemId.Trinity_Force);         
        public static readonly Item Redemption = new Item((int)ItemId.Redemption);        
        static readonly Item Quicksilver = new Item((int)ItemId.Quicksilver_Sash);
        static readonly Item Mercurial = new Item((int)ItemId.Mercurial_Scimitar);

        public static void Items_Use()
        { 
            if (Player.Instance.IsDead) return; 
            if (Player.Instance.CountEnemiesInRange(1500) == 0) return;

            if (Main.Status_CheckBox(Main.Principial, "Item.BK") && (Bilgewater.IsOwned() || BladeKing.IsOwned()))
            {
                var Botrk_Target = TargetSelector.GetTarget(550, DamageType.Physical);

                if (Botrk_Target != null)
                { 
                    if (Bilgewater.IsReady() || (BladeKing.IsReady() && Player.Instance.HealthPercent <= Main.Status_Slider(Main.Principial, "Item.BK.Hp")))
                    {
                        Bilgewater.Cast(Botrk_Target);
                    }
                } 
            }

            if (Youmuu.IsOwned() && Youmuu.IsReady() && Player.Instance.CountEnemiesInRange(1000) >= 1)
            {
                Youmuu.Cast();
            }
            Active_Item();
        }   

        public static void Active_Redemption()
        {
            if (Redemption.IsOwned() && Redemption.IsReady() && Main.Status_CheckBox(Main.Principial, "Item.Redemption"))
            {
                var Redemption_Target = TargetSelector.GetTarget(5500, DamageType.Physical);

                if (Redemption_Target == null) return;

                var ipRedemption = Prediction.Position.PredictCircularMissile(Player.Instance, 5500, 275, 2500, 3200);

                if (Redemption_Target.IsAttackingPlayer)
                {
                    if (Player.Instance.HealthPercent <= Main.Status_Slider(Main.Principial, "Item.Redemption.MyHp"))
                    {
                        if (ipRedemption.HitChancePercent >= 50)
                        {
                            Redemption.Cast(ipRedemption.CastPosition);
                        }
                    }
                }

                foreach (var team in EntityManager.Heroes.Allies.Where(x => x.IsValidTarget(5500) && !x.IsMe))
                {
                    if (team.CountAlliesInRange(550) >= 2 && (Redemption_Target.HealthPercent <= 50 || team.HealthPercent <= 55))
                    {
                        if (ipRedemption.HitChancePercent >= 50)
                        {
                            Redemption.Cast(ipRedemption.CastPosition);
                        }
                    }
                }
            }
        }

        private static void Active_Item()
        {
            var Delay_Time = Main.Status_Slider(Main.Principial, "CastDelay");

            if (Quicksilver.IsOwned() && Quicksilver.IsReady() && Main.Status_CheckBox(Main.Principial, "QSS"))
            {
                if (Main.Status_CheckBox(Main.Principial, "Poisons") && Player.Instance.HasBuffOfType(BuffType.Poison))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Supression") && Player.Instance.HasBuffOfType(BuffType.Suppression))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Blind") && Player.Instance.HasBuffOfType(BuffType.Blind))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Charm") && Player.Instance.HasBuffOfType(BuffType.Charm))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Fear") && Player.Instance.HasBuffOfType(BuffType.Fear))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Polymorph") && Player.Instance.HasBuffOfType(BuffType.Polymorph))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Silence") && Player.Instance.HasBuffOfType(BuffType.Silence))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Slow") && Player.Instance.HasBuffOfType(BuffType.Slow))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Stun") && Player.Instance.HasBuffOfType(BuffType.Stun))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Snare") && Player.Instance.HasBuffOfType(BuffType.Snare))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Taunt") && Player.Instance.HasBuffOfType(BuffType.Taunt))
                { Core.DelayAction(() => Quicksilver.Cast(), Delay_Time); }
            }

            if (Mercurial.IsOwned() && Mercurial.IsReady() && Main.Status_CheckBox(Main.Principial, "Scimitar"))
            {
                if (Main.Status_CheckBox(Main.Principial, "Poisons") && Player.Instance.HasBuffOfType(BuffType.Poison))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Supression") && Player.Instance.HasBuffOfType(BuffType.Suppression))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Blind") && Player.Instance.HasBuffOfType(BuffType.Blind))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Charm") && Player.Instance.HasBuffOfType(BuffType.Charm))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Fear") && Player.Instance.HasBuffOfType(BuffType.Fear))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Polymorph") && Player.Instance.HasBuffOfType(BuffType.Polymorph))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Silence") && Player.Instance.HasBuffOfType(BuffType.Silence))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Slow") && Player.Instance.HasBuffOfType(BuffType.Slow))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Stun") && Player.Instance.HasBuffOfType(BuffType.Stun))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }
            
                if (Main.Status_CheckBox(Main.Principial, "Snare") && Player.Instance.HasBuffOfType(BuffType.Snare))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }

                if (Main.Status_CheckBox(Main.Principial, "Taunt") && Player.Instance.HasBuffOfType(BuffType.Taunt))
                { Core.DelayAction(() => Mercurial.Cast(), Delay_Time); }
            }
        }   //End Active_Item
    }
}