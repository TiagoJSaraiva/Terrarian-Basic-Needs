using System;

namespace ChallengingTerrariaMod.Common.Logic
{
    public enum SanityStatus
    {
        Well,
        Stressed,
        Scared,
        Terrified,
        Gloom, // Sanity at zero: a countdown to death starts
    }

    // Snapshot of everything the sanity rules need to know about a player in one update.
    public struct SanityContext
    {
        public bool ArmoredMind;
        public bool InHostileZone; // Dungeon, Underworld, Corruption or Crimson
        public bool LowHealth;
        public bool EclipseOrBloodMoon;
        public bool InOverworld;
        public float TownNPCs; // Terraria counts some NPCs as fractions
        public float Multiplier;
    }

    // Pure sanity rules. Must not reference Terraria so it can be unit tested.
    public static class SanityLogic
    {
        public const int UpdateRate = 60; // Sanity is updated once per second

        public const float Max = 1200;
        public const float Min = 0;

        public const float StressedThreshold = 900;
        public const float ScaredThreshold = 600;
        public const float TerrifiedThreshold = 300;

        public const float TownGain = 9;
        public const float OverworldGain = 2;
        public const float TownNPCsForGain = 2; // Needs more than this

        // Sanity lost per second, without and with the Armored Mind buff
        private static readonly LossFactors NormalLoss = new(hostileZone: 4f, lowHealth: 9f, eclipseOrBloodMoon: 7f);
        private static readonly LossFactors ArmoredMindLoss = new(hostileZone: 2.5f, lowHealth: 6f, eclipseOrBloodMoon: 3.5f);

        private readonly struct LossFactors
        {
            public readonly float HostileZone;
            public readonly float LowHealth;
            public readonly float EclipseOrBloodMoon;

            public LossFactors(float hostileZone, float lowHealth, float eclipseOrBloodMoon)
            {
                HostileZone = hostileZone;
                LowHealth = lowHealth;
                EclipseOrBloodMoon = eclipseOrBloodMoon;
            }
        }

        public static SanityStatus GetStatus(float sanity)
        {
            if (sanity <= Min) return SanityStatus.Gloom;
            if (sanity <= TerrifiedThreshold) return SanityStatus.Terrified;
            if (sanity <= ScaredThreshold) return SanityStatus.Scared;
            if (sanity <= StressedThreshold) return SanityStatus.Stressed;
            return SanityStatus.Well;
        }

        // Sanity change in one update (one second).
        public static float Delta(in SanityContext context)
        {
            LossFactors loss = context.ArmoredMind ? ArmoredMindLoss : NormalLoss;
            float delta = 0f;

            if (context.InHostileZone) delta -= loss.HostileZone * context.Multiplier;
            if (context.TownNPCs > TownNPCsForGain) delta += TownGain;
            if (context.LowHealth) delta -= loss.LowHealth * context.Multiplier;
            if (context.EclipseOrBloodMoon) delta -= loss.EclipseOrBloodMoon * context.Multiplier;
            if (context.InOverworld) delta += OverworldGain;

            return delta;
        }

        public static float Clamp(float sanity) => Math.Clamp(sanity, Min, Max);

        public static bool IsLowHealth(int life, int maxLife) => life <= maxLife / 2;

        // Low sanity makes the world look darker. null means "don't change the lighting".
        public static float? LightingScale(float sanity)
        {
            if (sanity <= Min) return 0.75f;
            if (sanity <= TerrifiedThreshold) return 0.85f;
            if (sanity <= ScaredThreshold) return 0.90f;
            return null;
        }
    }
}
