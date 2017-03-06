using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy;
using System;

namespace HTTF_TopLane_Series
{
    class Main
    {
        public static Menu Principial;

        public static AIHeroClient myhero { get { return ObjectManager.Player; } }
        public static Spell.Targeted ignt = new Spell.Targeted(myhero.GetSpellSlotFromName("summonerdot"), 600);


        public static void Load()
        {

            Principial = MainMenu.AddMenu("Activator HTTF", "Activator");

            Principial = MainMenu.AddMenu("Activator HTTF", "HTTF Top");
            Principial.AddLabel("Hey, this is series for top lane, soon mid lane jung and bot lane.");
            Principial.AddLabel("Your feedback on the forum");
            Principial.AddLabel("By IHTTFCreator o/");
            Principial.Add("checkSkin", new CheckBox("Use Skin Changer", false));
            Principial.Add("skin.Id", new ComboBox("Skin Mode", 6, "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" , "11", "12", "13", "14"));
            Principial.Add("ignite", new CheckBox("Auto Ignite?", true));
            Principial.AddLabel("ItemActivator");
            Principial.Add("Item.Youmuu", new CheckBox("Use Yommu?"));
            Principial.Add("Item.BK", new CheckBox("Bilgewater / Blade King"));
            Principial.Add("Item.BK.Hp", new Slider("[If player Hp is lower than  {0}% ]", 95, 0, 100));
            Principial.AddSeparator(10);
            Principial.Add("QSS", new CheckBox("Use QSS?")); 
            Principial.Add("Scimitar", new CheckBox("Use Scimitar?"));
            Principial.Add("CastDelay", new Slider("Cast Delay: {0}ms", 350, 0, 1200));
            Principial.AddSeparator(10);
            Principial.Add("Blind", new CheckBox("Use QSS"));
            Principial.Add("Charm", new CheckBox("Charm"));
            Principial.Add("Fear", new CheckBox("Fear"));
            Principial.Add("Ploymorph", new CheckBox("Ploymorph"));
            Principial.Add("Poisons", new CheckBox("Poisons"));
            Principial.Add("Silence", new CheckBox("Silence"));
            Principial.Add("Slow", new CheckBox("Slow"));
            Principial.Add("Stun", new CheckBox("Stun"));
            Principial.Add("Supression", new CheckBox("Supression"));
            Principial.Add("Taunt", new CheckBox("Taunt"));
            Principial.Add("Snare", new CheckBox("Snare"));

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

                if (check(Principial, "ignite") && ignt.IsReady() && target.IsValidTarget(ignt.Range) &&
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
            return Principial["skin.Id"].Cast<ComboBox>().CurrentValue;
        }

        public static bool CheckSkin()
        {
            return Principial["checkSkin"].Cast<CheckBox>().CurrentValue;
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