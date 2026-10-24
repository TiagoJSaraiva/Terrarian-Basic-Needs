using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    public class TemperatureLogicTests
    {
        private const int Comfortable = TemperatureLogic.Comfortable;

        private static TemperatureContext Day() => new TemperatureContext { IsDayTime = true, InOverworld = true };

        [Theory]
        [InlineData(0, TemperatureStatus.Freezing)]
        [InlineData(200, TemperatureStatus.Freezing)]
        [InlineData(201, TemperatureStatus.VeryCold)]
        [InlineData(500, TemperatureStatus.VeryCold)]
        [InlineData(800, TemperatureStatus.Cold)]
        [InlineData(801, TemperatureStatus.Comfortable)]
        [InlineData(1000, TemperatureStatus.Comfortable)]
        [InlineData(1199, TemperatureStatus.Comfortable)]
        [InlineData(1200, TemperatureStatus.Warm)]
        [InlineData(1500, TemperatureStatus.Hot)]
        [InlineData(1800, TemperatureStatus.Scorching)]
        [InlineData(2000, TemperatureStatus.Scorching)]
        public void GetStatus_UsesInclusiveThresholds(int temperature, TemperatureStatus expected)
        {
            Assert.Equal(expected, TemperatureLogic.GetStatus(temperature));
        }

        [Fact]
        public void EnvironmentIncrement_PleasantDay_NoChange()
        {
            Assert.Equal(0, TemperatureLogic.EnvironmentIncrement(Day(), Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_SnowAtNightInHeavyRain_StacksColdSources()
        {
            var context = Day();
            context.IsDayTime = false;
            context.InSnow = true;
            context.Raining = true;
            context.HeavyRain = true;
            Assert.Equal(-3 - 8 - 5, TemperatureLogic.EnvironmentIncrement(context, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_LightRain_LessCold()
        {
            var context = Day();
            context.Raining = true;
            Assert.Equal(-3, TemperatureLogic.EnvironmentIncrement(context, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_RainUnderground_NoEffect()
        {
            var context = Day();
            context.InOverworld = false;
            context.Raining = true;
            Assert.Equal(0, TemperatureLogic.EnvironmentIncrement(context, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_DesertAndBeachOnlyHotDuringDay()
        {
            var context = Day();
            context.InDesert = true;
            context.InBeach = true;
            Assert.Equal(8, TemperatureLogic.EnvironmentIncrement(context, Comfortable));

            context.IsDayTime = false;
            Assert.Equal(-3, TemperatureLogic.EnvironmentIncrement(context, Comfortable)); // Only the overworld night is left
        }

        [Fact]
        public void EnvironmentIncrement_UnderworldAndSky()
        {
            var underworld = new TemperatureContext { IsDayTime = true, InUnderworld = true };
            Assert.Equal(8, TemperatureLogic.EnvironmentIncrement(underworld, Comfortable));

            var sky = new TemperatureContext { IsDayTime = true, InSky = true };
            Assert.Equal(30, TemperatureLogic.EnvironmentIncrement(sky, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_HeatSourceWarmsUpColdPlayer()
        {
            var context = Day();
            context.NearHeatSource = true;
            Assert.Equal(20, TemperatureLogic.EnvironmentIncrement(context, Comfortable - 1));
            Assert.Equal(0, TemperatureLogic.EnvironmentIncrement(context, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_WaterCoolsDownHotPlayer()
        {
            var context = Day();
            context.InWater = true;
            Assert.Equal(-20, TemperatureLogic.EnvironmentIncrement(context, Comfortable + 1));
            Assert.Equal(0, TemperatureLogic.EnvironmentIncrement(context, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_SnowWithCampfire_WarmsUpWhenCold()
        {
            var context = Day();
            context.InSnow = true;
            context.NearHeatSource = true;
            Assert.Equal(-8 + 20, TemperatureLogic.EnvironmentIncrement(context, 700));
        }

        // The potions only change how fast an overheated/frozen player recovers (kept on purpose).
        [Fact]
        public void EnvironmentIncrement_WarmthPotion_SlowsCoolingWhenHot()
        {
            var context = Day();
            context.InWater = true;
            context.HasWarmthPotion = true;
            Assert.Equal(-16, TemperatureLogic.EnvironmentIncrement(context, 1500));
            Assert.Equal(0, TemperatureLogic.EnvironmentIncrement(context, 700));
        }

        [Fact]
        public void EnvironmentIncrement_ObsidianSkin_SlowsWarmingWhenCold()
        {
            var context = Day();
            context.NearHeatSource = true;
            context.HasObsidianSkin = true;
            Assert.Equal(17, TemperatureLogic.EnvironmentIncrement(context, 700));
            Assert.Equal(0, TemperatureLogic.EnvironmentIncrement(context, 1500));
        }

        [Theory]
        [InlineData(-8, 1f, -8)]
        [InlineData(-8, 2f, -16)]
        [InlineData(-8, 0.5f, 0)] // A1: fractional multipliers are truncated
        [InlineData(-8, 1.9f, -8)]
        public void ApplyMultiplier_CurrentBehavior_A1(int increment, float multiplier, int expected)
        {
            Assert.Equal(expected, TemperatureLogic.ApplyMultiplier(increment, multiplier));
        }

        [Theory]
        [InlineData(1000, 0, 1000)]
        [InlineData(1100, 0, 1090)]
        [InlineData(1005, 0, 1000)]
        [InlineData(900, 0, 910)]
        [InlineData(995, 0, 1000)]
        public void Step_WithoutIncrement_DriftsBackToComfortable(int temperature, int increment, int expected)
        {
            Assert.Equal(expected, TemperatureLogic.Step(temperature, increment));
        }

        [Theory]
        [InlineData(1100, -8, 1092)]
        [InlineData(5, -8, 0)]
        [InlineData(1990, 30, 2000)]
        public void Step_WithIncrement_AppliesAndClamps(int temperature, int increment, int expected)
        {
            Assert.Equal(expected, TemperatureLogic.Step(temperature, increment));
        }
    }
}
