using System;
using System.Collections.Generic;
using System.Threading;
using EloBuddy;
using EloBuddy.SDK;

namespace _HTTF_Riven_v2
{
    internal static class Utils
    {
        public static void DelayAction(Action func, int delay)
        {
            /*
            Timer timer = null;
            timer = new Timer(obj =>
            {
                func();
                timer.Dispose();
            },
                null, delay, Timeout.Infinite);
             * */
            Core.DelayAction(func, delay);
        }

        public static class DelayAction2
        {
            public delegate void Callback();

            public static List<DelayedAction> ActionList = new List<DelayedAction>();

            static DelayAction2()
            {
                Game.OnUpdate += GameOnOnGameUpdate;
            }

            private static void GameOnOnGameUpdate(EventArgs args)
            {
                for (var i = ActionList.Count - 1; i >= 0; i--)
                {
                    if (ActionList[i].Time <= Environment.TickCount)
                    {
                        try
                        {
                            if (ActionList[i].CallbackObject != null)
                            {
                                ActionList[i].CallbackObject();
                                //Will somehow result in calling ALL non-internal marked classes of the called assembly and causes NullReferenceExceptions.
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        ActionList.RemoveAt(i);
                    }
                }
            }

            public static void Add(Action func, int time)
            {
                var action = new DelayedAction(time, func);
                ActionList.Add(action);
            }

            public struct DelayedAction
            {
                public Action CallbackObject;
                public int Time;

                public DelayedAction(int time, Action callback)
                {
                    Time = time + Environment.TickCount;
                    CallbackObject = callback;
                }
            }
        }
    }
}
