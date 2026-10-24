using System;

namespace ChallengingTerrariaMod.Common.Logic
{
    public enum WarmthArrow
    {
        None, // Base sprite, no arrow
        Up,
        Down,
    }

    public readonly struct WarmthSprite
    {
        public readonly int Index; // 1..WarmthSpriteCount, ignored when Arrow is None
        public readonly WarmthArrow Arrow;

        public WarmthSprite(int index, WarmthArrow arrow)
        {
            Index = index;
            Arrow = arrow;
        }
    }

    // Which sprite each meter shows for a given value. Indexes match the PNG file names.
    public static class MeterSprites
    {
        public const int HungerSpriteCount = 15;
        public const int RestSpriteCount = 12;
        public const int SanitySpriteCount = 12;
        public const int WarmthSpriteCount = 37;

        private const int HungerNormalIndex = 11; // Sprite for MaxNormal hunger
        private const float PointsPerSprite = 100f;

        private const int WarmthCenterIndex = 19;
        private const int WarmthDegreesPerSprite = 52;

        public static int Hunger(float hunger)
        {
            float distance = hunger - HungerLogic.MaxNormal;
            int steps = (int)Math.Floor(Math.Abs(distance) / PointsPerSprite);
            int index = distance > 0 ? HungerNormalIndex + steps : HungerNormalIndex - steps;
            return Math.Clamp(index, 0, HungerSpriteCount - 1);
        }

        public static int Rest(float rest) => Math.Clamp((int)Math.Floor(rest / PointsPerSprite), 0, RestSpriteCount - 1);

        public static int Sanity(float sanity) => Math.Clamp((int)Math.Floor(sanity / PointsPerSprite), 0, SanitySpriteCount - 1);

        // Hotter temperatures use lower indexes. The arrow shows where the temperature is heading.
        public static WarmthSprite Warmth(int temperature, int lastChange)
        {
            if (temperature == TemperatureLogic.Comfortable && lastChange == 0)
            {
                return new WarmthSprite(0, WarmthArrow.None);
            }

            int distance = Math.Abs(temperature - TemperatureLogic.Comfortable);
            int steps = Math.Max(1, distance / WarmthDegreesPerSprite);
            bool hot = temperature > TemperatureLogic.Comfortable;

            int index = hot ? WarmthCenterIndex - (steps - 1) : WarmthCenterIndex + (steps - 1);
            index = Math.Clamp(index, 1, WarmthSpriteCount);

            WarmthArrow arrow;
            if (lastChange > 0) arrow = WarmthArrow.Up;
            else if (lastChange < 0) arrow = WarmthArrow.Down;
            else arrow = hot ? WarmthArrow.Down : WarmthArrow.Up; // Idle: drifting back to comfortable

            return new WarmthSprite(index, arrow);
        }
    }
}
