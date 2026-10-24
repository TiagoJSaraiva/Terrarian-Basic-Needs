using ChallengingTerrariaMod.Common;
using ChallengingTerrariaMod.Common.Logic;
using ChallengingTerrariaMod.Content.Buffs;
using ChallengingTerrariaMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ChallengingTerrariaMod.Content.Consumables
{
    public class Cigarette : ModItem
    {
        public override void SetDefaults()
        {
            Item.useStyle = ItemUseStyleID.DrinkLong;
            Item.useAnimation = 240;
            Item.useTime = 240;
            Item.shoot = ModContent.ProjectileType<CigaretteProjectile>(); // The cigarette drawn in the player's mouth
            Item.shootSpeed = 1f;
            Item.useTurn = true;
            Item.UseSound = new SoundStyle("ChallengingTerrariaMod/Assets/Audio/CigaretteSFX");
            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.buyPrice(silver: 50);
            Item.noUseGraphic = true;
        }

        public override void OnConsumeItem(Player player)
        {
            player.GetModPlayer<CigarettePlayer>().Smoke();
        }
    }

    public class CigarettePlayer : ModPlayer
    {
        // Counted per player; it used to live on the item stack and was lost whenever the stack ran out
        public int CigarettesSmoked;

        public void Smoke()
        {
            if (Player.whoAmI != Main.myPlayer) return;

            Player.AddBuff(ModContent.BuffType<ArmoredMind>(), CigaretteLogic.ArmoredMindDurationSeconds * 60);

            if (CigaretteLogic.Smoke(ref CigarettesSmoked))
            {
                Player.AddBuff(ModContent.BuffType<Cancer>(), CigaretteLogic.CancerDurationSeconds * 60);
                Main.NewText(Text.Get("Messages.Cancer"), Color.White);
            }
        }

        public override void ModifyNursePrice(NPC nurse, int health, bool removeDebuffs, ref int price)
        {
            if (Player.HasBuff(ModContent.BuffType<Cancer>()))
            {
                price += Item.buyPrice(platinum: 2);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CigarettesSmoked"] = CigarettesSmoked;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("CigarettesSmoked"))
            {
                CigarettesSmoked = tag.GetInt("CigarettesSmoked");
            }
        }
    }
}
