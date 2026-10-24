using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    public class HungerLogicTests
    {
        [Theory]
        [InlineData(1500, HungerStatus.Bloated)]
        [InlineData(1400, HungerStatus.Bloated)]
        [InlineData(1399.9f, HungerStatus.Full)]
        [InlineData(1300, HungerStatus.Full)]
        [InlineData(1299.9f, HungerStatus.WellFed)]
        [InlineData(1200, HungerStatus.WellFed)]
        [InlineData(900.1f, HungerStatus.WellFed)]
        [InlineData(900, HungerStatus.Peckish)]
        [InlineData(600.1f, HungerStatus.Peckish)]
        [InlineData(600, HungerStatus.Hungry)]
        [InlineData(300.1f, HungerStatus.Hungry)]
        [InlineData(300, HungerStatus.Famished)]
        [InlineData(0.1f, HungerStatus.Famished)]
        [InlineData(0, HungerStatus.Starved)]
        public void GetStatus_UsesInclusiveThresholds(float hunger, HungerStatus expected)
        {
            Assert.Equal(expected, HungerLogic.GetStatus(hunger));
        }

        [Fact]
        public void Drain_Moving_LosesTwoPerSecond()
        {
            Assert.Equal(2f, HungerLogic.Drain(isMoving: true, nourished: false, multiplier: 1f));
        }

        [Fact]
        public void Drain_MovingWhileNourished_LosesHalf()
        {
            Assert.Equal(1f, HungerLogic.Drain(isMoving: true, nourished: true, multiplier: 1f));
        }

        [Theory]
        [InlineData(0.5f, 1f)]
        [InlineData(2.5f, 5f)]
        public void Drain_ScalesWithMultiplier(float multiplier, float expected)
        {
            Assert.Equal(expected, HungerLogic.Drain(isMoving: true, nourished: false, multiplier), 3);
        }

        // A6: hunger used to drop only while moving
        [Fact]
        public void Drain_Idle_LosesOnePerSecond()
        {
            Assert.Equal(1f, HungerLogic.Drain(isMoving: false, nourished: false, multiplier: 1f));
        }

        [Fact]
        public void Drain_IdleWhileNourished_LosesHalf()
        {
            Assert.Equal(0.5f, HungerLogic.Drain(isMoving: false, nourished: true, multiplier: 1f));
        }

        [Theory]
        [InlineData(-10, 0)]
        [InlineData(700, 700)]
        [InlineData(1600, 1500)]
        public void Clamp_KeepsHungerInRange(float hunger, float expected)
        {
            Assert.Equal(expected, HungerLogic.Clamp(hunger));
        }

        [Theory]
        [InlineData(1499.9f, false)]
        [InlineData(1500, true)]
        public void ShouldVomit_OnlyAtAbsoluteMax(float hunger, bool expected)
        {
            Assert.Equal(expected, HungerLogic.ShouldVomit(hunger));
        }

        [Theory]
        [InlineData(0, 300)]
        [InlineData(299, 300)]
        [InlineData(800, 800)]
        public void OnRespawn_GuaranteesMinimumHunger(float hunger, float expected)
        {
            Assert.Equal(expected, HungerLogic.OnRespawn(hunger));
        }

        [Theory]
        [InlineData(WellFedTier.None, 0)]
        [InlineData(WellFedTier.WellFed, 150)]
        [InlineData(WellFedTier.PlentySatisfied, 250)]
        [InlineData(WellFedTier.ExquisitelyStuffed, 350)]
        public void RestoreAmount_DependsOnFoodTier(WellFedTier tier, int expected)
        {
            Assert.Equal(expected, HungerLogic.RestoreAmount(tier));
        }

        [Theory]
        [InlineData(899.9f, true)]
        [InlineData(900, false)]
        public void ShouldShortenFoodBuff_WhenStillPeckish(float hunger, bool expected)
        {
            Assert.Equal(expected, HungerLogic.ShouldShortenFoodBuff(hunger));
        }
    }
}
