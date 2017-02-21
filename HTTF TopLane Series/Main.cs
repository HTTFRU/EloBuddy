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
        public static Menu Principial, ActivatorMenu, M_NVer;

        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static Spell.Targeted ignt = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);


        public static void Load()
        {
            Chat.Print("<font color = '#20b2aa'>Welcome to </font><font color = '#ffffff'>[ HTTF ] " + "Riven" + "</font><font color = '#20b2aa'>. Addon is ready.</font>");
            CheckVersion.CheckUpdate();
            Principial = MainMenu.AddMenu("HTTF Riven v2", "Riven");

            ActivatorMenu = MainMenu.AddMenu("Activator HTTF", "HTTF Top");
            ActivatorMenu.AddLabel("Hey, this is series for top lane, soon mid lane jung and bot lane.");
            ActivatorMenu.AddLabel("Your feedback on the forum");
            ActivatorMenu.AddLabel("By IHTTFCreator o/");
            ActivatorMenu.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            ActivatorMenu.Add("skin.Id", new ComboBox("Skin Mode", 6, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" , "11", "12", "13", "14"));
            ActivatorMenu.Add("ignite", new CheckBox("Auto Ignite?", true));
            ActivatorMenu.AddLabel("ItemActivator");
            ActivatorMenu.Add("Item.Youmuu", new CheckBox("Use Yommu?"));
            ActivatorMenu.Add("Item.BK", new CheckBox("Bilgewater / Blade King"));
            ActivatorMenu.Add("Item.BK.Hp", new Slider("[If player Hp is lower than  {0}% ]", 95, 0, 100));
            ActivatorMenu.AddSeparator(10);
            ActivatorMenu.Add("QSS", new CheckBox("Use QSS?")); 
            ActivatorMenu.Add("Scimitar", new CheckBox("Use Scimitar?"));
            ActivatorMenu.Add("CastDelay", new Slider("Cast Delay: {0}ms", 350, 0, 1200));
            ActivatorMenu.AddSeparator(10);
            ActivatorMenu.Add("Blind", new CheckBox("Use QSS"));
            ActivatorMenu.Add("Charm", new CheckBox("Charm"));
            ActivatorMenu.Add("Fear", new CheckBox("Fear"));
            ActivatorMenu.Add("Ploymorph", new CheckBox("Ploymorph"));
            ActivatorMenu.Add("Poisons", new CheckBox("Poisons"));
            ActivatorMenu.Add("Silence", new CheckBox("Silence"));
            ActivatorMenu.Add("Slow", new CheckBox("Slow"));
            ActivatorMenu.Add("Stun", new CheckBox("Stun"));
            ActivatorMenu.Add("Supression", new CheckBox("Supression"));
            ActivatorMenu.Add("Taunt", new CheckBox("Taunt"));
            ActivatorMenu.Add("Snare", new CheckBox("Snare"));

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

        public static bool Status_CheckBox(Menu sub, string str)
        {
            return sub[str].Cast<CheckBox>().CurrentValue;
        }

        public static int Status_Slider(Menu sub, string str)
        {
            return sub[str].Cast<Slider>().CurrentValue;
        }

    }
}