using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.ID;

namespace ChallengingTerrariaMod.Content.Buffs
{
    public class ThrowingUp : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed *= 0.20f;
            player.velocity.X = 0.30f;

            if (Main.rand.NextBool(5)) 
            {
                Dust.NewDust(player.position, player.width, player.height, DustID.Smoke, 0f, 0f, 100, new Color(0, 255, 0), 1.5f);
            }

            // 
            // if (player.whoAmI == Main.myPlayer && Main.rand.NextBool(60))
            // {
            //     SoundEngine.PlaySound(SoundID.NPCDeath1, player.position);
            // }
        }
    }
}