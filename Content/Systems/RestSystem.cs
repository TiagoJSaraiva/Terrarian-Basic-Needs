using Terraria;
using Terraria.ModLoader;
using Terraria.ID; 
using ChallengingTerrariaMod.Content.Systems.Players;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.UI;
using Terraria.Localization;
using ChallengingTerrariaMod.Content.Systems.UI;
using System;
using ChallengingTerrariaMod.Content.Buffs;
using ChallengingTerrariaMod.Content.ModConfigs;

namespace ChallengingTerrariaMod.Content.Systems
{
    public class RestSystem : ModSystem
    {
        public const int restUpdateRate = 60; // Atualiza a cada 60 ticks (1 segundo real)

        // Sleep gain and loss of the player
        private const float sleepPerSecond = 4;

        private const float sleepPerSecondAccelerated = 24; // Tiredness loss per second when the time is accelerated

        public const short maxSleep = 1200;
        public const int minSleep = 0;

        // Removeremos as constantes de horário específicas, pois não serão mais usadas para a lógica de ganho.
        // private const double START_SLEEP_GAIN_TIME = 19.5; 
        // private const double END_SLEEP_GAIN_TIME = 4.5; 

        // UI do sono
        public static UserInterface RestUserInterface;
        public static RestMeterUI RestUIState;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                RestUIState = new RestMeterUI();
                RestUIState.Activate();
                RestUserInterface = new UserInterface();
                RestUserInterface.SetState(RestUIState);
            }
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                RestUIState = null;
                RestUserInterface = null;
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (RestUserInterface?.CurrentState != null)
            {
                RestUserInterface.Update(gameTime);
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "ChallengingTerrariaMod: Rest UI",
                    delegate
                    {
                        // Desenha a UI apenas se o jogador local estiver ativo e não morto/fantasma
                        if (Main.LocalPlayer.active && !Main.LocalPlayer.dead && !Main.LocalPlayer.ghost)
                        {
                            RestUserInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        private void UpdateRest(RestPlayer restPlayer, float _sleepPerSecond, float _sleepPerSecondAccelerated)
        {
            if (Main.dayRate > 1)
            {
                restPlayer.CurrentRest += _sleepPerSecondAccelerated;
            }
            else
            {
                restPlayer.CurrentRest += _sleepPerSecond;
            }
        }

        public override void PostUpdatePlayers()
        {
            if (Main.GameUpdateCount % restUpdateRate == 0)
            {
                foreach (Player player in Main.player)
                {
                    if (player.active && !player.dead && !player.ghost)
                    {
                        RestPlayer restPlayer = player.GetModPlayer<RestPlayer>();
                        float RestMultiplier = ModContent.GetInstance<ModConfigServer>().RestMultiplier;
                        float _sleepPerSecond = sleepPerSecond * RestMultiplier;
                        float _sleepPerSecondAccelerated = sleepPerSecondAccelerated * RestMultiplier;

                        if (restPlayer == null) continue;

                        if (player.sleeping.isSleeping)
                        {
                            if (player.townNPCs < 2)
                            {
                                UpdateRest(restPlayer, sleepPerSecond / 4, sleepPerSecondAccelerated / 2);
                                player.AddBuff(ModContent.BuffType<BadRest>(), 60);
                            }
                            else
                            {
                                UpdateRest(restPlayer, sleepPerSecond, sleepPerSecondAccelerated);
                            }
                            restPlayer.timeNoSleep -= 30;
                            restPlayer.timeNoSleep = Utils.Clamp(restPlayer.timeNoSleep, 0, 1200);
                        }
                        else if (player.sitting.isSitting)
                        {
                            restPlayer.CurrentRest += sleepPerSecond / 4;
                            player.AddBuff(ModContent.BuffType<BadRest>(), 60);
                        }
                        else if (player.HasBuff(ModContent.BuffType<SleepDeprived>()) || !Main.dayTime)
                        {
                            if (player.HasBuff(ModContent.BuffType<Cafeinated>()))
                            {
                                restPlayer.CurrentRest -= _sleepPerSecond / 2;
                            }
                            else
                            {
                                restPlayer.CurrentRest -= _sleepPerSecond;
                            }
                        }
                        restPlayer.CurrentRest = Utils.Clamp(restPlayer.CurrentRest, minSleep, maxSleep);
                    }
                }
            }
        }

        public static int RoundValue(int value, float divisor)
        {
            value = (int)(value / divisor);

            while (value % 20 != 0)
            {
                value--;
            }

            return value;
        }
    }
}