using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Buffs
{
    // Given after too many cigarettes. Only the Nurse can cure it, for an extra price (see CigarettePlayer).
    public class Cancer : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = true;
            Main.persistentBuff[Type] = true; // Survives death
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense /= 2f;
            player.endurance -= 0.50f;
            player.pickSpeed += 0.90f;
            player.moveSpeed -= 0.95f;
            player.GetDamage(DamageClass.Generic) -= 0.50f;
        }
    }
}
