using ChallengingTerrariaMod.Common;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Buffs
{
    public class Scorching : ModBuff
    {
        private const int DamagePerSecond = 15;

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

            // Damage and random debuffs happen once per second, only on the owner's client.
            // The hurt is not quiet so it's sent to the server and the other players.
            if (player.whoAmI != Main.myPlayer || Main.GameUpdateCount % 60 != 0) return;

            if (Main.rand.NextFloat() < 0.05f && !player.HasBuff(BuffID.OnFire))
            {
                player.AddBuff(BuffID.OnFire, 5 * 60);
            }

            var deathReason = PlayerDeathReason.ByCustomReason(Text.Network("DeathReasons.Scorching", player.name));
            player.Hurt(deathReason, DamagePerSecond, 0, pvp: false, quiet: false, cooldownCounter: -1, dodgeable: false);
        }
    }
}
