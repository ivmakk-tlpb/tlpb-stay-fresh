using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace StayFresh
{
    [BepInPlugin(PluginGuid, "Stay Fresh", "1.0.0")]
    [BepInProcess("TheLastPlague.exe")]
    public sealed class Plugin : BasePlugin
    {
        public const string PluginGuid = "com.ivmakk.thelastplagueblight.stayfresh";

        internal static new ManualLogSource Log;
        internal static ConfigEntry<bool> CookingRemovesContamination;
        internal static ConfigEntry<float> SpoilageSlowdown;
        internal static ConfigEntry<float> AnimalSpoilageSlowdown;
        internal static ConfigEntry<float> PreservationMultiplier;
        internal static ConfigEntry<bool> Verbose;

        // A patch group is applied alone with its own Harmony id: a missing target removes only that group.
        internal sealed class PatchGroup
        {
            public readonly string Name;
            public readonly string OffText;
            public readonly List<Type> Types;

            public PatchGroup(string name, string offText, params Type[] types)
            {
                Name = name;
                OffText = offText;
                Types = new List<Type>(types);
            }
        }

        internal static readonly List<PatchGroup> Groups = new List<PatchGroup>
        {
            new PatchGroup("cooking", "Stay Fresh leaves the contamination of cooked and smoked food as in the game", typeof(Patches.CookingPatches), typeof(Patches.SmokingPatches)),
            new PatchGroup("spoilage", "Stay Fresh leaves the spoilage of items as in the game", typeof(Patches.SpoilagePatches),
                typeof(Patches.PreservationPatches), typeof(Patches.FermentationPatches)),
            new PatchGroup("carcass", "Stay Fresh leaves the decay of carcasses as in the game", typeof(Patches.CarcassPatches)),
            new PatchGroup("tooltip", "Stay Fresh shows no spoilage time in the tooltip", typeof(Patches.TooltipPatches)),
        };

        private static readonly HashSet<string> warned = new HashSet<string>();

        // The names of the patch groups that are in place.
        internal static readonly HashSet<string> Applied = new HashSet<string>();

        // Each value is limited to its range when it is read, also after an edit of the config file by hand.
        // The factor that the patches multiply the spoilage by: 1 / slowdown, and 0 at a slowdown of 0.
        internal static float Spoilage => SpoilageFactor(SpoilageSlowdown.Value);
        internal static float AnimalSpoilage => SpoilageFactor(AnimalSpoilageSlowdown.Value);

        // 0 is no spoilage. Above 0, the slowdown is 1 (the game speed) to 20: the mod never spoils faster than the game,
        // and the game subtracts the step of each frame from a float, so a smaller step is lost and the item would stop
        // spoiling as at 0.
        private const float HighestSlowdown = 20f;

        private static float SpoilageFactor(float slowdown) => slowdown <= 0f ? 0f : 1f / Math.Clamp(slowdown, 1f, HighestSlowdown);
        internal static float Preservation => Math.Clamp(PreservationMultiplier.Value, 1f, 10f);

        public override void Load()
        {
            Log = base.Log;
            warned.Clear();
            Applied.Clear();
            CookingRemovesContamination = Config.Bind(
                "General", "CookingRemovesContamination", true,
                "Each food item that cooking or smoking makes starts with no contamination, also boiled water, teas, and soups in the cooking vessel. Dried bones and dried flowers keep the game value. With false, cooking and smoking give the contamination of the game.");
            SpoilageSlowdown = Config.Bind(
                "General", "SpoilageSlowdown", 5f,
                new ConfigDescription(
                    "How many times more slowly each item and liquid spoils, other than an animal product. 5 is 5 times slower, 1 is as in the game, and 20 is the highest. A value between 0 and 1 counts as 1. 0 means no spoilage; salt, smoking, and fermentation still work. Cooked, smoked, and cured meat use this value.",
                    new AcceptableValueRange<float>(0f, HighestSlowdown)));
            AnimalSpoilageSlowdown = Config.Bind(
                "General", "AnimalSpoilageSlowdown", 5f,
                new ConfigDescription(
                    "How many times more slowly raw meat, hides, furs, fat, and other animal products spoil, also inside a carcass, and how many times longer a carcass stays in the world. 5 is 5 times slower, 1 is as in the game, and 20 is the highest. A value between 0 and 1 counts as 1. 0 means no spoilage, and a carcass then stays in the world until you harvest it.",
                    new AcceptableValueRange<float>(0f, HighestSlowdown)));
            PreservationMultiplier = Config.Bind(
                "General", "PreservationMultiplier", 2f,
                new ConfigDescription(
                    "A Covered Clay Jar or a Covered Glass Bottle slows the spoilage of its items this many times more, also in the backpack. Salt preservation builds and juice in a covered container ferments this many times faster. 1.0 is as in the game.",
                    new AcceptableValueRange<float>(1f, 10f)));
            Verbose = Config.Bind(
                "General", "Verbose", false,
                "Writes extra diagnostic lines to the BepInEx log for troubleshooting. Keep off in normal play.");
            foreach (var group in Groups) Apply(group);
            Log.LogInfo("Stay Fresh loaded.");
        }

        // A game update can rename a target. Then the patches of that group go, and the other groups stay.
        private static void Apply(PatchGroup group)
        {
            var harmony = new Harmony(PluginGuid + "." + group.Name);
            try
            {
                foreach (var patches in group.Types) harmony.PatchAll(patches);
                Applied.Add(group.Name);
                Debug("Patch group " + group.Name + " applied");
            }
            catch (Exception e)
            {
                harmony.UnpatchSelf();
                Log.LogWarning(group.OffText + ": " + e.Message);
            }
        }

        // One warning for each distinct error, so a repeated error does not flood the log.
        internal static void WarnOnce(string where, Exception e)
        {
            string text = where + ": " + e.GetType().Name + ": " + e.Message;
            if (warned.Add(text)) Log.LogWarning(text + "\n" + e);
        }

        internal static void Debug(string line)
        {
            if (Verbose.Value) Log.LogDebug(line);
        }
    }
}
