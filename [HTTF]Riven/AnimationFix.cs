using EloBuddy;


namespace HTTF_Riven_v2
{
    class AnimationFix
    {
        class AnimationCancel : AnimationFix
        {
            public void OnLoad()
            {
                Obj_AI_Base.OnPlayAnimation += Obj_AI_Base_OnPlayAnimation;
            }

            private void Obj_AI_Base_OnPlayAnimation(Obj_AI_Base sender, GameObjectPlayAnimationEventArgs args)
            {
                if (!sender.IsMe) return;
                if (ObjectManager.Player.ChampionName == "Riven")
                {
                    switch (args.Animation)
                    {
                        case "Spell1a":
                            if (RivenMenu.AnimationCancelQ)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell1b":
                            if (RivenMenu.AnimationCancelQ)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell1c":
                            if (RivenMenu.AnimationCancelQ)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell3":
                            if (RivenMenu.AnimationCancelE)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell4a":
                            if (RivenMenu.AnimationCancelR)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell4b":
                            if (RivenMenu.AnimationCancelR)
                            {
                                Chat.Say("/d");
                            }
                            break;
                    }
                }
                else
                {
                    switch (args.Animation)
                    {
                        case "Spell1":
                            if (RivenMenu.AnimationCancelQ)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell2":
                            if (RivenMenu.AnimationCancelW)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell3":
                            if (RivenMenu.AnimationCancelE)
                            {
                                Chat.Say("/d");
                            }
                            break;
                        case "Spell4":
                            if (RivenMenu.AnimationCancelR)
                            {
                                Chat.Say("/d");
                            }
                            break;
                    }
                }
            }


            public ModuleType GetModuleType()
            {
                return ModuleType.OnUpdate;
            }

            public bool ShouldGetExecuted()
            {
                return false;
            }

            public void OnExecute()
            {
            }

        }
    }
}