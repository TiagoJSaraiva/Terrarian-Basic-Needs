using System.Collections.Generic;
using ChallengingTerrariaMod.Common;
using ChallengingTerrariaMod.Common.Logic;
using ChallengingTerrariaMod.Content.Buffs;
using ChallengingTerrariaMod.Content.Consumables;
using ChallengingTerrariaMod.Content.Systems.Players;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChallengingTerrariaMod.Content.GlobalChanges
{
    public class GlobalItemChanges : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            if (item.type == ItemID.CoffeeCup)
            {
                item.buffType = ModContent.BuffType<Cafeinated>();
                item.buffTime = 240 * 60;
                item.rare = ItemRarityID.Green;
                item.consumable = true;
                item.value = Item.buyPrice(silver: 50);
            }
        }

        public override void OnConsumeItem(Item item, Player player)
        {
            player.GetModPlayer<HungerPlayer>().OnFoodEaten(item);
        }

        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            switch (item.type)
            {
                case ItemID.CoffeeCup:
                    // Replace vanilla's "Minor improvements to all stats" (the Well Fed buff is gone)
                    tooltips.RemoveAll(t => t.Name == "Tooltip0");
                    TooltipLine buffTooltip = tooltips.Find(t => t.Name == "Tooltip1");
                    if (buffTooltip != null) buffTooltip.Text = Text.Get("Tooltips.CoffeeCup");
                    else tooltips.Add(new TooltipLine(Mod, "CoffeeEffect", Text.Get("Tooltips.CoffeeCup")));
                    tooltips.Add(new TooltipLine(Mod, "CustomDescription", Text.Get("Tooltips.CoffeeCupFlavor")));
                    break;

                case ItemID.WarmthPotion:
                    tooltips.Add(new TooltipLine(Mod, "CustomDescription", Text.Get("Tooltips.WarmthPotion")));
                    break;

                case ItemID.ObsidianSkinPotion:
                    tooltips.Add(new TooltipLine(Mod, "CustomDescription", Text.Get("Tooltips.ObsidianSkinPotion")));
                    break;
            }
        }
    }

    public class GlobalNPC_Changes : GlobalNPC
    {
        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            switch (npc.type)
            {
                case NPCID.SkeletonCommando:
                    AddDrop<Cigarette>(npcLoot, 30);
                    AddDrop<AdrenalineInjection>(npcLoot, 30);
                    break;
                case NPCID.UndeadViking:
                case NPCID.SnowFlinx:
                    AddDrop<Cigarette>(npcLoot, 50);
                    break;
                case NPCID.ArmoredViking:
                    AddDrop<Cigarette>(npcLoot, 50);
                    AddDrop<AdrenalineInjection>(npcLoot, 50);
                    break;
                case NPCID.Necromancer:
                    AddDrop<Vitamin>(npcLoot, 30);
                    AddDrop<FoodPill>(npcLoot, 30);
                    break;
                case NPCID.DesertGhoul:
                    AddDrop<Vitamin>(npcLoot, 50);
                    AddDrop<FoodPill>(npcLoot, 50);
                    break;
                case NPCID.WalkingAntlion:
                case NPCID.FlyingAntlion:
                    AddDrop<Vitamin>(npcLoot, 50);
                    break;
                case NPCID.AngryTrapper:
                    AddDrop<CaffeinePill>(npcLoot, 50);
                    break;
                case NPCID.BoneLee:
                    AddDrop<CaffeinePill>(npcLoot, 30);
                    break;
            }
        }

        // 1 in chanceDenominator chance to drop one
        private static void AddDrop<T>(NPCLoot npcLoot, int chanceDenominator) where T : ModItem
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<T>(), chanceDenominator));
        }

        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.ArmsDealer)
            {
                shop.Add(new Item(ModContent.ItemType<Cigarette>())
                {
                    shopCustomPrice = Item.buyPrice(gold: 3, silver: 50)
                });
            }
        }

        // The Travelling Merchant sells 2 different items from the pool; Hardmode unlocks more of them.
        public override void SetupTravelShop(int[] shop, ref int nextSlot)
        {
            var pool = new List<int> { ModContent.ItemType<Cigarette>(), ModContent.ItemType<Vitamin>(), ItemID.CoffeeCup };
            if (Main.hardMode)
            {
                pool.Add(ModContent.ItemType<CaffeinePill>());
                pool.Add(ModContent.ItemType<FoodPill>());
                pool.Add(ModContent.ItemType<AdrenalineInjection>());
            }

            foreach (int itemType in ShopLogic.PickDistinct(pool, 2, Main.rand.Next))
            {
                if (nextSlot >= shop.Length) break; // Other mods may have filled the shop already
                shop[nextSlot++] = itemType;
            }
        }
    }
}
