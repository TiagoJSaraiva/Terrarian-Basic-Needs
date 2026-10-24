using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ChallengingTerrariaMod.Common.Logic;
using Xunit;

namespace ChallengingTerrariaMod.Tests
{
    // The HUD builds keys like "UI.Hunger.<HungerStatus>", so a renamed enum value would show a raw key in game.
    public class LocalizationTests
    {
        public static IEnumerable<object[]> Languages => new[] { new object[] { "en-US" }, new object[] { "pt-BR" } };

        [Fact]
        public void AllLanguages_HaveTheSameKeys()
        {
            var english = ReadKeys("en-US");
            var portuguese = ReadKeys("pt-BR");

            Assert.Empty(english.Except(portuguese));
            Assert.Empty(portuguese.Except(english));
        }

        [Theory]
        [MemberData(nameof(Languages))]
        public void EveryMeterStatus_HasATooltip(string language)
        {
            var keys = ReadKeys(language);

            AssertHasKeys(keys, "UI.Hunger", Enum.GetNames<HungerStatus>());
            AssertHasKeys(keys, "UI.Rest", Enum.GetNames<RestStatus>().Where(s => s != nameof(RestStatus.Fainted))); // Shown as Exhausted
            AssertHasKeys(keys, "UI.Sanity", Enum.GetNames<SanityStatus>().Where(s => s != nameof(SanityStatus.Gloom))); // Shown as Terrified
            AssertHasKeys(keys, "UI.Warmth", Enum.GetNames<TemperatureStatus>());
        }

        private static void AssertHasKeys(HashSet<string> keys, string prefix, IEnumerable<string> names)
        {
            foreach (string name in names.Append("Title"))
            {
                Assert.Contains($"{prefix}.{name}", keys);
            }
        }

        // Minimal reader for the hjson layout used by tModLoader: "Key: value" lines inside "Group: {" blocks.
        private static HashSet<string> ReadKeys(string language)
        {
            string path = Path.Combine(FindModRoot(), "Localization", $"{language}_Mods.ChallengingTerrariaMod.hjson");
            var keys = new HashSet<string>();
            var groups = new Stack<string>();
            bool inMultiline = false;

            foreach (string rawLine in File.ReadLines(path))
            {
                string line = rawLine.Trim();
                if (line == "'''")
                {
                    inMultiline = !inMultiline;
                    continue;
                }
                if (inMultiline) continue;

                if (line == "}")
                {
                    groups.Pop();
                    continue;
                }

                Match match = Regex.Match(line, @"^([\w.]+):\s*(\{)?");
                if (!match.Success) continue;

                if (match.Groups[2].Success) groups.Push(match.Groups[1].Value);
                else keys.Add(string.Join(".", groups.Reverse().Append(match.Groups[1].Value)));
            }

            Assert.Empty(groups);
            return keys;
        }

        private static string FindModRoot()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null && !File.Exists(Path.Combine(directory.FullName, "build.txt")))
            {
                directory = directory.Parent;
            }
            return directory?.FullName ?? throw new DirectoryNotFoundException("Mod folder (with build.txt) not found");
        }
    }
}
