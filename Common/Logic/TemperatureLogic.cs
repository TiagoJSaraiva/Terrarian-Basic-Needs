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

            // A cold place cools down an overheated player (and vice versa).
            // The potions only change how fast that happens: Warmth slows the cooling,
            // Obsidian Skin slows the warming. They do not protect against cold or heat.
            if (coldEnvironment && currentTemperature > Comfortable)
            {
                if (context.HasWarmthPotion) increment += 4;
                increment -= EnvironmentRecovery;
            }
            if (warmEnvironment && currentTemperature < Comfortable)
            {
                if (context.HasObsidianSkin) increment -= 3;
                increment += EnvironmentRecovery;
            }

            return increment;
        }

        public static int ApplyMultiplier(int increment, float multiplier) => increment * (int)multiplier;

        // New temperature after one update. Without any environmental change, it drifts back to comfortable.
        public static int Step(int currentTemperature, int increment)
        {
            int temperature = currentTemperature + increment;

            if (increment == 0)
            {
                if (temperature > Comfortable) temperature = Math.Max(Comfortable, temperature - NormalizationRate);
                else if (temperature < Comfortable) temperature = Math.Min(Comfortable, temperature + NormalizationRate);
            }

            return Math.Clamp(temperature, Min, Max);
        }
    }
}
