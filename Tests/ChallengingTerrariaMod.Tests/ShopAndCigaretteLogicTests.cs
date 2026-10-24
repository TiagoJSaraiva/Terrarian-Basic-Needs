using System;
using System.Linq;
using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    public class ShopLogicTests
    {
        [Fact]
        public void PickDistinct_NeverRepeats()
        {
            var random = new Random(1234);
            int[] pool = { 1, 2, 3, 4, 5, 6 };

            for (int i = 0; i < 1000; i++)
            {
                var picked = ShopLogic.PickDistinct(pool, 2, random.Next);
                Assert.Equal(2, picked.Count);
                Assert.Equal(2, picked.Distinct().Count());
                Assert.All(picked, item => Assert.Contains(item, pool));
            }
        }

        [Fact]
        public void PickDistinct_EveryItemCanBePicked()
        {
            var random = new Random(42);
            int[] pool = { 1, 2, 3 };
            var seen = Enumerable.Range(0, 200).SelectMany(_ => ShopLogic.PickDistinct(pool, 2, random.Next)).ToHashSet();
            Assert.Equal(pool.ToHashSet(), seen);
        }

        [Fact]
        public void PickDistinct_CountLargerThanPool_ReturnsWholePool()
        {
            var picked = ShopLogic.PickDistinct(new[] { "a", "b" }, 5, _ => 0);
            Assert.Equal(new[] { "a", "b" }, picked);
        }

        [Fact]
        public void PickDistinct_DoesNotModifyPool()
        {
            int[] pool = { 1, 2, 3 };
            ShopLogic.PickDistinct(pool, 2, _ => 0);
            Assert.Equal(new[] { 1, 2, 3 }, pool);
        }
    }

    public class CigaretteLogicTests
    {
        [Fact]
        public void Smoke_CancerOnTheFiftiethCigarette_ThenCountStartsOver()
        {
            int smoked = 0;
            for (int i = 1; i < 50; i++)
            {
                Assert.False(CigaretteLogic.Smoke(ref smoked));
                Assert.Equal(i, smoked);
            }

            Assert.True(CigaretteLogic.Smoke(ref smoked));
            Assert.Equal(0, smoked);
        }
    }
}
