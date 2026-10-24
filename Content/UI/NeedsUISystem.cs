using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChallengingTerrariaMod.Content.UI
{
    // Creates, updates and draws the needs HUD. Client only.
    public class NeedsUISystem : ModSystem
    {
        private UserInterface _interface;
        private NeedsHud _hud;

        public override void Load()
        {
            if (Main.dedServ) return;

            _hud = new NeedsHud();
            _interface = new UserInterface();
            _interface.SetState(_hud);
        }

        public override void Unload()
        {
            _hud = null;
            _interface = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (ShouldShow()) _interface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseTextIndex == -1) return;

            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "ChallengingTerrariaMod: Needs HUD",
                delegate
                {
                    if (ShouldShow()) _interface.Draw(Main.spriteBatch, Main._drawInterfaceGameTime);
                    return true;
                },
                InterfaceScaleType.UI));
        }

        private bool ShouldShow()
        {
            Player player = Main.LocalPlayer;
            return _interface != null && player.active && !player.dead && !player.ghost;
        }
    }
}
