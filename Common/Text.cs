using Terraria.Localization;

namespace ChallengingTerrariaMod.Common
{
    // Shortcut for this mod's localization keys (see Localization/*.hjson).
    public static class Text
    {
        public const string Prefix = "Mods.ChallengingTerrariaMod.";

        public static string Get(string key) => Language.GetTextValue(Prefix + key);

        public static string Get(string key, params object[] args) => Language.GetTextValue(Prefix + key, args);

        // For text sent over the network (death reasons...): every client translates it into its own language.
        public static NetworkText Network(string key, params object[] args) => NetworkText.FromKey(Prefix + key, args);
    }
}
