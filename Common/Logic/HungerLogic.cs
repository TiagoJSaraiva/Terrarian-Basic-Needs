using System;

namespace ChallengingTerrariaMod.Common.Logic
{
    public enum HungerStatus
    {
        WellFed,
        Peckish,
        Hungry,
        Famished,
        Starved,
        Full,
        Bloated,
    }

    // Vanilla food buffs, from weakest to strongest.
    public enum WellFedTier
    {
        None,
        WellFed,
        PlentySatisfied,
        ExquisitelyStuffed,
    }

    // Pure hunger rules. Must not reference Terraria so it can be unit tested.
    public static class HungerLogic
    {
        public const int TickRate = 60; // Hunger is updated once per second

        public const float MaxNormal = 1200; // "Full" stomach without any buff/debuff
        public const float AbsoluteMax = 1500; // Reaching this makes the player throw up

        public const float BloatedThreshold = 1400;
        public const float FullThreshold = 1300;
        public const float PeckishThreshold = 900;
        public const float HungryThreshold = 600;
        public const float FamishedThreshold = 300;
        public const float StarvedThreshold = 0;

        public const float DrainIdle = 1f; // Hunger lost per second while standing still
        public const float DrainMoving = 2f; // Hunger lost per second while moving
        public const float NourishedDrainFactor = 0.5f;

        public const float HungerAfterVomiting = 400;
        public const float MinHungerOnRespawn = FamishedThreshold;

        public static HungerStatus GetStatus(float hunger)
        {
            if (hunger >= BloatedThreshold) return HungerStatus.Bloated;
            if (hunger >= FullThreshold) return HungerStatus.Full;
            if (hunger <= StarvedThreshold) return HungerStatus.Starved;
            if (hunger <= FamishedThreshold) return HungerStatus.Famished;
            if (hunger <= HungryThreshold) return HungerStatus.Hungry;
            if (hunger <= PeckishThreshold) return HungerStatus.Peckish;
            return HungerStatus.WellFed;
        }

        // Hunger lost in one update (one second).
        public static float Drain(bool isMoving, bool nourished, float multiplier)
        {
            float drain = isMoving ? DrainMoving : DrainIdle;
            if (nourished) drain *= NourishedDrainFactor;
            return drain * multiplier;
        }

        public static float Clamp(float hunger) => Math.Clamp(hunger, 0f, AbsoluteMax);

        public static bool ShouldVomit(float hunger) => hunger >= AbsoluteMax;

        public static float OnRespawn(float hunger) => Math.Max(hunger, MinHungerOnRespawn);

        public static int RestoreAmount(WellFedTier tier) => tier switch
        {
            WellFedTier.ExquisitelyStuffed => 350,
            WellFedTier.PlentySatisfied => 250,
            WellFedTier.WellFed => 150,
            _ => 0,
        };

        // Eating while still hungry gives only a short food buff, so food buffs can't be farmed while starving.
        public static bool ShouldShortenFoodBuff(float hungerAfterEating) => hungerAfterEating < PeckishThreshold;
    }
}
