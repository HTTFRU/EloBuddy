using System.Reflection;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Drawing;
using System;
using EloBuddy;

namespace _HTTF_Nidalee
{
    class NidaleeMenu
    {
        public static Menu menu, combo, harass, laneclear, jungleclear, misc, draw;

        public static void Load()
        {
            
            menu = MainMenu.AddMenu("HTTF Nidalee", "HTTF Nidalee");
            menu.AddSeparator();
            menu.AddLabel("Please Report Any Bugs in forum (topic)");

            combo = menu.AddSubMenu("Combo", "combo");
            combo.AddLabel("Spells");
            combo.AddLabel("Human Form");
            combo.Add("ComboQ1", new CheckBox("Use Q"));
            combo.Add("ComboW1", new CheckBox("Use W", false));
            combo.AddSeparator();
            combo.AddLabel("Cougar Form");
            combo.Add("ComboQ2", new CheckBox("Use Q"));
            combo.Add("ComboW2", new CheckBox("Use W"));
            combo.Add("ComboE2", new CheckBox("Use E"));
            combo.AddSeparator();
            combo.Add("ComboR", new CheckBox("Auto Switch Forms (R)"));

            laneclear = menu.AddSubMenu("Laneclear", "lclear");
            laneclear.AddGroupLabel("Spells");
            laneclear.AddLabel("Human Form");
            laneclear.Add("LaneCQ1", new CheckBox("Use Q", false));
            laneclear.AddSeparator();
            laneclear.AddLabel("Cougar Form");
            laneclear.Add("LaneCQ2", new CheckBox("Use Q", false));
            laneclear.Add("LQ2MIN", new Slider("Min Minions For Q", 1, 1, 4));
            laneclear.AddSeparator();
            laneclear.Add("LW2", new CheckBox("Use W", false));
            laneclear.Add("LaneCW2MIN", new Slider("Min Minions To Hit With W", 1, 1, 10));
            laneclear.AddSeparator();
            laneclear.Add("LaneCE2", new CheckBox("Use E", false));
            laneclear.AddSeparator();
            laneclear.AddLabel("R Usage");
            laneclear.Add("LaneR", new CheckBox("Auto Switch Forms (R)", false));
            laneclear.AddSeparator();
            laneclear.Add("LaneCMIN", new Slider("Min Mana % To Laneclear", 50, 0, 100));

            jungleclear = menu.AddSubMenu("Jungleclear", "jclear");
            jungleclear.AddGroupLabel("Spells");
            jungleclear.AddLabel("Human Form");
            jungleclear.Add("JungQ1", new CheckBox("Use Q", false));
            jungleclear.AddSeparator();
            jungleclear.AddLabel("Cougar Form");
            jungleclear.Add("JungQ2", new CheckBox("Use Q", false));
            jungleclear.Add("JungQ2MIN", new Slider("Min Monsters For Q", 1, 1, 4));
            jungleclear.AddSeparator();
            jungleclear.Add("JungW2", new CheckBox("Use W", false));
            jungleclear.Add("JungW2MIN", new Slider("Min Monsters To Hit With W", 1, 1, 4));
            jungleclear.AddSeparator();
            jungleclear.Add("JungE2", new CheckBox("Use E", false));
            jungleclear.AddSeparator();
            jungleclear.AddLabel("R Usage");
            jungleclear.Add("JungR", new CheckBox("Auto Switch Forms (R)", false));
            jungleclear.AddSeparator();
            jungleclear.Add("JungMIN", new Slider("Min Mana % To Jungleclear", 50, 0, 100));

            draw = menu.AddSubMenu("Drawings", "draw");
            draw.Add("nodraw", new CheckBox("Disable All Drawings", false));
            draw.AddSeparator();
            draw.Add("drawQ", new CheckBox("Draw Q Range"));
            draw.Add("drawW", new CheckBox("Draw W Range"));
            draw.Add("drawE", new CheckBox("Draw E Range"));
            draw.AddSeparator();
            draw.Add("drawonlyrdy", new CheckBox("Draw Only Ready Spells", false));


            misc = menu.AddSubMenu("Misc", "misc");
            jungleclear.AddLabel("Flee");
            misc.Add("W2FLEE", new CheckBox("Use Cougar W To Flee", false));
            misc.AddSeparator();
            misc.AddLabel("Killsteal");
            misc.Add("ksQ", new CheckBox("Killsteal with Q", false));
            misc.Add("autoign", new CheckBox("Auto Ignite If Killable"));
            misc.AddSeparator();
            jungleclear.AddLabel("Smite");
            misc.Add("SMITEKEY", new KeyBind("Auto-Smite Key", false, KeyBind.BindTypes.PressToggle, 'G'));
            misc.AddLabel("(Smite Will Target On Big Monsters Like Blue, Red, Dragon etc.)");


            misc.AddLabel("Prediction");
            misc.AddLabel("Q :");
            misc.Add("QPred", new Slider("Select % Hitchance", 90, 1, 100));
            misc.AddSeparator();
            misc.AddLabel("Human W :");
            misc.Add("W1Pred", new Slider("Select % Hitchance", 90, 1, 100));
            misc.AddSeparator();
            misc.AddLabel("Cougar W :");
            misc.Add("W2Pred", new Slider("Select % Hitchance", 90, 1, 100));
            misc.AddSeparator();
            misc.AddLabel("E :");
            misc.Add("EPred", new Slider("Select % Hitchance", 90, 1, 100));
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
    }

}

