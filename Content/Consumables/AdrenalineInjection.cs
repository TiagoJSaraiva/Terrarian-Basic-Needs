using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ChallengingTerrariaMod.Content.Buffs;

namespace ChallengingTerrariaMod.Content.Consumables
{
    public class AdrenalineInjection : ModItem
    {
        public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;
		}

        public override void SetDefaults()
        {
            Item.value = Item.buyPrice(silver: 50);
            Item.useAnimation = 20;
            Item.useTime = 20;
            Item.UseSound = null;
            Item.rare = ItemRarityID.LightRed;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.buffType = ModContent.BuffType<ArmoredMind>();
            Item.consumable = true;
            Item.buffTime = 60 * 480;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.buyPrice(gold: 1);
        }
    }
}