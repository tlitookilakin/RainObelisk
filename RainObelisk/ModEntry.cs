using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using System;

namespace RainObelisk
{
    public class ModEntry : Mod
    {
        private ITranslationHelper i18n => Helper.Translation;
        private IReflectedField<Multiplayer> mp;

        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Starting up...", LogLevel.Trace);
            GameLocation.RegisterTileAction("UseRainObelisk", (loc, args, who, pos) => SetRain(loc, who.Position));
            GameStateQuery.RegisterQueryType("LOCATION_RAIN_TOTEM_ALLOWED", RainTotemAllowedHere);
            mp = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer");
            helper.Events.Content.AssetRequested += (object _, AssetRequestedEventArgs ev) =>
            {
                if (ev.NameWithoutLocale.IsEquivalentTo("Mods/RainObelisk/Default"))
                    ev.LoadFromModFile<Texture2D>("assets/Obelisk.png", AssetLoadPriority.Low);
                else if (ev.NameWithoutLocale.IsEquivalentTo("Data/BuildingsData"))
                    ev.Edit(AddBuilding);
            };
        }
        private void AddBuilding(IAssetData asset)
        {
            var data = asset.AsDictionary<string, BuildingData>().Data;
            data.Add("tlitookilakin.rainObelisk", new()
            {
                Name = i18n.Get("buildings.obelisk.name"),
                Description = i18n.Get("buildings.obelisk.desc"),
                Texture = PathUtilities.NormalizeAssetName("Mods/RainObelisk/Default"),
                BuildCondition = "LOCATION_RAIN_TOTEM_ALLOWED Target",
                Builder = "Wizard",
                BuildCost = 500_000,
                BuildMaterials = new()
                {
                    new() { ItemID = "(O)337", Amount = 10 }, //Iridium bars
                    new() { ItemID = "(O)84", Amount = 10 }, //Frozen tears
                    new() { ItemID = "(O)432", Amount = 10 } //Truffle oil
                },
                Size = new(3, 2),
                DefaultAction = "UseRainObelisk"
            });
        }
        private bool RainTotemAllowedHere(string[] split)
        {
            return split.Length <= 1 || (GameStateQuery.GetLocation(split[1])?.GetLocationContext()?.AllowRainTotem ?? true);
        } 
        private bool SetRain(GameLocation where, Vector2 pos)
        {
            var context = where.GetLocationContext();

            if (!context.AllowRainTotem)
                return false;

            if (context == GameLocation.LocationContext.Default)
            {
                if (!Utility.isFestivalDay(Game1.dayOfMonth + 1, Game1.currentSeason))
                {
                    Game1.netWorldState.Value.WeatherForTomorrow = Game1.weatherForTomorrow = "Rain";
                    Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"), showProgressBar: false);
                }
            }
            else
            {
                Game1.netWorldState.Value.GetWeatherForLocation(context).weatherForTomorrow.Value = "Rain";
                Game1.pauseThenMessage(2000, Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.12822"), showProgressBar: false);
            }
            Game1.screenGlow = false;
            where.playSound("thunder");
            Game1.screenGlowOnce(Color.SlateBlue, hold: false);
            for (int i = 0; i < 6; i++)
            {
                mp?.GetValue()?.broadcastSprites(where, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(648, 1045, 52, 33), 9999f, 1, 999, pos + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
                {
                    motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -2f),
                    delayBeforeAnimationStart = i * 200
                },
                new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(648, 1045, 52, 33), 9999f, 1, 999, pos + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
                {
                    motion = new Vector2((float)Game1.random.Next(-30, -10) / 10f, -1f),
                    delayBeforeAnimationStart = 100 + i * 200
                },
                new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(648, 1045, 52, 33), 9999f, 1, 999, pos + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 1f, 0.01f, 0f, 0f)
                {
                    motion = new Vector2((float)Game1.random.Next(10, 30) / 10f, -1f),
                    delayBeforeAnimationStart = 200 + i * 200
                });
            }
            DelayedAction.playSoundAfterDelay("rainsound", 2000);
            return true;
        }
    }
}
