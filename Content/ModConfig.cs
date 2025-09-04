using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using System.ComponentModel;

namespace ChallengingTerrariaMod.Content.ModConfigs
{
    public class ModConfigServer : ModConfig // Global config
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
        public float sanityMultiplier;

        [Range(0.1f, 5f)]
        [DefaultValue(1f)]
        public float warmthMultiplier;

    }

    public class ModConfigClient : ModConfig // Local config
    {
        public static void setLocalization(ref int x, ref int y)
        {
            x += ModContent.GetInstance<ModConfigClient>().localizationX;
            y += ModContent.GetInstance<ModConfigClient>().localizationY;
        }
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Range(0, 1200)]
        [DefaultValue(0)]
        public int localizationX;
        
        [Range(0, 1200)]
        [DefaultValue(0)]
        public int localizationY;
    }
}
