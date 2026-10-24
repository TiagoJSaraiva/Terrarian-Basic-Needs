using System;
using System.Collections.Generic;

namespace ChallengingTerrariaMod.Common.Logic
{
    public static class ShopLogic
    {
        // Picks count different entries from pool. nextInt(n) must return a random number in [0, n).
        public static List<T> PickDistinct<T>(IReadOnlyList<T> pool, int count, Func<int, int> nextInt)
        {
            var remaining = new List<T>(pool);
            var picked = new List<T>(Math.Min(count, remaining.Count));

            while (picked.Count < count && remaining.Count > 0)
            {
                int index = nextInt(remaining.Count);
                picked.Add(remaining[index]);
                remaining.RemoveAt(index);
            }

            return picked;
        }
    }
}
