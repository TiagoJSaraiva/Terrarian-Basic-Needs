using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    public class SanityLogicTests
    {
        private static SanityContext Context() => new SanityContext { Multiplier = 1f };

        [Theory]
        [InlineData(1200, SanityStatus.Well)]
        [InlineData(900, SanityStatus.Stressed)]
        [InlineData(600, SanityStatus.Scared)]
        [InlineData(300, SanityStatus.Terrified)]
        [InlineData(0.1f, SanityStatus.Terrified)]
        [InlineData(0, SanityStatus.Gloom)]
        public void GetStatus_UsesInclusiveThresholds(float sanity, SanityStatus expected)
        {
            Assert.Equal(expected, SanityLogic.GetStatus(sanity));
        }

        [Fact]
        public void Delta_NothingHappening_NoChange()
        {
            Assert.Equal(0f, SanityLogic.Delta(Context()));
        }

        [Fact]
        public void Delta_EverythingBad_SumsAllLosses()
        {
            var context = Context();
            context.InHostileZone = true;
            context.LowHealth = true;
            context.EclipseOrBloodMoon = true;
            Assert.Equal(-(4f + 9f + 7f), SanityLogic.Delta(context), 3);
        }

        [Fact]
        public void Delta_ArmoredMind_ReducesLosses()
        {
            var context = Context();
            context.ArmoredMind = true;
            context.InHostileZone = true;
            context.LowHealth = true;
            context.EclipseOrBloodMoon = true;
            Assert.Equal(-(2.5f + 6f + 3.5f), SanityLogic.Delta(context), 3);
        }

        [Fact]
        public void Delta_LossesScaleWithMultiplier_GainsDoNot()
        {
            var context = Context();
            context.Multiplier = 2f;
            context.InHostileZone = true;
            context.InOverworld = true;
            context.TownNPCs = 3;
            Assert.Equal(-8f + 2f + 9f, SanityLogic.Delta(context), 3);
        }

        [Theory]
        [InlineData(2, 0f)]
        [InlineData(3, 9f)]
        public void Delta_TownNeedsAtLeastThreeNPCs(int townNPCs, float expected)
        {
            var context = Context();
            context.TownNPCs = townNPCs;
            Assert.Equal(expected, SanityLogic.Delta(context));
        }

        [Theory]
        [InlineData(50, 100, true)]
        [InlineData(51, 100, false)]
        [InlineData(50, 101, true)] // integer division: 101 / 2 = 50
        public void IsLowHealth_AtOrBelowHalf(int life, int maxLife, bool expected)
        {
            Assert.Equal(expected, SanityLogic.IsLowHealth(life, maxLife));
        }

        [Theory]
        [InlineData(0, 0.75f)]
        [InlineData(300, 0.85f)]
        [InlineData(600, 0.90f)]
        public void LightingScale_DarkensAtLowSanity(float sanity, float expected)
        {
            Assert.Equal(expected, SanityLogic.LightingScale(sanity));
        }

        [Fact]
        public void LightingScale_UnchangedAboveScared()
        {
            Assert.Null(SanityLogic.LightingScale(600.1f));
        }
    }
}
