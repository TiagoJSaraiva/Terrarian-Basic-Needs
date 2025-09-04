using Terraria;
using Terraria.GameContent.UI.Elements; 
using Terraria.UI; 
using Microsoft.Xna.Framework; 
using Microsoft.Xna.Framework.Graphics; 
using Terraria.ModLoader;
using System;
using ReLogic.Content;
using ChallengingTerrariaMod.Content.Systems.Players;
using ChallengingTerrariaMod.Content.ModConfigs;

namespace ChallengingTerrariaMod.Content.Systems.UI
{
    public class HungerBarUI : UIState
    {
        private UIElement area;
        private UIImage hungerMeterImage;

        private Asset<Texture2D>[] hungerFillTextures;
        private const int TotalSprites = 15; 

        private int localizationX = 850;
        private int localizationY = 20;

        public override void OnInitialize()
        {
            ModConfigClient.setLocalization(ref localizationX, ref localizationY);

            area = new UIElement();
            area.Left.Set(localizationX, 0f);
            area.Top.Set(localizationY, 0f);
            area.Width.Set(30, 0f);
            area.Height.Set(50, 0f);
            Append(area);

            hungerFillTextures = new Asset<Texture2D>[TotalSprites];
            for (int i = 0; i < TotalSprites; i++)
            {
                string texturePath = $"ChallengingTerrariaMod/Content/Systems/UI/Images/HungerMeter/HungerMeter_{i}";
                hungerFillTextures[i] = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad);
            }

            hungerMeterImage = new UIImage(hungerFillTextures[GetSpriteIndex(HungerSystem.MaxHungerNormal)]);
            hungerMeterImage.Left.Set(0, 0f);
            hungerMeterImage.Top.Set(0, 0f);
            hungerMeterImage.Width.Set(30, 0f);
            hungerMeterImage.Height.Set(50, 0f);
            area.Append(hungerMeterImage);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            HungerPlayer hungerPlayer = Main.LocalPlayer.GetModPlayer<HungerPlayer>();

            if (Main.LocalPlayer != null && Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost)
            {
                int newSpriteIndex = GetSpriteIndex(hungerPlayer.CurrentHunger);

                hungerMeterImage.SetImage(hungerFillTextures[newSpriteIndex]);
            }

            if (area.IsMouseHovering)
            {
                if (hungerPlayer.CurrentHunger >= HungerSystem.MaxHungerDebuffThreshold_Bloated)
                {
                    Main.instance.MouseText("Hunger Meter\nYou're bloated\nIf you eat more, you might throw up!");
                }
                else if (hungerPlayer.CurrentHunger >= HungerSystem.MaxHungerDebuffThreshold_Full)
                {
                    Main.instance.MouseText("Hunger Meter\nYou're full\nIf you eat more, you might throw up!");
                }
                else if (hungerPlayer.CurrentHunger <= HungerSystem.HungerDebuffThreshold_Starved)
                {
                    Main.instance.MouseText("Hunger Meter\nYou're starving\nEat something!\nThe better the quality of the food, the more it satisfies you.");
                }
                else if (hungerPlayer.CurrentHunger <= HungerSystem.HungerDebuffThreshold_Famished)
                {
                    Main.instance.MouseText("Hunger Meter\nYou're famished\nEat something!\nThe better the quality of the food, the more it satisfies you.");
                }
                else if (hungerPlayer.CurrentHunger <= HungerSystem.HungerDebuffThreshold_Hungry)
                {
                    Main.instance.MouseText("Hunger Meter\nYou're hungry\nEat something!\nThe better the quality of the food, the more it satisfies you.");
                }
                else if (hungerPlayer.CurrentHunger <= HungerSystem.HungerDebuffThreshold_Peckish)
                {
                    Main.instance.MouseText("Hunger Meter\nYou're peckish\nEat something!\nThe better the quality of the food, the more it satisfies you.");
                }
                else
                {
                    Main.instance.MouseText("Hunger Meter\nYou're well fed");
                }
            }
            base.Update(gameTime);

            int X = 850;
            int Y = 20;
            ModConfigClient.setLocalization(ref X, ref Y);

            area.Left.Set(X, 0f);
            area.Top.Set(Y, 0f);
        }
        private int GetSpriteIndex(float hungerValue)
        {
            const int NormalHungerSpriteIndex = 11; 
            const float BaseHungerValue = HungerSystem.MaxHungerNormal; 

            int calculatedSpriteIndex;

            if (hungerValue > BaseHungerValue)
            {
                float hungerAboveNormal = hungerValue - BaseHungerValue;
                int spritesIncrement = (int)Math.Floor(hungerAboveNormal / 100f);

                calculatedSpriteIndex = NormalHungerSpriteIndex + spritesIncrement;
            }
            else 
            {
                float hungerBelowNormal = BaseHungerValue - hungerValue;
                int spritesDecrement;
                
                spritesDecrement = (int)Math.Floor(hungerBelowNormal / 100f);
            
                calculatedSpriteIndex = NormalHungerSpriteIndex - spritesDecrement;
            }

            return Utils.Clamp(calculatedSpriteIndex, 0, TotalSprites - 1);
        }
    }
}