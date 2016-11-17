using System.Reflection;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Drawing;

    namespace HTTF_Yasuo
    {
        class YasuoMenu
        {
            public static Menu Principal, Combo;

            public static void Load()
            {
                Principal = MainMenu.AddMenu("HTTF Yasuo ", "Yasuo");
                Principal.AddLabel("Yasuo Riven v" + Assembly.GetExecutingAssembly().GetName().Version);


                Combo = Principal.AddSubMenu("Combo", "Combo");
                Combo.AddSeparator(3);
                Combo.AddLabel("• Spells Combo");
                Combo.Add("UseQCombo", new CheckBox("Use Q"));
                Combo.Add("UseWCombo", new CheckBox("Use W"));
                Combo.Add("UseECombo", new CheckBox("Use E"));
                Combo.Add("UseRCombo", new CheckBox("Use R"));









            }



            public static bool CheckBox(Menu m, string s)
            {
                return m[s].Cast<CheckBox>().CurrentValue;
            }

            public static int Slider(Menu m, string s)
            {
                return m[s].Cast<Slider>().CurrentValue;
            }

            public static bool Keybind(Menu m, string s)
            {
                return m[s].Cast<KeyBind>().CurrentValue;
            }

            public static int ComboBox(Menu m, string s)
            {
                return m[s].Cast<ComboBox>().SelectedIndex;
            }
        }
    }
