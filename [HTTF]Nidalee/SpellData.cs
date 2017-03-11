using System;
using EloBuddy;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Rendering;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;

namespace _HTTF_Nidalee
{
    class SpellData
    {
        
        public static Spell.Skillshot Q1 { get; private set; }
        public static Spell.Active Q2 { get; private set; }
        public static Spell.Skillshot W1 { get; private set; }
        public static Spell.Skillshot W2 { get; private set; }
        public static Spell.Skillshot W2E { get; private set; }
        public static Spell.Targeted E1 { get; private set; }
        public static Spell.Skillshot E2 { get; private set; }
        public static Spell.Active R { get; private set; }

        static SpellData()
        {
            Q1 = new Spell.Skillshot(SpellSlot.Q, 1500, SkillShotType.Linear, 500, 1300, 80);
            Q2 = new Spell.Active(SpellSlot.Q, 200);
            W1 = new Spell.Skillshot(SpellSlot.W, 900, SkillShotType.Circular, 500, 1450, 80);
            W2 = new Spell.Skillshot(SpellSlot.W, 375, SkillShotType.Circular, 500, int.MaxValue, 210);
            W2E = new Spell.Skillshot(SpellSlot.W, 750, SkillShotType.Circular, 500, int.MaxValue, 210);
            E1 = new Spell.Targeted(SpellSlot.E, 600);
            E2 = new Spell.Skillshot(SpellSlot.E, 300, SkillShotType.Cone, 500, int.MaxValue,
                (int)(15.00 * Math.PI / 180.00))
            {
                ConeAngleDegrees = 180
            };
            R = new Spell.Active(SpellSlot.R);
        }
    }

}
