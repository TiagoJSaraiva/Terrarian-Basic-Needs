using ChallengingTerrariaMod.Common.Logic;
using ChallengingTerrariaMod.Content.Buffs;
using ChallengingTerrariaMod.Content.ModConfigs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ChallengingTerrariaMod.Content.Systems.Players
{
    public class WarmthPlayer : ModPlayer
    {
        private const int DebuffDurationTicks = 60; // Renewed every second while the condition lasts
        private const float HeavyRainCloudAlpha = 0.7f;

        private static readonly int[] HeatSourceTiles =
        {
            TileID.Furnaces, TileID.Hellforge, TileID.AdamantiteForge, TileID.Fireplace,
            TileID.GlassKiln, TileID.LihzahrdFurnace, TileID.Campfire,
        };

        public int CurrentTemperature;
        public int LastTemperatureChange; // Change in the last update, shown as an arrow on the meter

        private float _multiplierRemainder;

        public override void Initialize()
        {
            CurrentTemperature = TemperatureLogic.Comfortable;
            LastTemperatureChange = 0;
            _multiplierRemainder = 0f;
        }

        // The owner's client simulates temperature; buffs are synced to other players by vanilla.
        private bool IsSimulated => Player.whoAmI == Main.myPlayer && !Player.dead && !Player.ghost;

        public override void PostUpdate()
        {
            if (!IsSimulated || Main.GameUpdateCount % TemperatureLogic.UpdateRate != 0) return;

            var context = new TemperatureContext
            {
                InWater = Player.wet && !Player.lavaWet && !Player.honeyWet,
                IsDayTime = Main.dayTime,
                InOverworld = Player.ZoneOverworldHeight,
                InSnow = Player.ZoneSnow,
                Raining = Main.raining,
                HeavyRain = Main.cloudAlpha > HeavyRainCloudAlpha,
                NearHeatSource = IsNearHeatSource(),
                InBeach = Player.ZoneBeach,
                InDesert = Player.ZoneDesert,
                InUnderworld = Player.ZoneUnderworldHeight,
                InSky = Player.ZoneSkyHeight,
                HasWarmthPotion = Player.HasBuff(BuffID.Warmth),
                HasObsidianSkin = Player.HasBuff(BuffID.ObsidianSkin),
            };

            float multiplier = ModContent.GetInstance<ModConfigServer>().WarmthMultiplier;
            int increment = TemperatureLogic.EnvironmentIncrement(context, CurrentTemperature);
            int newTemperature = TemperatureLogic.Step(CurrentTemperature, increment, multiplier, ref _multiplierRemainder);

            LastTemperatureChange = newTemperature - CurrentTemperature;
            CurrentTemperature = newTemperature;

            ApplyWarmthDebuffs();
        }

        private bool IsNearHeatSource()
        {
            Point center = Player.Center.ToTileCoordinates();
            int radius = TemperatureLogic.DetectionRadiusTiles;

            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                for (int y = center.Y - radius; y <= center.Y + radius; y++)
                {
                    if (!WorldGen.InWorld(x, y)) continue;

                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && System.Array.IndexOf(HeatSourceTiles, (int)tile.TileType) >= 0) return true;
                }
            }
            return false;
        }

        private void ApplyWarmthDebuffs()
        {
            int? current = TemperatureLogic.GetStatus(CurrentTemperature) switch
            {
                TemperatureStatus.Cold => ModContent.BuffType<Cold>(),
                TemperatureStatus.VeryCold => ModContent.BuffType<VeryCold>(),
                TemperatureStatus.Freezing => ModContent.BuffType<Freezing>(),
                TemperatureStatus.Warm => ModContent.BuffType<Warm>(),
                TemperatureStatus.Hot => ModContent.BuffType<Hot>(),
                TemperatureStatus.Scorching => ModContent.BuffType<Scorching>(),
                _ => null,
            };

            foreach (int debuff in new[]
            {
                ModContent.BuffType<Cold>(), ModContent.BuffType<VeryCold>(), ModContent.BuffType<Freezing>(),
                ModContent.BuffType<Warm>(), ModContent.BuffType<Hot>(), ModContent.BuffType<Scorching>(),
            })
            {
                if (debuff == current) Player.AddBuff(debuff, DebuffDurationTicks);
                else Player.ClearBuff(debuff);
            }
        }

        public override void OnRespawn()
        {
            CurrentTemperature = TemperatureLogic.Comfortable;
            LastTemperatureChange = 0;
            _multiplierRemainder = 0f;
        }

        // Old saves also contain "TemperatureBeforeDeath" and "LastTemperatureChange"; they were never used and are ignored.
        public override void SaveData(TagCompound tag)
        {
            tag["CurrentTemperature"] = CurrentTemperature;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("CurrentTemperature"))
            {
                CurrentTemperature = System.Math.Clamp(tag.GetInt("CurrentTemperature"), TemperatureLogic.Min, TemperatureLogic.Max);
            }
        }
    }
}
