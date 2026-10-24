using System.Collections.Generic;
using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    public class GloomCountdownTests
    {
        private static List<GloomStep> TickUntilKill(GloomCountdown gloom)
        {
            var steps = new List<GloomStep>();
            for (int i = 0; i < 20; i++)
            {
                GloomStep step = gloom.Tick();
                steps.Add(step);
                if (step.Kill) break;
            }
            return steps;
        }

        [Fact]
        public void Tick_FullSequence_WarnsThenSilentThenCountsDownThenKills()
        {
            var steps = TickUntilKill(new GloomCountdown());

            Assert.Equal(10, steps.Count);
            Assert.True(steps[0].ShowWarning);
            Assert.All(steps.GetRange(1, 9), s => Assert.False(s.ShowWarning));
            Assert.All(steps.GetRange(0, 4), s => Assert.Equal(0, s.Countdown));
            Assert.Equal(new[] { 5, 4, 3, 2, 1 }, steps.GetRange(4, 5).ConvertAll(s => s.Countdown));
            Assert.True(steps[9].Kill);
        }

        [Fact]
        public void Tick_AfterKill_StartsOverFromTheBeginning()
        {
            var gloom = new GloomCountdown();
            TickUntilKill(gloom);

            Assert.False(gloom.Active);
            Assert.Equal(10, TickUntilKill(gloom).Count);
        }

        [Fact]
        public void Recover_ReturnsTrueOnlyWhenCountdownWasRunning()
        {
            var gloom = new GloomCountdown();
            Assert.False(gloom.Recover());

            gloom.Tick();
            Assert.True(gloom.Recover());
            Assert.False(gloom.Active);
        }

        // A10: the next gloom used to continue the old countdown ("2...") instead of starting over
        [Fact]
        public void Recover_ThenGloomAgain_StartsOver()
        {
            var gloom = new GloomCountdown();
            for (int i = 0; i < 7; i++) gloom.Tick(); // Countdown already printed 5, 4, 3
            gloom.Recover();

            var steps = TickUntilKill(gloom);

            Assert.True(steps[0].ShowWarning);
            Assert.Equal(0, steps[0].Countdown);
            Assert.Equal(10, steps.Count);
        }

        [Fact]
        public void Reset_StartsOverFromTheBeginning()
        {
            var gloom = new GloomCountdown();
            for (int i = 0; i < 7; i++) gloom.Tick();
            gloom.Reset();

            Assert.False(gloom.Active);
            Assert.Equal(10, TickUntilKill(gloom).Count);
        }
    }
}
