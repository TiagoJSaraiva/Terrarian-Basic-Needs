using System.ComponentModel;
using Newtonsoft.Json;
using Terraria.ModLoader.Config;

namespace ChallengingTerrariaMod.Content.ModConfigs
{
    // [JsonProperty] keeps the names already saved in players' config files, so renaming a field doesn't reset it.
    public class ModConfigServer : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        public float HungerMultiplier;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        public float RestMultiplier;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        [JsonProperty("sanityMultiplier")]
        public float SanityMultiplier;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        [JsonProperty("warmthMultiplier")]
        public float WarmthMultiplier;
    }

    public class ModConfigClient : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        // Offset in pixels applied to all meters
        [Range(0, 1200)]
        [DefaultValue(0)]
        [JsonProperty("localizationX")]
        public int OffsetX;

        [Range(0, 1200)]
        [DefaultValue(0)]
        [JsonProperty("localizationY")]
        public int OffsetY;
    }
}
