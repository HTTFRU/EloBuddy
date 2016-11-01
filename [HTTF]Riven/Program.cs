using System;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using SharpDX;

namespace HTTF_Riven_v2
{
    class Program
    {

        public static Text Status;
        public static int CountQ;

        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            if (Player.Instance.ChampionName != "Riven")
                return;

            RivenMenu.Load();
            Riven.Load();

        }


    }
}
