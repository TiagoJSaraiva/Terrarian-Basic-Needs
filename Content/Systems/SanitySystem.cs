using ChallengingTerrariaMod.Common.Logic;
using ChallengingTerrariaMod.Content.Systems.Players;
using Terraria;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Systems
{
    // Low sanity makes the world look darker for the local player.
    public class SanitySystem : ModSystem
    {
        public override void ModifyLightingBrightness(ref float scale)
        {
            Player player = Main.LocalPlayer;
            if (player == null || player.dead || player.ghost) return;

            float? sanityScale = SanityLogic.LightingScale(player.GetModPlayer<SanityPlayer>().CurrentSanity);
            if (sanityScale.HasValue) scale = sanityScale.Value;
        }
    }
}
