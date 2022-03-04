using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using System;

namespace RainObelisk
{
    public class ModEntry : Mod, IAssetLoader, IAssetEditor
    {
        internal static ITranslationHelper i18n => helper.Translation;
        internal static IMonitor monitor;
        internal static IModHelper helper;
        internal static Harmony harmony;
        internal static string ModID;

        public override void Entry(IModHelper helper)
        {
            Monitor.Log("Starting up...", LogLevel.Debug);

            monitor = Monitor;
            ModEntry.helper = Helper;
            harmony = new(ModManifest.UniqueID);
            ModID = ModManifest.UniqueID;
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            harmony.PatchAll();
        }
        public void OnGameLaunched(object sender, GameLaunchedEventArgs ev)
        {
            ActionPatch.Setup();
        }
        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.Name.IsEquivalentTo("Buildings/RainObelisk");
        }

        public T Load<T>(IAssetInfo asset)
        {
            return helper.Content.Load<T>("assets/Obelisk.png");
        }
        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.Name.IsEquivalentTo("Data/BuildingsData");
        }
        public void Edit<T>(IAssetData asset)
        {
            var data = asset.AsDictionary<string, BuildingData>().Data;
            data.Add("tlitookilakin.rainObelisk", new()
            {
                Name = ModEntry.i18n.Get("buildings.obelisk.name"),
                Description = ModEntry.i18n.Get("buildings.obelisk.desc"),
                Texture = PathUtilities.NormalizeAssetName("Buildings/RainObelisk"),
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
    }
}
