using System;
using System.Linq;
using Color = System.Drawing.Color;
using EloBuddy;
using EloBuddy.SDK;
using SharpDX;


namespace HTTF_Riven_v2
{
    class DamageIndicator
    {
        private const int BarWidth = 104;
        private const int LineThickness = 10;

        public delegate float DamageToUnitDelegate(AIHeroClient hero);
        private static DamageToUnitDelegate DamageToUnit { get; set; }

        private static Vector2 BarOffset = new Vector2(0, 15);

        private static Color _drawingColor;
        public static Color DrawingColor
        {
            get { return _drawingColor; }
            set { _drawingColor = Color.FromArgb(170, value); }
        }

        public static bool HealthbarEnabled { get; set; }
        public static bool PercentEnabled { get; set; }

        public static void Initialize(DamageToUnitDelegate damageToUnit)
        {
            DamageToUnit = damageToUnit;

            DrawingColor = Color.DarkBlue;
            HealthbarEnabled = RivenMenu.CheckBox(RivenMenu.Draw, "DrawDamage");
            PercentEnabled = RivenMenu.CheckBox(RivenMenu.Draw, "DrawDamage");

            Drawing.OnEndScene += OnEndScene;
        }

        private static void OnEndScene(EventArgs args)
        {
            if (HealthbarEnabled || PercentEnabled)
            {
                foreach (var unit in EntityManager.Heroes.Enemies.Where(u => u.IsHPBarRendered && u.IsValid))
                {
                    var damage = DamageToUnit(unit);

                    if (damage <= 0)
                    {
                        continue;
                    }

                    if (HealthbarEnabled)
                    {
                        var damagePercentage = ((unit.TotalShieldHealth() - 0.9 * damage) > 0 ? (unit.TotalShieldHealth() - damage) : 0) /
                                               (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);
                        var currentHealthPercentage = unit.TotalShieldHealth() / (unit.MaxHealth + unit.AllShield + unit.AttackShield + unit.MagicShield);

                        var startPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + damagePercentage * BarWidth), (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);
                        var endPoint = new Vector2((int)(unit.HPBarPosition.X + BarOffset.X + currentHealthPercentage * BarWidth) + 1, (int)(unit.HPBarPosition.Y + BarOffset.Y) - 5);

                        Drawing.DrawLine(startPoint, endPoint, LineThickness, System.Drawing.Color.Chartreuse);
                    }

                    if (PercentEnabled)
                    {
                        Drawing.DrawText(new Vector2(unit.HPBarPosition.X - 2, unit.HPBarPosition.Y + 30), Color.Red, string.Concat(Math.Ceiling((damage / unit.TotalShieldHealth()) * 100)), 10);
                    }
                }
            }
        }
    }
}