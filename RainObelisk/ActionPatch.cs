using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RainObelisk
{
    [HarmonyPatch(typeof(GameLocation))]
    internal class ActionPatch
    {
        private static FieldInfo mp;
        internal static void Setup()
        {
            mp = AccessTools.Field(typeof(Game1), "multiplayer");
        }

        [HarmonyPatch("performAction")]
        [HarmonyPrefix]
        public static bool PerformAction(GameLocation __instance, string action, Farmer who, ref bool __result)
        {
            if(action == "UseRainObelisk")
            {
                SetRain(__instance, who.Position);
                __result = true; //executed action
                return false; //skip
            }
            return true;
        }
        public static void SetRain(GameLocation where, Vector2 pos)
        {
            var context = where.GetLocationContext();
            if(context == GameLocation.LocationContext.Default)
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
            Multiplayer multiplayer = null;
            try
            {
                multiplayer = (Multiplayer)mp.GetValue(Game1.game1);
            } catch(Exception e)
            {
                ModEntry.monitor.Log("Could not retrieve multiplayer instance!", LogLevel.Warn);
                ModEntry.monitor.Log(e.ToString(), LogLevel.Warn);
            }
            for (int i = 0; i < 6; i++)
            {
                multiplayer?.broadcastSprites(where, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(648, 1045, 52, 33), 9999f, 1, 999, pos + new Vector2(0f, -128f), flicker: false, flipped: false, 1f, 0.01f, Color.White * 0.8f, 2f, 0.01f, 0f, 0f)
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
        }
    }
}
