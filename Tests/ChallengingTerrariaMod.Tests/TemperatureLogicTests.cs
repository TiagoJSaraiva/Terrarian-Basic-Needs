using System;
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

        [Fact]
        public void EnvironmentIncrement_WarmthPotion_HalvesColdSourcesOnly()
        {
            var context = Day();
            context.IsDayTime = false;
            context.InSnow = true;
            context.InUnderworld = true;
            context.HasWarmthPotion = true;
            Assert.Equal((-3 - 8) * 0.5f + 8, TemperatureLogic.EnvironmentIncrement(context, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_ObsidianSkin_HalvesHeatSourcesOnly()
        {
            var context = new TemperatureContext { IsDayTime = true, InSky = true, InSnow = true, HasObsidianSkin = true };
            Assert.Equal(30 * 0.5f - 8, TemperatureLogic.EnvironmentIncrement(context, Comfortable));
        }

        [Fact]
        public void EnvironmentIncrement_Potions_DoNotChangeRecovery()
        {
            var water = Day();
            water.InWater = true;
            water.HasWarmthPotion = water.HasObsidianSkin = true;
            Assert.Equal(-20, TemperatureLogic.EnvironmentIncrement(water, 1500));

            var campfire = Day();
            campfire.NearHeatSource = true;
            campfire.HasWarmthPotion = campfire.HasObsidianSkin = true;
            Assert.Equal(20, TemperatureLogic.EnvironmentIncrement(campfire, 700));
        }

        [Fact]
        public void Step_HalfDegreeIncrement_AddsUpOverTime()
        {
            float remainder = 0f;
            int temperature = Comfortable;
            for (int i = 0; i < 10; i++) temperature = TemperatureLogic.Step(temperature, -1.5f, 1f, ref remainder);
            Assert.Equal(Comfortable - 15, temperature);
        }

        [Theory]
        [InlineData(-8, 1f, -8)]
        [InlineData(-8, 2f, -16)]
        [InlineData(-8, 0.5f, -4)] // A1: used to be 0, the multiplier was truncated to an integer
        [InlineData(-8, 1.5f, -12)] // A1: used to be -8
        [InlineData(30, 0.25f, 7)]
        public void ApplyMultiplier_ScalesIncrement(int increment, float multiplier, int expected)
        {
            float remainder = 0f;
            Assert.Equal(expected, TemperatureLogic.ApplyMultiplier(increment, multiplier, ref remainder));
        }

        [Theory]
        [InlineData(-3, 0.1f)]
        [InlineData(-3, 0.5f)]
        [InlineData(5, 0.3f)]
        [InlineData(-8, 1.7f)]
        public void ApplyMultiplier_SmallChangesAddUpOverTime(int increment, float multiplier)
        {
            float remainder = 0f;
            int total = 0;
            for (int i = 0; i < 100; i++) total += TemperatureLogic.ApplyMultiplier(increment, multiplier, ref remainder);

            int expected = (int)Math.Round(increment * multiplier * 100);
            Assert.InRange(total, expected - 1, expected + 1);
        }

        [Theory]
        [InlineData(1000, 1000)]
        [InlineData(1100, 1090)]
        [InlineData(1005, 1000)]
        [InlineData(900, 910)]
        [InlineData(995, 1000)]
        public void Step_WithoutIncrement_DriftsBackToComfortable(int temperature, int expected)
        {
            float remainder = 0.7f;
            Assert.Equal(expected, TemperatureLogic.Step(temperature, 0, 1f, ref remainder));
            Assert.Equal(0f, remainder);
        }

        [Theory]
        [InlineData(1100, -8, 1092)]
        [InlineData(5, -8, 0)]
        [InlineData(1990, 30, 2000)]
        public void Step_WithIncrement_AppliesAndClamps(int temperature, int increment, int expected)
        {
            float remainder = 0f;
            Assert.Equal(expected, TemperatureLogic.Step(temperature, increment, 1f, ref remainder));
        }

        [Fact]
        public void Step_WeakMultiplier_DoesNotNormalizeWhileEnvironmentIsActive()
        {
            float remainder = 0f;
            int temperature = TemperatureLogic.Step(900, -3, 0.1f, ref remainder); // -0.3 is not a whole degree yet

            Assert.Equal(900, temperature); // Must not drift up towards comfortable while it's cold
        }
    }
}
