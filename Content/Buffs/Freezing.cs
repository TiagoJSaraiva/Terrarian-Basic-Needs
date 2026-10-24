using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Buffs
{
    public class Freezing : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.pickSpeed += 0.50f;
            player.tileSpeed *= 0.5f;
            player.wallSpeed *= 0.5f;

            // Random debuffs are rolled once per second, only on the owner's client (buffs are synced from there)
            if (player.whoAmI != Main.myPlayer || Main.GameUpdateCount % 60 != 0) return;

            if (Main.rand.NextFloat() < 0.05f && !player.HasBuff(BuffID.Frostburn))
            {
                player.AddBuff(BuffID.Frostburn, 7 * 60);
            }
            if (Main.rand.NextFloat() < 0.15f)
            {
                player.AddBuff(BuffID.Frozen, 120);
            }
        }
    }
}
