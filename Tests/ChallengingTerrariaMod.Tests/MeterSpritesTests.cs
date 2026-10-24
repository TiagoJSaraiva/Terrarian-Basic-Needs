using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    public class MeterSpritesTests
    {
        [Theory]
        [InlineData(1200, 11)]
        [InlineData(1150, 11)]
        [InlineData(1100, 10)]
        [InlineData(1299, 11)]
        [InlineData(1300, 12)]
        [InlineData(1500, 14)]
        [InlineData(100, 0)]
        [InlineData(0, 0)]
        [InlineData(5000, 14)]
        public void Hunger_CentersOnNormalAndClamps(float hunger, int expected)
        {
            Assert.Equal(expected, MeterSprites.Hunger(hunger));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(99, 0)]
        [InlineData(100, 1)]
        [InlineData(1100, 11)]
        [InlineData(1200, 11)]
        [InlineData(-50, 0)]
        public void RestAndSanity_OneSpritePer100(float value, int expected)
        {
            Assert.Equal(expected, MeterSprites.Rest(value));
            Assert.Equal(expected, MeterSprites.Sanity(value));
        }

        [Fact]
        public void Warmth_ComfortableAndStable_UsesBaseSprite()
        {
            Assert.Equal(WarmthArrow.None, MeterSprites.Warmth(1000, 0).Arrow);
        }

        [Theory]
        [InlineData(1000, 5, 19, WarmthArrow.Up)]
        [InlineData(1051, 5, 19, WarmthArrow.Up)]
        [InlineData(1104, -5, 18, WarmthArrow.Down)]
        [InlineData(896, 5, 20, WarmthArrow.Up)]
        [InlineData(2000, 0, 1, WarmthArrow.Down)] // Idle and hot: drifting down
        [InlineData(0, 0, 37, WarmthArrow.Up)] // Idle and cold: drifting up
        public void Warmth_IndexAndArrow(int temperature, int lastChange, int expectedIndex, WarmthArrow expectedArrow)
        {
            WarmthSprite sprite = MeterSprites.Warmth(temperature, lastChange);
            Assert.Equal(expectedIndex, sprite.Index);
            Assert.Equal(expectedArrow, sprite.Arrow);
        }

        [Fact]
        public void Warmth_AllTemperaturesMapToExistingSprites()
        {
            for (int t = TemperatureLogic.Min; t <= TemperatureLogic.Max; t++)
            {
                WarmthSprite sprite = MeterSprites.Warmth(t, 1);
                Assert.InRange(sprite.Index, 1, MeterSprites.WarmthSpriteCount);
            }
        }
    }
}
