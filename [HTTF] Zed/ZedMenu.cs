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

    class ZedMenu
    {
        public static Menu Principal, comboMenu, hybridMenu, lhMenu, ksMenu, drawMenu, miscMenu;



        public static void Load()
        {
            comboMenu = Principal.AddSubMenu("Combo", "Combo");
            comboMenu.AddGroupLabel("Q/E: Always On");
            comboMenu.Add("Ignite", new CheckBox("Use Ignite"));
            comboMenu.Add("Items", new CheckBox("Use Items"));
            comboMenu.AddGroupLabel("Swap Settings");
            comboMenu.Add("SwapIfKill", new CheckBox("Swap W/R If Mark Can Kill Target", false));
            comboMenu.Add("SwapIfHpU", new Slider("Swap W/R If Hp < (%)", 10));
            comboMenu.Add("SwapGap", new ComboBox("Swap W/R To Gap Close", 1, "OFF", "Smart", "Always"));
            comboMenu.AddGroupLabel("W Settings");
            comboMenu.Add("WNormal", new CheckBox("Use For Non-R Combo"));
            comboMenu.Add("WAdv", new ComboBox("Use For R Combo", 1, "OFF", "Line", "Triangle", "Mouse"));
            comboMenu.AddGroupLabel("R Settings");
            comboMenu.Add("R", new KeyBind("Use R", false, KeyBind.BindTypes.PressToggle, 'X'));
            comboMenu.Add("RMode", new ComboBox("Mode", 0, "Always", "Wait Q/E"));


            comboMenu.AddGroupLabel("Extra R Settings");
            foreach (var enemy in EntityManager.Heroes.Enemies)
            {
                comboMenu.Add("RCast" + enemy.ChampionName, new CheckBox("Cast On " + enemy.ChampionName, false));
            }


            hybridMenu = Principal.AddSubMenu("Hybrid", "Hybrid");
            hybridMenu.Add("Mode", new ComboBox("Mode", 1, "W-E-Q", "E-Q", "Q"));
            hybridMenu.Add("WEQ", new KeyBind("Only W If Hit E-Q", false, KeyBind.BindTypes.PressToggle, 'X'));
            hybridMenu.AddGroupLabel("Auto Q Settings (Champ)");
            hybridMenu.Add("AutoQ", new KeyBind("KeyBind", false, KeyBind.BindTypes.PressToggle, 'T'));
            hybridMenu.Add("AutoQMpA", new Slider("If Mp >=", 100, 0, 200));
            hybridMenu.AddGroupLabel("Auto E Settings (Champ/Shadow)");
            hybridMenu.Add("AutoE", new CheckBox("Auto", false));

            lhMenu = Principal.AddSubMenu("LastHit", "Last Hit");
            lhMenu.Add("Q", new CheckBox("Use Q"));

            ksMenu = Principal.AddSubMenu("KillSteal", "Kill Steal");
            ksMenu.Add("Q", new CheckBox("Use Q"));
            ksMenu.Add("E", new CheckBox("Use E"));



            drawMenu = Principal.AddSubMenu("Draw", "Draw");
            drawMenu.Add("Q", new CheckBox("Q Range", false));
            drawMenu.Add("W", new CheckBox("W Range", false));
            drawMenu.Add("E", new CheckBox("E Range", false));
            drawMenu.Add("R", new CheckBox("R Range", false));
            drawMenu.Add("RStop", new CheckBox("Prevent Q/W/E Range", false));
            drawMenu.Add("UseR", new CheckBox("R In Combo Status"));
            drawMenu.Add("Target", new CheckBox("Target"));
            drawMenu.Add("DMark", new CheckBox("Death Mark"));
            drawMenu.Add("WPos", new CheckBox("W Shadow"));
            drawMenu.Add("RPos", new CheckBox("R Shadow"));

            miscMenu = Principal.AddSubMenu("Misc", "Misc");
            miscMenu.Add("FleeW", new KeyBind("Use W To Flee", false, KeyBind.BindTypes.HoldActive, 'C'));
        }


        public static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        public static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        public static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }

        public static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }
    }
}
