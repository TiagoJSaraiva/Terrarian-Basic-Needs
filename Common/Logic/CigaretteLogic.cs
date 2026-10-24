namespace ChallengingTerrariaMod.Common.Logic
{
    public static class CigaretteLogic
    {
        public const int CigarettesUntilCancer = 50;
        public const int ArmoredMindDurationSeconds = 240;
        public const int CancerDurationSeconds = 3000;

        // Counts one more cigarette. Returns true when the player gets cancer (the count then starts over).
        public static bool Smoke(ref int cigarettesSmoked)
        {
            cigarettesSmoked++;
            if (cigarettesSmoked < CigarettesUntilCancer) return false;

            cigarettesSmoked = 0;
            return true;
        }
    }
}
