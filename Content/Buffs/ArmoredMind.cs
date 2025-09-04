using Terraria;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.Buffs
{
    public class ArmoredMind : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}