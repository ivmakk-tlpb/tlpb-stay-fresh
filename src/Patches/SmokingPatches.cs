using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using StayFresh.Game;

namespace StayFresh.Patches
{
    // Smoking makes meat with no contamination. The game gives the new stack the contamination of the hardened stack
    // times 0.5, in the slot path and in the ground path, and then drops the hardened stack. So the prefix clears the
    // hardened stack when it is smoked meat: an animal product that hardens into a different consumable item.
    [HarmonyPatch(typeof(ItemHardenedCommand), "Execute")]
    internal static class SmokingPatches
    {
        private static AnimalProducts products;
        private static readonly HashSet<string> logged = new HashSet<string>();

        internal static void Reset()
        {
            products = null;
            logged.Clear();
        }

        [HarmonyPrefix]
        internal static void Prefix(ItemHardenedCommand __instance)
        {
            try
            {
                if (!Plugin.CookingRemovesContamination.Value) return;
                var signal = __instance?.signalParams;
                if (signal == null || signal.hardenedMixture != null) return;
                var stack = signal.hardenedStack;
                var item = stack?.Item;
                var hardened = item?.hardenedItem;
                if (hardened == null || hardened.Pointer == item.Pointer || !hardened.HasTag(ItemTag.CONSUMABLE)) return;
                products ??= CreatureData.Load();
                var identifier = item.Identifier;
                if (products == null || !products.Contains(identifier)) return;
                var old = stack.ContaminatedPercentage;
                stack.ContaminatedPercentage = 0f;
                if (Plugin.Verbose.Value && logged.Add(identifier))
                    Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Smoking: {0} into {1}, contamination {2} set to 0",
                        identifier, hardened.Identifier, old));
            }
            catch (Exception e)
            {
                Plugin.WarnOnce("Smoking", e);
            }
        }
    }
}
