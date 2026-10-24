using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    public class RestLogicTests
    {
        private static RestContext Awake(bool day = true) => new RestContext { IsDayTime = day, Multiplier = 1f, TownNPCs = 5 };

        [Theory]
        [InlineData(1200, RestStatus.Rested)]
        [InlineData(900.1f, RestStatus.Rested)]
        [InlineData(900, RestStatus.Tired)]
        [InlineData(600, RestStatus.Sleepy)]
        [InlineData(300, RestStatus.Exhausted)]
        [InlineData(0.1f, RestStatus.Exhausted)]
        [InlineData(0, RestStatus.Fainted)]
        public void GetStatus_UsesInclusiveThresholds(float rest, RestStatus expected)
        {
            Assert.Equal(expected, RestLogic.GetStatus(rest));
        }

        [Fact]
        public void Delta_AwakeDuringDay_NoChange()
        {
            Assert.Equal(0f, RestLogic.Delta(Awake(), out bool badRest));
            Assert.False(badRest);
        }

        [Fact]
        public void Delta_AwakeAtNight_LosesRest()
        {
            Assert.Equal(-4f, RestLogic.Delta(Awake(day: false), out _));
        }

        [Fact]
        public void Delta_SleepDeprived_LosesRestEvenDuringDay()
        {
            var context = Awake();
            context.SleepDeprived = true;
            Assert.Equal(-4f, RestLogic.Delta(context, out _));
        }

        [Fact]
        public void Delta_Caffeinated_HalvesLoss()
        {
            var context = Awake(day: false);
            context.Caffeinated = true;
            Assert.Equal(-2f, RestLogic.Delta(context, out _));
        }

        [Fact]
        public void Delta_Loss_ScalesWithMultiplier()
        {
            var context = Awake(day: false);
            context.Multiplier = 2.5f;
            Assert.Equal(-10f, RestLogic.Delta(context, out _));
        }

        [Fact]
        public void Delta_SleepingInTown_GoodRest()
        {
            var context = Awake(day: false);
            context.IsSleeping = true;
            Assert.Equal(4f, RestLogic.Delta(context, out bool badRest));
            Assert.False(badRest);

            context.TimeAccelerated = true;
            Assert.Equal(24f, RestLogic.Delta(context, out _));
        }

        [Fact]
        public void Delta_SleepingOutsideTown_BadRest()
        {
            var context = Awake(day: false);
            context.IsSleeping = true;
            context.TownNPCs = 1.5f;
            Assert.Equal(1f, RestLogic.Delta(context, out bool badRest));
            Assert.True(badRest);

            context.TimeAccelerated = true;
            Assert.Equal(12f, RestLogic.Delta(context, out _));
        }

        [Fact]
        public void Delta_Gain_IgnoresMultiplier()
        {
            var context = Awake(day: false);
            context.IsSleeping = true;
            context.Multiplier = 5f;
            Assert.Equal(4f, RestLogic.Delta(context, out _));
        }

        [Fact]
        public void Delta_Sitting_SlowBadRest()
        {
            var context = Awake(day: false);
            context.IsSitting = true;
            Assert.Equal(1f, RestLogic.Delta(context, out bool badRest));
            Assert.True(badRest);
        }

        [Theory]
        [InlineData(10, 0)]
        [InlineData(100, 70)]
        public void TimeNoSleepWhileSleeping_Recovers30PerSecond(int timeNoSleep, int expected)
        {
            Assert.Equal(expected, RestLogic.TimeNoSleepWhileSleeping(timeNoSleep));
        }

        // A8: one second of sleep used to cut the progress towards Sleep Deprived from 2399 down to 1200
        [Fact]
        public void TimeNoSleepWhileSleeping_DoesNotJumpDown()
        {
            Assert.Equal(2369, RestLogic.TimeNoSleepWhileSleeping(2399));
        }

        [Theory]
        [InlineData(false, false, false, false, 1f)]
        [InlineData(true, false, false, false, 0.8f)]
        [InlineData(false, true, false, false, 0.6f)]
        [InlineData(false, false, true, false, 0.4f)]
        [InlineData(false, false, false, true, 0.4f)]
        [InlineData(false, false, true, true, 0.4f)] // Exhausted and Sleep Deprived don't stack
        [InlineData(true, false, false, true, 0.32f)]
        public void LifeRegenFactor_DependsOnTiredness(bool tired, bool sleepy, bool exhausted, bool deprived, float expected)
        {
            Assert.Equal(expected, RestLogic.LifeRegenFactor(tired, sleepy, exhausted, deprived), 3);
        }

        [Theory]
        [InlineData(100, 1.2f, 80)] // 83 -> 80
        [InlineData(400, 1.2f, 320)] // 333 -> 320
        [InlineData(500, 1.6f, 300)] // 312 -> 300
        [InlineData(20, 1.4f, 0)] // 14 -> 0
        [InlineData(0, 1.2f, 0)]
        [InlineData(-30, 1f, -40)]
        public void RoundDownToStep_RoundsToMultipleOf20(int value, float divisor, int expected)
        {
            Assert.Equal(expected, RestLogic.RoundDownToStep(value, divisor));
        }
    }
}
