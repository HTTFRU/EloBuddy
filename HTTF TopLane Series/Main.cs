using System.Reflection;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Drawing;
using EloBuddy.SDK.Events;
using EloBuddy;
using System;

namespace HTTF_TopLane_Series
{
    class Main
    {
        public static Menu ActivatorMenu;

        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static Spell.Targeted ignt = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);


        public static void Load()
        {
            ActivatorMenu = MainMenu.AddMenu("Activator HTTF", "HTTF Top");
            ActivatorMenu.AddLabel("HTTF Riven v" + Assembly.GetExecutingAssembly().GetName().Version);
            ActivatorMenu.AddLabel("Hey, this is series for top lane, soon mid lane jung and bot lane.");
            ActivatorMenu.AddLabel("Your feedback on the forum");
            ActivatorMenu.AddLabel("By IHTTFCreator o/");
            ActivatorMenu.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            ActivatorMenu.Add("skin.Id", new ComboBox("Skin Mode", 6, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" , "11", "12", "13", "14"));
            ActivatorMenu.Add("ignite", new CheckBox("Auto Ignite?", true));

            Game.OnTick += Game_OnTick;
            Game.OnUpdate += OnUpdate;
        }
        private static void OnUpdate(EventArgs args) { Ignite(); }

        private static void Ignite()
        {
            var target = TargetSelector.GetTarget(700, DamageType.True, Player.Instance.Position);

            float IgniteDMG = 50 + (20 * myhero.Level);

            if (target != null)
            {
                float HP5 = target.HPRegenRate * 5;

                if (check(ActivatorMenu, "ignite") && ignt.IsReady() && target.IsValidTarget(ignt.Range) &&
                    (IgniteDMG > (target.TotalShieldHealth() + HP5)))
                {
                    ignt.Cast(target);
                }
            }
        }
        private static bool check(Menu submenu, string sig)
        {
            return submenu[sig].Cast<CheckBox>().CurrentValue;
        }

        public static void Game_OnTick(EventArgs args)
        {
            if (CheckSkin())
            {
                Player.SetSkinId(SkinId());
            }
        }

        public static int SkinId()
        {
            return ActivatorMenu["skin.Id"].Cast<ComboBox>().CurrentValue;
        }

        public static bool CheckSkin()
        {
            return ActivatorMenu["checkSkin"].Cast<CheckBox>().CurrentValue;
        }


    }
}