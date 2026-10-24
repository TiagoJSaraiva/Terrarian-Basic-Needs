using ChallengingTerrariaMod.Content.Systems.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Buffs
{
    // Given by SanityPlayer.OnHurt while Terrified: the player runs away from what hit them.
    public class Flee : ModBuff
    {
        private const float FleeSpeed = 5f;

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // Movement is owned by the player's client; other clients receive the position
            if (player.whoAmI != Main.myPlayer) return;

            int direction = player.GetModPlayer<SanityPlayer>().FleeDirection;
            if (direction != 0)
            {
                player.velocity.X = FleeSpeed * direction;
                player.direction = direction;
            }
            player.controlLeft = false;
            player.controlRight = false;
        }
    }
}
