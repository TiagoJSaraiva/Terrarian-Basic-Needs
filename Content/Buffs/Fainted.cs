using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Buffs
{
    // Item use is blocked in RestPlayer.CanUseItem and the "Zzz" effect is spawned by RestPlayer
    public class Fainted : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.controlRight = false;
            player.controlLeft = false;
            player.controlDown = false;
            player.controlJump = false;
            player.controlHook = false;
            player.controlMount = false;
            player.controlTorch = false;
            player.immuneAlpha = 110;
        }
    }
}
