using System.Reflection;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System.Drawing;
using System;
using EloBuddy;
using EloBuddy.SDK.Events;
using HTTF_Yasuo.Utils;
using SharpDX;
using System.Linq;

namespace HTTF_Yasuo
    {
    class Yasuo
    {
        public static Menu Principal, Combo, Misc, Flee, Clean, Draw, Evadee;
        private static bool IsDead;

        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            Chat.Print("Yasuo HTTF (Beta)");
            Game.OnTick += Game_OnTick;
            Game.OnUpdate += OnGameUpdate;
            

        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            Principal = MainMenu.AddMenu("HTTF Yasuo ", "Yasuo");
            Principal.AddLabel("HTTF Yasuo v" + Assembly.GetExecutingAssembly().GetName().Version);

            //combo+harasse
            Combo = Principal.AddSubMenu("Combo", "Combo");
            Combo.AddSeparator(3);
            Combo.AddLabel("• Spells Combo");
            Combo.Add("UseQCombo", new CheckBox("Use Q"));
            Combo.Add("UseWCombo", new CheckBox("Use W"));
            Combo.Add("UseECombo", new CheckBox("Use E"));
            Combo.Add("UseRCombo", new CheckBox("Use R"));
            Combo.Add("stack.combo", new CheckBox("Stack Q?"));
            Combo.Add("combo.leftclickRape", new CheckBox("ComboTarget use?"));
            Combo.Add("PredictQ2", new ComboBox("Predict Q2", 1, "Minimal", "Medium", "High"));

            Combo.AddLabel("• R Setting ");
            Combo.Add("combo.RTarget", new CheckBox("Use R always on Selected Target"));
            Combo.Add("combo.RKillable", new CheckBox("Use R KS"));
            Combo.Add("combo.MinTargetsR", new Slider("Use R Min Targets", 2, 1, 5));

            Combo.AddLabel("• Harasse ");
            Combo.Add("Auto.Q3", new CheckBox("Auto Q3 ?"));
            Combo.Add("harass.Q", new CheckBox("Use Q"));
            Combo.Add("harass.E", new CheckBox("Use E"));
            Combo.Add("harass.stack", new CheckBox("Stack Q in harras?"));
            //clean
            Clean = Principal.AddSubMenu("Clean", "Clean Setting");
            Clean.AddSeparator(3);
            Clean.AddLabel("•Last Hit•");
            Clean.Add("LastE", new CheckBox("Use E"));
            Clean.Add("LastQ", new CheckBox("Use Q"));
            Clean.Add("LaseEUT", new CheckBox("Use E Under Tower"));
            Clean.AddLabel("•Wave Clean•");
            Clean.Add("WCQ", new CheckBox("Use Q"));
            Clean.Add("WCE", new CheckBox("Use E"));
            Clean.AddLabel("•Jung Clean•");
            Clean.Add("JungQ", new CheckBox("Use Q"));
            Clean.Add("JungE", new CheckBox("Use E"));

            //flee
            Flee = Principal.AddSubMenu("Flee", "Flee Setting");
            Flee.AddSeparator(3);
            Flee.AddLabel("•Flee•");
            Flee.Add("FleeE", new CheckBox("Use E"));
            Flee.Add("Flee.stack", new CheckBox("Stack Q In Flee?"));
            //Draw
            Draw = Principal.AddSubMenu("Draw", "Draw Setting");
            Draw.AddSeparator(3);
            Draw.Add("DrawE", new CheckBox("Draw E range"));
            Draw.Add("DrawQ", new CheckBox("Draw Q range"));
            Draw.Add("DrawR", new CheckBox("Draw R range"));
            Draw.Add("DrawDmg", new CheckBox("Draw Damage?"));
            //Misc
            Misc = Principal.AddSubMenu("Misc", "Misc Setting");
            Misc.AddSeparator(3);
            Misc.Add("EGapclos", new CheckBox("E ANTIGAPCLOSE"));
            Misc.Add("Einter", new CheckBox("E INTERRUPT"));
            Misc.AddLabel("• SkinHack •");
            Misc.Add("checkSkin", new CheckBox("Use Skin Changer"));
            Misc.Add("Skinid", new Slider("Skin ID", 0, 0, 10));

            //Evade
            Evadee = Principal.AddSubMenu("Evadee", "Evadee Setting");
            Evadee.AddSeparator(3);
            Evadee.AddLabel("•SOON•");







            Utils.ForDash.Init();


        }
        private static void OnGameUpdate(EventArgs args)
        {
            if (CheckSkin())
            {
                EloBuddy.Player.SetSkinId(SkinId());
            }
        }
        private static int SkinId()
        {
            return Misc["Skinid"].Cast<Slider>().CurrentValue;
        }

        private static bool CheckSkin()
        {
            return Misc["checkSkin"].Cast<CheckBox>().CurrentValue;
        }

        private static void Game_OnTick(EventArgs args)
            {
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Flee))
                {
                    StateLogic.Flee();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
                {
                    StateLogic.Harass();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
                {
                    StateLogic.Combo();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
                {
                    StateLogic.LastHit();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
                {
                    StateLogic.LineClearn();
                }
                if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.JungleClear))
                {
                    StateLogic.Jungle();
                }
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
