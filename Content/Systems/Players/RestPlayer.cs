using ChallengingTerrariaMod.Common;
using ChallengingTerrariaMod.Common.Logic;
using ChallengingTerrariaMod.Content.Buffs;
using ChallengingTerrariaMod.Content.ModConfigs;
using ChallengingTerrariaMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ChallengingTerrariaMod.Content.Systems.Players
{
    public class RestPlayer : ModPlayer
    {
        private const int DebuffDurationTicks = 61; // Renewed every second while the condition lasts
        private const int BadRestDurationTicks = 60;
        private const int ConfusedDurationTicks = 2 * 60;
        private const float ExhaustedConfusionChance = 0.05f;

        public float CurrentRest;
        public int timeNoSleep; // Seconds awake; reaching SleepDeprivationLimit gives Sleep Deprived

        private bool _faintedLastUpdate;

        public override void Initialize()
        {
            CurrentRest = RestLogic.Max;
            timeNoSleep = 0;
        }

        // The owner's client simulates rest; buffs are synced to other players by vanilla.
        private bool IsSimulated => Player.whoAmI == Main.myPlayer && !Player.dead && !Player.ghost;

        private bool IsFainted => Player.HasBuff(ModContent.BuffType<Fainted>());

        public override void PostUpdate()
        {
            if (!IsSimulated) return;

            if (IsFainted) EnsureFaintedVFX();

            if (Main.GameUpdateCount % RestLogic.UpdateRate != 0) return;

            if (!IsFainted)
            {
                timeNoSleep++;
                ApplyRestDebuffs();
            }
            UpdateRest();
        }

        private void UpdateRest()
        {
            var context = new RestContext
            {
                IsSleeping = Player.sleeping.isSleeping,
                IsSitting = Player.sitting.isSitting,
                IsDayTime = Main.dayTime,
                TimeAccelerated = Main.dayRate > 1,
                SleepDeprived = Player.HasBuff(ModContent.BuffType<SleepDeprived>()),
                Caffeinated = Player.HasBuff(ModContent.BuffType<Cafeinated>()),
                TownNPCs = Player.townNPCs,
                Multiplier = ModContent.GetInstance<ModConfigServer>().RestMultiplier,
            };

            CurrentRest = RestLogic.Clamp(CurrentRest + RestLogic.Delta(context, out bool badRest));

            if (badRest) Player.AddBuff(ModContent.BuffType<BadRest>(), BadRestDurationTicks);
            if (context.IsSleeping) timeNoSleep = RestLogic.TimeNoSleepWhileSleeping(timeNoSleep);
        }

        private void ApplyRestDebuffs()
        {
            if (timeNoSleep >= RestLogic.SleepDeprivationLimit)
            {
                Main.NewText(Text.Get("Messages.SleepDeprived"), Color.LightBlue);
                Player.AddBuff(ModContent.BuffType<SleepDeprived>(), RestLogic.SleepDeprivedDurationSeconds * 60);
                timeNoSleep = 0;
            }

            if (Player.HasBuff(ModContent.BuffType<Exhausted>()) && !Player.HasBuff(BuffID.Confused) && Main.rand.NextFloat() < ExhaustedConfusionChance)
            {
                Player.AddBuff(BuffID.Confused, ConfusedDurationTicks);
            }

            RestStatus status = RestLogic.GetStatus(CurrentRest);
            int? current = status switch
            {
                RestStatus.Tired => ModContent.BuffType<Tired>(),
                RestStatus.Sleepy => ModContent.BuffType<Sleepy>(),
                RestStatus.Exhausted => ModContent.BuffType<Exhausted>(),
                _ => null,
            };

            foreach (int debuff in new[] { ModContent.BuffType<Tired>(), ModContent.BuffType<Sleepy>(), ModContent.BuffType<Exhausted>() })
            {
                if (debuff == current) Player.AddBuff(debuff, DebuffDurationTicks);
                else Player.ClearBuff(debuff);
            }

            if (status == RestStatus.Fainted)
            {
                CurrentRest = RestLogic.RestAfterFainting;
                Player.AddBuff(ModContent.BuffType<Fainted>(), RestLogic.FaintedDurationSeconds * 60);
            }
        }

        private void EnsureFaintedVFX()
        {
            int type = ModContent.ProjectileType<FaintedVFX>();
            if (Player.ownedProjectileCounts[type] > 0) return;

            // The projectile follows the player and removes itself when Fainted ends
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Top, Vector2.Zero, type, 0, 0, Player.whoAmI);
        }

        public override void PostUpdateBuffs()
        {
            // Fainted makes the player translucent; undo it once, when the buff ends
            bool fainted = IsFainted;
            if (_faintedLastUpdate && !fainted) Player.immuneAlpha = 0;
            _faintedLastUpdate = fainted;
        }

        public override bool CanUseItem(Item item)
        {
            if (IsFainted) return false;
            return base.CanUseItem(item);
        }

        public override void UpdateBadLifeRegen()
        {
            if (Player.lifeRegen <= 0) return;

            float factor = RestLogic.LifeRegenFactor(
                tired: Player.HasBuff(ModContent.BuffType<Tired>()),
                sleepy: Player.HasBuff(ModContent.BuffType<Sleepy>()),
                exhausted: Player.HasBuff(ModContent.BuffType<Exhausted>()),
                sleepDeprived: Player.HasBuff(ModContent.BuffType<SleepDeprived>()));

            Player.lifeRegen = (int)(Player.lifeRegen * factor);
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CurrentRest"] = CurrentRest;
            tag["timeNoSleep"] = timeNoSleep;
        }

        // Missing keys keep the Initialize() values (fully rested) instead of 0, which made the player faint on load.
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("CurrentRest"))
            {
                CurrentRest = RestLogic.Clamp(tag.GetFloat("CurrentRest"));
            }
            if (tag.ContainsKey("timeNoSleep"))
            {
                timeNoSleep = tag.GetInt("timeNoSleep");
            }
        }
    }
}
