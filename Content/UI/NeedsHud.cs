using ChallengingTerrariaMod.Common;
using ChallengingTerrariaMod.Common.Logic;
using ChallengingTerrariaMod.Content.ModConfigs;
using ChallengingTerrariaMod.Content.Systems.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ChallengingTerrariaMod.Content.UI
{
    // The four needs meters, side by side at the top of the screen.
    public class NeedsHud : UIState
    {
        private const string ImagePath = "ChallengingTerrariaMod/Content/Systems/UI/Images/";
        private const int MeterTop = 20;

        private Meter _warmthMeter;
        private Meter _sanityMeter;
        private Meter _restMeter;
        private Meter _hungerMeter;

        private Asset<Texture2D>[] _hungerSprites;
        private Asset<Texture2D>[] _restSprites;
        private Asset<Texture2D>[] _sanitySprites;
        private Asset<Texture2D> _warmthBaseSprite;
        private Asset<Texture2D>[] _warmthUpSprites;
        private Asset<Texture2D>[] _warmthDownSprites;

        private int _appliedOffsetX = -1;
        private int _appliedOffsetY = -1;

        public override void OnInitialize()
        {
            _hungerSprites = LoadSprites("HungerMeter/HungerMeter_{0}", 0, MeterSprites.HungerSpriteCount);
            _restSprites = LoadSprites("RestMeter/RestMeter_{0}", 0, MeterSprites.RestSpriteCount);
            _sanitySprites = LoadSprites("SanityMeter/SanityMeter_{0}", 0, MeterSprites.SanitySpriteCount);
            _warmthBaseSprite = ModContent.Request<Texture2D>(ImagePath + "WarmthMeter/Warmthmeter_base", AssetRequestMode.ImmediateLoad);
            _warmthUpSprites = LoadSprites("WarmthMeter/Warmthmeter_{0}_up", 1, MeterSprites.WarmthSpriteCount);
            _warmthDownSprites = LoadSprites("WarmthMeter/Warmthmeter_{0}_down", 1, MeterSprites.WarmthSpriteCount);

            _warmthMeter = AddMeter(700, 60, 60);
            _sanityMeter = AddMeter(750, 30, 51);
            _restMeter = AddMeter(800, 30, 50);
            _hungerMeter = AddMeter(850, 30, 50);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            ApplyConfigOffset();

            Player player = Main.LocalPlayer;
            UpdateHunger(player.GetModPlayer<HungerPlayer>().CurrentHunger);
            UpdateRest(player.GetModPlayer<RestPlayer>().CurrentRest);
            UpdateSanity(player.GetModPlayer<SanityPlayer>().CurrentSanity);
            WarmthPlayer warmthPlayer = player.GetModPlayer<WarmthPlayer>();
            UpdateWarmth(warmthPlayer.CurrentTemperature, warmthPlayer.LastTemperatureChange);
        }

        private void UpdateHunger(float hunger)
        {
            _hungerMeter.Texture = _hungerSprites[MeterSprites.Hunger(hunger)];
            if (!_hungerMeter.IsMouseHovering) return;

            HungerStatus status = HungerLogic.GetStatus(hunger);
            string hint = status switch
            {
                HungerStatus.Bloated or HungerStatus.Full => "HintTooMuch",
                HungerStatus.WellFed => null,
                _ => "HintEat",
            };
            ShowTooltip("Hunger", status.ToString(), hint);
        }

        private void UpdateRest(float rest)
        {
            _restMeter.Texture = _restSprites[MeterSprites.Rest(rest)];
            if (!_restMeter.IsMouseHovering) return;

            RestStatus status = RestLogic.GetStatus(rest);
            if (status == RestStatus.Fainted) status = RestStatus.Exhausted;
            ShowTooltip("Rest", status.ToString(), status == RestStatus.Rested ? null : "Hint");
        }

        private void UpdateSanity(float sanity)
        {
            _sanityMeter.Texture = _sanitySprites[MeterSprites.Sanity(sanity)];
            if (!_sanityMeter.IsMouseHovering) return;

            SanityStatus status = SanityLogic.GetStatus(sanity);
            if (status == SanityStatus.Gloom) status = SanityStatus.Terrified;
            ShowTooltip("Sanity", status.ToString(), status == SanityStatus.Well ? null : "Hint");
        }

        private void UpdateWarmth(int temperature, int lastChange)
        {
            WarmthSprite sprite = MeterSprites.Warmth(temperature, lastChange);
            _warmthMeter.Texture = sprite.Arrow switch
            {
                WarmthArrow.Up => _warmthUpSprites[sprite.Index],
                WarmthArrow.Down => _warmthDownSprites[sprite.Index],
                _ => _warmthBaseSprite,
            };
            if (!_warmthMeter.IsMouseHovering) return;

            TemperatureStatus status = TemperatureLogic.GetStatus(temperature);
            string hint = status switch
            {
                TemperatureStatus.Cold or TemperatureStatus.VeryCold or TemperatureStatus.Freezing => "HintCold",
                TemperatureStatus.Warm or TemperatureStatus.Hot or TemperatureStatus.Scorching => "HintHot",
                _ => null,
            };
            ShowTooltip("Warmth", status.ToString(), hint);
        }

        private static void ShowTooltip(string meter, string statusKey, string hintKey)
        {
            string text = Text.Get($"UI.{meter}.Title") + "\n" + Text.Get($"UI.{meter}.{statusKey}");
            if (hintKey != null) text += "\n" + Text.Get($"UI.{meter}.{hintKey}");
            Main.instance.MouseText(text);
        }

        private void ApplyConfigOffset()
        {
            var config = ModContent.GetInstance<ModConfigClient>();
            if (config.OffsetX == _appliedOffsetX && config.OffsetY == _appliedOffsetY) return;

            _appliedOffsetX = config.OffsetX;
            _appliedOffsetY = config.OffsetY;
            foreach (Meter meter in new[] { _warmthMeter, _sanityMeter, _restMeter, _hungerMeter })
            {
                meter.Left.Set(meter.BaseLeft + _appliedOffsetX, 0f);
                meter.Top.Set(MeterTop + _appliedOffsetY, 0f);
            }
            Recalculate();
        }

        private Meter AddMeter(int left, int width, int height)
        {
            var meter = new Meter(left);
            meter.Width.Set(width, 0f);
            meter.Height.Set(height, 0f);
            meter.Left.Set(left, 0f);
            meter.Top.Set(MeterTop, 0f);
            Append(meter);
            return meter;
        }

        private static Asset<Texture2D>[] LoadSprites(string pathFormat, int firstIndex, int count)
        {
            // Indexed by sprite number, so an array starting at 1 has an unused slot 0
            var sprites = new Asset<Texture2D>[firstIndex + count];
            for (int i = firstIndex; i < firstIndex + count; i++)
            {
                sprites[i] = ModContent.Request<Texture2D>(ImagePath + string.Format(pathFormat, i), AssetRequestMode.ImmediateLoad);
            }
            return sprites;
        }

        private class Meter : UIElement
        {
            public readonly int BaseLeft;
            public Asset<Texture2D> Texture;

            public Meter(int baseLeft)
            {
                BaseLeft = baseLeft;
            }

            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (Texture == null || !Texture.IsLoaded) return;
                spriteBatch.Draw(Texture.Value, GetDimensions().Position(), Color.White);
            }
        }
    }
}
