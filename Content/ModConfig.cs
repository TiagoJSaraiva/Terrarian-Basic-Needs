using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace ChallengingTerrariaMod.Content.ModConfigs
{
    public class MeuModConfig : ModConfig
    {
        // Indica que a configuração é global (mesmo para todos os jogadores)
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        public float HungerMultiplier;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        public float RestMultiplier;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        public float sanityMultiplier;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        public float warmthMultiplier;

    }
}
