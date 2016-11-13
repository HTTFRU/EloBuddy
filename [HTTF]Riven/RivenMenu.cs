using System.Reflection;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
namespace HTTF_Riven_v2
{
    class RivenMenu
    {
        public static Menu Principal, Combo, Burst, Shield, Items, Laneclear, Jungleclear, Flee, Misc, Draw, Killsteal, AnimationCancle;

        public static void Load()
        {
            Principal = MainMenu.AddMenu("HTTF Riven v2", "Riven");
            Principal.AddLabel("HTTF Riven v" + Assembly.GetExecutingAssembly().GetName().Version);


            Combo = Principal.AddSubMenu("Combo", "Combo");
            Combo.AddSeparator(3);
            Combo.AddLabel("• Spells Combo");
            Combo.Add("UseQCombo", new CheckBox("Use Q?"));
            Combo.Add("UseWCombo", new CheckBox("Use W?"));
            Combo.Add("UseECombo", new CheckBox("Use E"));
            Combo.Add("UseRCombo", new CheckBox("Use R?"));
            Combo.Add("UseR2Combo", new CheckBox("Use R2?"));
            Combo.Add("BrokenAnimations", new CheckBox("Broken Animations ?"));
            Combo.AddSeparator(3);
            Combo.AddLabel("• Spell R");
            Combo.Add("UseRType", new ComboBox("Use R when", 1, "Target less than 40 % HP", "DamageIndicator greater than 100 %", "Always", "On Keypress"));
            Combo.Add("ForceR", new KeyBind("R On Keypress Key", false, KeyBind.BindTypes.PressToggle, 'U'));
            Combo.Add("DontR1", new Slider("Dont R if Target HP {0}% <=", 25, 10, 50));
            Combo.AddSeparator(3);
            Combo.AddLabel("• Spell R2");
            Combo.Add("UseR2Type", new ComboBox("Use R2 when", 0, "Kill only", "Max damage when target less than 25 %"));
            Combo.AddLabel("• Burst");
            Combo.AddLabel("The combo burst key is the Combo !");
            Combo.AddLabel("This 'Burst allowed' option is just to confirm that you want to use the Burst");
            Combo.AddSeparator(2);
            Combo.Add("BurstAllowed", new KeyBind("Burst Allowed ?", false, KeyBind.BindTypes.PressToggle, 'T'));
            Combo.Add("BurstType", new ComboBox("Burst:", 0, "Damage Check", "Always"));
            Combo.AddSeparator(2);
            Combo.AddLabel("Select Burst style");
            Combo.AddLabel("BURST STYLE №1 BROKEN. USE ONLY 2");
            Combo.AddLabel("Style Burst 1: E > Flash > R > W > Hydra > R2");
            Combo.AddLabel("Style Burst 2: E > R > Flash > W > Hydra > R2");
            Combo.AddSeparator(1);
            Combo.Add("BurstStyle", new Slider("Burst style", 2, 1, 2));
            Combo.AddLabel(" FLEE");
            Combo.Add("UseQFlee", new CheckBox("Use Q"));
            Combo.Add("UseEFlee", new CheckBox("Use E"));

            Shield = Principal.AddSubMenu("Shield", "Shield");
            Shield.AddLabel("• Spell E");
            foreach (var Enemy in EntityManager.Heroes.Enemies)
            {
                Shield.AddLabel(Enemy.ChampionName);
                Shield.Add("E/" + Enemy.BaseSkinName + "/Q", new CheckBox(Enemy.ChampionName + " (Q)", false));
                Shield.Add("E/" + Enemy.BaseSkinName + "/W", new CheckBox(Enemy.ChampionName + " (W)", false));
                Shield.Add("E/" + Enemy.BaseSkinName + "/E", new CheckBox(Enemy.ChampionName + " (E)", false));
                Shield.Add("E/" + Enemy.BaseSkinName + "/R", new CheckBox(Enemy.ChampionName + " (R)", false));
                Shield.AddSeparator(1);
            }




            Laneclear = Principal.AddSubMenu("Laneclear", "Laneclear");
            Laneclear.AddLabel("• WaweClean");
            Laneclear.Add("UseQLane", new CheckBox("Use Q"));
            Laneclear.Add("UseWLane", new CheckBox("Use W"));
            Laneclear.Add("UseWLaneMin", new Slider("Use W if you hit {0} minions", 3, 0, 10));
            Laneclear.AddLabel("• JunglClean");
            Laneclear.Add("UseQJG", new CheckBox("Use Q"));
            Laneclear.Add("UseWJG", new CheckBox("Use W"));
            Laneclear.Add("UseEJG", new CheckBox("Use E"));
           


            Misc = Principal.AddSubMenu("Misc", "Misc");
            Misc.Add("Skin", new CheckBox("Skinhack ?", false));
            Misc.Add("SkinID", new Slider("Skin ID: {0}", 4, 0, 11));
            Misc.Add("Interrupter", new CheckBox("Interrupter ?"));
            Misc.Add("InterrupterW", new CheckBox("Interrupter with W ?"));
            Misc.Add("Gapcloser", new CheckBox("Gapcloser ?"));
            Misc.Add("GapcloserW", new CheckBox("Use W on Gapcloser ?"));
            Misc.AddLabel("• ItemLogic");
            Misc.AddLabel("• Hydra Logic");
            Misc.Add("Hydra", new CheckBox("Use Hydra?"));
            Misc.Add("HydraReset", new CheckBox("Use hydra to reset your AA"));
            Misc.AddSeparator(3);
            Misc.AddLabel("• Tiamat Logic");
            Misc.Add("Tiamat", new CheckBox("Use Tiamat?"));
            Misc.Add("TiamatReset", new CheckBox("Use the Tiamat to reset your AA"));
            Misc.AddSeparator(3);
            Misc.AddLabel("• Qss / Mercurial Logic");
            Misc.Add("Qss", new CheckBox("Use Qss?"));
            Misc.Add("QssCharm", new CheckBox("Use Qss because of charm"));
            Misc.Add("QssFear", new CheckBox("Use Qss because of fear"));
            Misc.Add("QssTaunt", new CheckBox("Use Qss because of taunt"));
            Misc.Add("QssSuppression", new CheckBox("Use Qss because of suppression"));
            Misc.Add("QssSnare", new CheckBox("Use Qss because of snare"));
            Misc.AddSeparator(3);
            Misc.AddLabel("• Youmu Logic");
            Misc.Add("Youmu", new CheckBox("Use Youmu?"));
            Misc.AddLabel("• Recommend Use 250•");
            Misc.Add("YoumuRange", new Slider("Range Cast Youmu", 1, 1, 400));
            

            Draw = Principal.AddSubMenu("Drawing", "Drawing");
            Draw.Add("DrawDamage", new CheckBox("Draw Damage"));
            Draw.Add("DrawOFF", new CheckBox("Draw OFF", false));
            Draw.Add("DrawKILL", new CheckBox("Draw Kill in combo", false));


            AnimationCancle = Principal.AddSubMenu("AnimationCancle", "CanslAnimatio");
            AnimationCancle.Add("4", new CheckBox("Q"));
            AnimationCancle.Add("Spell2", new CheckBox("W"));
            AnimationCancle.Add("Spell3", new CheckBox("E"));
            AnimationCancle.Add("Spell4", new CheckBox("R"));




        }
        //Cancler
        public static bool AnimationCancelQ
        {
            get { return (AnimationCancle["Spell1"].Cast<CheckBox>().CurrentValue); }
        }
        public static bool AnimationCancelW
        {
            get { return (AnimationCancle["Spell2"].Cast<CheckBox>().CurrentValue); }
        }
        public static bool AnimationCancelE
        {
            get { return (AnimationCancle["Spell3"].Cast<CheckBox>().CurrentValue); }
        }
        public static bool AnimationCancelR
        {
            get { return (AnimationCancle["Spell4"].Cast<CheckBox>().CurrentValue); }
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