using System;

namespace ChallengingTerrariaMod.Common.Logic
{
    public enum RestStatus
    {
        Rested,
        Tired,
        Sleepy,
        Exhausted,
        Fainted,
    }

    // Snapshot of everything the rest rules need to know about a player in one update.
    public struct RestContext
    {
        public bool IsSleeping;
        public bool IsSitting;
        public bool IsDayTime;
        public bool TimeAccelerated; // Main.dayRate > 1 (everyone in bed, enchanted sundial...)
        public bool SleepDeprived;
        public bool Caffeinated;
        public int TownNPCs;
        public float Multiplier;
    }

    // Pure rest rules. Must not reference Terraria so it can be unit tested.
    public static class RestLogic
    {
        public const int UpdateRate = 60; // Rest is updated once per second

        public const float Max = 1200;
        public const float Min = 0;

        public const float TiredThreshold = 900;
        public const float SleepyThreshold = 600;
        public const float ExhaustedThreshold = 300;
        public const float FaintedThreshold = 0;

        public const float GainPerSecond = 4;
        public const float GainPerSecondAccelerated = 24;

        public const int MinTownNPCsForGoodSleep = 2;

        public const float RestAfterFainting = ExhaustedThreshold;
        public const int FaintedDurationSeconds = 10;

        // Seconds awake before getting Sleep Deprived
        public const int SleepDeprivationLimit = 2400;
        public const int SleepDeprivedDurationSeconds = 600;
        public const int TimeNoSleepRecoveredPerSecond = 30;
        public const int TimeNoSleepCapWhileSleeping = 1200;

        public static RestStatus GetStatus(float rest)
        {
            if (rest <= FaintedThreshold) return RestStatus.Fainted;
            if (rest <= ExhaustedThreshold) return RestStatus.Exhausted;
            if (rest <= SleepyThreshold) return RestStatus.Sleepy;
            if (rest <= TiredThreshold) return RestStatus.Tired;
            return RestStatus.Rested;
        }

        // Rest change in one update (one second). badRest tells whether the Bad Rest buff should be shown.
        public static float Delta(in RestContext context, out bool badRest)
        {
            badRest = false;

            if (context.IsSleeping)
            {
                if (context.TownNPCs < MinTownNPCsForGoodSleep)
                {
                    badRest = true;
                    return context.TimeAccelerated ? GainPerSecondAccelerated / 2 : GainPerSecond / 4;
                }
                return context.TimeAccelerated ? GainPerSecondAccelerated : GainPerSecond;
            }

            if (context.IsSitting)
            {
                badRest = true;
                return GainPerSecond / 4;
            }

            if (context.SleepDeprived || !context.IsDayTime)
            {
                float loss = GainPerSecond * context.Multiplier;
                return context.Caffeinated ? -loss / 2 : -loss;
            }

            return 0f;
        }

        public static float Clamp(float rest) => Math.Clamp(rest, Min, Max);

        public static int TimeNoSleepWhileSleeping(int timeNoSleep)
            => Math.Clamp(timeNoSleep - TimeNoSleepRecoveredPerSecond, 0, TimeNoSleepCapWhileSleeping);

        // Divides a max life/mana value and rounds it down to a multiple of step (life crystals give 20 each).
        public static int RoundDownToStep(int value, float divisor, int step = 20)
        {
            int divided = (int)(value / divisor);
            int remainder = ((divided % step) + step) % step;
            return divided - remainder;
        }
    }
}
