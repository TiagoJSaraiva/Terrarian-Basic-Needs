using ChallengingTerrariaMod.Common.Logic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Buffs
{
    // The life regeneration penalty and the random confusion are applied in RestPlayer
    public class Exhausted : ModBuff
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
            player.manaRegenBonus -= 50;
            player.statLifeMax2 = RestLogic.RoundDownToStep(player.statLifeMax2, 1.6f);
            player.statManaMax2 = RestLogic.RoundDownToStep(player.statManaMax2, 1.6f);
        }
    }
}
