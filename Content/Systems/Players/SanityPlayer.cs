using ChallengingTerrariaMod.Common;
using ChallengingTerrariaMod.Common.Logic;
using ChallengingTerrariaMod.Content.Buffs;
using ChallengingTerrariaMod.Content.ModConfigs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ChallengingTerrariaMod.Content.Systems.Players
{
    public class SanityPlayer : ModPlayer
    {
        private const int DebuffDurationTicks = 61; // Renewed every second while the condition lasts
        private const int FleeDurationTicks = 120;
        private const float TerrifiedFleeChance = 0.2f;

        public float CurrentSanity;
        public int FleeDirection; // Direction the player runs to while Flee is active

        private readonly GloomCountdown _gloom = new GloomCountdown();

        public override void Initialize()
        {
            CurrentSanity = SanityLogic.Max;
        }

        // The owner's client simulates sanity; buffs are synced to other players by vanilla.
        private bool IsSimulated => Player.whoAmI == Main.myPlayer && !Player.dead && !Player.ghost;

        public override void OnHurt(Player.HurtInfo info)
        {
            if (Player.whoAmI != Main.myPlayer) return;

            if (Player.HasBuff(ModContent.BuffType<Terrified>()) && !Player.HasBuff(ModContent.BuffType<Flee>()) && Main.rand.NextFloat() < TerrifiedFleeChance)
            {
                FleeDirection = info.HitDirection;
                Player.AddBuff(ModContent.BuffType<Flee>(), FleeDurationTicks);
            }
        }

        public override void PostUpdate()
        {
            if (!IsSimulated || Main.GameUpdateCount % SanityLogic.UpdateRate != 0) return;

            var context = new SanityContext
            {
                ArmoredMind = Player.HasBuff(ModContent.BuffType<ArmoredMind>()),
                InHostileZone = Player.ZoneDungeon || Player.ZoneUnderworldHeight || Player.ZoneCrimson || Player.ZoneCorrupt,
                LowHealth = SanityLogic.IsLowHealth(Player.statLife, Player.statLifeMax2),
                EclipseOrBloodMoon = Main.eclipse || Main.bloodMoon,
                InOverworld = Player.ZoneOverworldHeight,
                TownNPCs = Player.townNPCs,
                Multiplier = ModContent.GetInstance<ModConfigServer>().SanityMultiplier,
            };

            CurrentSanity = SanityLogic.Clamp(CurrentSanity + SanityLogic.Delta(context));
            ApplySanityDebuffs();
        }

        private void ApplySanityDebuffs()
        {
            SanityStatus status = SanityLogic.GetStatus(CurrentSanity);

            if (status == SanityStatus.Gloom) UpdateGloom();
            else if (_gloom.Recover()) Main.NewText(Text.Get("Messages.GloomEscaped"), Color.DarkRed);

            int? current = status switch
            {
                SanityStatus.Stressed => ModContent.BuffType<Stressed>(),
                SanityStatus.Scared => ModContent.BuffType<Scared>(),
                SanityStatus.Terrified or SanityStatus.Gloom => ModContent.BuffType<Terrified>(),
                _ => null,
            };

            foreach (int debuff in new[] { ModContent.BuffType<Stressed>(), ModContent.BuffType<Scared>(), ModContent.BuffType<Terrified>() })
            {
                if (debuff == current) Player.AddBuff(debuff, DebuffDurationTicks);
                else Player.ClearBuff(debuff);
            }
        }

        private void UpdateGloom()
        {
            GloomStep step = _gloom.Tick();

            if (step.ShowWarning) Main.NewText(Text.Get("Messages.GloomWarning"), Color.DarkRed);
            if (step.Countdown > 0) Main.NewText(Text.Get("Messages.GloomCountdown", step.Countdown), Color.DarkRed);
            if (step.Kill)
            {
                Player.KillMe(PlayerDeathReason.ByCustomReason(Text.Network("DeathReasons.Gloom", Player.name)), 999, 0);
            }
        }

        public override void OnRespawn()
        {
            CurrentSanity = SanityLogic.Max;
            _gloom.Reset();
        }

        public override void SaveData(TagCompound tag)
        {
            tag["CurrentSanity"] = CurrentSanity;
        }

        // A missing key keeps the Initialize() value (full sanity) instead of 0, which started the death countdown.
        public override void LoadData(TagCompound tag)
        {
            if (tag.ContainsKey("CurrentSanity"))
            {
                CurrentSanity = SanityLogic.Clamp(tag.GetFloat("CurrentSanity"));
            }
        }
    }
}
