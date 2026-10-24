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
    public class HungerPlayer : ModPlayer
    {
        private const int DebuffDurationTicks = 61; // Renewed every second while the condition lasts
        private const int NauseousDurationTicks = 60 * 60;
        private const int ThrowingUpDurationTicks = 2 * 60;
        private const int ShortFoodBuffTicks = 3 * 60;
        private const int StarvedLifeRegen = -20; // Half life points per second, so 10 HP/s

        public float CurrentHunger;
        private bool _shortenFoodBuffs;

        public override void Initialize()
        {
            CurrentHunger = HungerLogic.MaxNormal;
        }

        // The owner's client simulates hunger; buffs are synced to other players by vanilla.
        private bool IsSimulated => Player.whoAmI == Main.myPlayer && !Player.dead && !Player.ghost;

        public override void PreUpdate()
        {
            if (!IsSimulated) return;

            if (HungerLogic.ShouldVomit(CurrentHunger))
            {
                Vomit();
            }

            if (Main.GameUpdateCount % HungerLogic.TickRate == 0)
            {
                float multiplier = ModContent.GetInstance<ModConfigServer>().HungerMultiplier;
                bool isMoving = Player.velocity != Vector2.Zero;
                bool nourished = Player.HasBuff(ModContent.BuffType<Nourished>());

                CurrentHunger = HungerLogic.Clamp(CurrentHunger - HungerLogic.Drain(isMoving, nourished, multiplier));
                ApplyHungerDebuffs();
            }
        }

        private void Vomit()
        {
            CurrentHunger = HungerLogic.HungerAfterVomiting;
            Player.AddBuff(ModContent.BuffType<Nauseous>(), NauseousDurationTicks);
            Player.AddBuff(ModContent.BuffType<ThrowingUp>(), ThrowingUpDurationTicks);

            Player.ClearBuff(BuffID.WellFed);
            Player.ClearBuff(BuffID.WellFed2);
            Player.ClearBuff(BuffID.WellFed3);

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.Smoke,
                             Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                             100, new Color(0, 255, 0, 100), Main.rand.NextFloat(1.5f, 2.5f));
            }
        }

        public override void PostUpdateRunSpeeds()
        {
            if (Player.HasBuff(ModContent.BuffType<Bloated>()))
            {
                Player.maxRunSpeed *= 0.65f;
                Player.accRunSpeed *= 0.65f;
            }
            else if (Player.HasBuff(ModContent.BuffType<Full>()))
            {
                Player.maxRunSpeed *= 0.85f;
                Player.accRunSpeed *= 0.85f;
            }
        }

        public override void UpdateBadLifeRegen()
        {
            if (!Player.HasBuff(ModContent.BuffType<Starved>())) return;

            if (Player.lifeRegen > 0) Player.lifeRegen = 0;
            Player.lifeRegenTime = 0;
            Player.lifeRegen += StarvedLifeRegen;
        }

        public override void OnRespawn()
        {
            CurrentHunger = HungerLogic.OnRespawn(CurrentHunger);
        }

        public override bool CanUseItem(Item item)
        {
            bool cannotEat = Player.HasBuff(ModContent.BuffType<Nauseous>()) || Player.HasBuff(ModContent.BuffType<Fainted>());
            if (cannotEat && item.consumable && TierOf(item.buffType) != WellFedTier.None)
            {
                return false;
            }
            return base.CanUseItem(item);
        }

        // Called by GlobalItemChanges.OnConsumeItem when this player eats something.
        public void OnFoodEaten(Item item)
        {
            if (Player.whoAmI != Main.myPlayer) return;

            int restored = HungerLogic.RestoreAmount(TierOf(item.buffType));
            if (restored == 0) return;

            CurrentHunger = HungerLogic.Clamp(CurrentHunger + restored);
            _shortenFoodBuffs = HungerLogic.ShouldShortenFoodBuff(CurrentHunger);
        }

        public override void PostUpdateBuffs()
        {
            // Done on the next update so the food buff has already been applied
            if (!_shortenFoodBuffs) return;
            _shortenFoodBuffs = false;

            for (int i = 0; i < Player.buffType.Length; i++)
            {
                if (TierOf(Player.buffType[i]) != WellFedTier.None)
                {
                    Player.buffTime[i] = ShortFoodBuffTicks;
                }
            }
        }

        private static WellFedTier TierOf(int buffType) => buffType switch
        {
            BuffID.WellFed3 => WellFedTier.ExquisitelyStuffed,
            BuffID.WellFed2 => WellFedTier.PlentySatisfied,
            BuffID.WellFed => WellFedTier.WellFed,
            _ => WellFedTier.None,
        };

        private void ApplyHungerDebuffs()
        {
            int? current = HungerLogic.GetStatus(CurrentHunger) switch
            {
                HungerStatus.Bloated => ModContent.BuffType<Bloated>(),
                HungerStatus.Full => ModContent.BuffType<Full>(),
                HungerStatus.Peckish => ModContent.BuffType<Peckish>(),
                HungerStatus.Hungry => ModContent.BuffType<Hungry>(),
                HungerStatus.Famished => ModContent.BuffType<Famished>(),
                HungerStatus.Starved => ModContent.BuffType<Starved>(),
                _ => null,
            };

            foreach (int debuff in new[]
            {
                ModContent.BuffType<Bloated>(), ModContent.BuffType<Full>(), ModContent.BuffType<Peckish>(),
                ModContent.BuffType<Hungry>(), ModContent.BuffType<Famished>(), ModContent.BuffType<Starved>(),
            })
            {
                if (debuff == current) Player.AddBuff(debuff, DebuffDurationTicks);
                else Player.ClearBuff(debuff);
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CurrentHunger"] = CurrentHunger;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("CurrentHunger"))
            {
                CurrentHunger = HungerLogic.Clamp(tag.GetFloat("CurrentHunger"));
            }
        }
    }
}
