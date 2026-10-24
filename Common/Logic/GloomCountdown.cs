namespace ChallengingTerrariaMod.Common.Logic
{
    // What the player should experience in one second of gloom.
    public readonly struct GloomStep
    {
        public readonly bool ShowWarning; // "You feel that something terrible is about to happen"
        public readonly int Countdown; // Number to print in chat, 0 means nothing to print
        public readonly bool Kill;

        public GloomStep(bool showWarning, int countdown, bool kill)
        {
            ShowWarning = showWarning;
            Countdown = countdown;
            Kill = kill;
        }
    }

    // When sanity hits zero the player gets a few silent seconds, then a 5..1 countdown, then dies.
    public sealed class GloomCountdown
    {
        public const int TotalSeconds = 10;
        public const int SilentSeconds = 4;

        private int _counter = TotalSeconds;

        public bool Active { get; private set; }

        // Call once per second while sanity is at zero.
        public GloomStep Tick()
        {
            bool showWarning = !Active;
            Active = true;

            if (_counter-- > TotalSeconds - SilentSeconds)
            {
                return new GloomStep(showWarning, 0, false);
            }

            if (_counter == 0)
            {
                _counter = TotalSeconds;
                Active = false;
                return new GloomStep(showWarning, 0, true);
            }

            return new GloomStep(showWarning, _counter, false);
        }

        // Call when sanity rises above zero. Returns true if a countdown was interrupted.
        public bool Recover()
        {
            if (!Active) return false;
            Active = false;
            return true;
        }

        public void Reset()
        {
            _counter = TotalSeconds;
            Active = false;
        }
    }
}
