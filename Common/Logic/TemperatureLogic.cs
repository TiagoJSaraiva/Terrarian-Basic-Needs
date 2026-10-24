using System;

namespace ChallengingTerrariaMod.Common.Logic
{
    public enum TemperatureStatus
    {
        Comfortable,
        Cold,
        VeryCold,
        Freezing,
        Warm,
        Hot,
        Scorching,
    }

    // Snapshot of everything the temperature rules need to know about a player in one update.
    public struct TemperatureContext
    {
        public bool InWater; // Water only, not lava or honey
        public bool IsDayTime;
        public bool InOverworld;
        public bool InSnow;
        public bool Raining;
        public bool HeavyRain;
        public bool NearHeatSource; // Campfire, furnace, fireplace... within DetectionRadiusTiles
        public bool InBeach;
        public bool InDesert;
        public bool InUnderworld;
        public bool InSky;
        public bool HasWarmthPotion;
        public bool HasObsidianSkin;
    }

    // Pure temperature rules. Must not reference Terraria so it can be unit tested.
    public static class TemperatureLogic
    {
        public const int UpdateRate = 60; // Temperature is updated once per second
        public const int DetectionRadiusTiles = 15;

        public const int Comfortable = 1000;
        public const int Min = 0;
        public const int Max = 2000;
        public const int NormalizationRate = 10;

        public const int FreezingThreshold = 200;
        public const int VeryColdThreshold = 500;
        public const int ColdThreshold = 800;
        public const int WarmThreshold = 1200;
        public const int HotThreshold = 1500;
        public const int ScorchingThreshold = 1800;

        // Pull back towards comfortable when the environment is the opposite of how the player feels
        private const int EnvironmentRecovery = 20;

        // Potion modifiers on that pull back. Both are 0: the potions don't affect temperature (author's decision).
        public const int WarmthPotionModifier = 0;
        public const int ObsidianSkinModifier = 0;

        public static TemperatureStatus GetStatus(int temperature)
        {
            if (temperature <= FreezingThreshold) return TemperatureStatus.Freezing;
            if (temperature <= VeryColdThreshold) return TemperatureStatus.VeryCold;
            if (temperature <= ColdThreshold) return TemperatureStatus.Cold;
            if (temperature >= ScorchingThreshold) return TemperatureStatus.Scorching;
            if (temperature >= HotThreshold) return TemperatureStatus.Hot;
            if (temperature >= WarmThreshold) return TemperatureStatus.Warm;
            return TemperatureStatus.Comfortable;
        }

        // Temperature change from the environment in one update, before the config multiplier.
        public static int EnvironmentIncrement(in TemperatureContext context, int currentTemperature)
        {
            int increment = 0;
            bool coldEnvironment = context.InWater;
            bool warmEnvironment = context.NearHeatSource;

            if (!context.IsDayTime && context.InOverworld)
            {
                increment -= 3;
                coldEnvironment = true;
            }
            if (context.InSnow)
            {
                increment -= 8;
                coldEnvironment = true;
            }
            if (context.Raining && context.InOverworld)
            {
                increment -= context.HeavyRain ? 5 : 3;
                coldEnvironment = true;
            }

            if (context.InBeach && context.IsDayTime)
            {
                increment += 3;
                warmEnvironment = true;
            }
            if (context.InDesert && context.IsDayTime)
            {
                increment += 5;
                warmEnvironment = true;
            }
            if (context.InUnderworld)
            {
                increment += 8;
                warmEnvironment = true;
            }
            if (context.InSky)
            {
                increment += 30;
                warmEnvironment = true;
            }

            // A cold place cools down an overheated player (and vice versa)
            if (coldEnvironment && currentTemperature > Comfortable)
            {
                if (context.HasWarmthPotion) increment += WarmthPotionModifier;
                increment -= EnvironmentRecovery;
            }
            if (warmEnvironment && currentTemperature < Comfortable)
            {
                if (context.HasObsidianSkin) increment += ObsidianSkinModifier;
                increment += EnvironmentRecovery;
            }

            return increment;
        }

        // Scales the environment increment by the config multiplier. Temperature is an integer, so the fraction
        // that doesn't fit is kept in remainder and added on the next update (0.5 x -3 = -1, -2, -1, -2...).
        public static int ApplyMultiplier(int increment, float multiplier, ref float remainder)
        {
            float scaled = increment * multiplier + remainder;
            int whole = (int)scaled; // Truncates towards zero, so the remainder keeps the sign of the change
            remainder = scaled - whole;
            return whole;
        }

        // New temperature after one update. Without any environmental change, it drifts back to comfortable.
        public static int Step(int currentTemperature, int environmentIncrement, float multiplier, ref float remainder)
        {
            if (environmentIncrement == 0)
            {
                remainder = 0f;
                if (currentTemperature > Comfortable) return Math.Max(Comfortable, currentTemperature - NormalizationRate);
                if (currentTemperature < Comfortable) return Math.Min(Comfortable, currentTemperature + NormalizationRate);
                return currentTemperature;
            }

            int temperature = currentTemperature + ApplyMultiplier(environmentIncrement, multiplier, ref remainder);
            return Math.Clamp(temperature, Min, Max);
        }
    }
}
