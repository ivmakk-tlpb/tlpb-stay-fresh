using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using HarmonyLib;
using StayFresh.Game;

namespace StayFresh.Patches
{
    // The spoilage of each stack: the game passes one decay factor for a holder to processStack, which uses it only for
    // the condition loss of the stack and the decay of its liquid. The prefix multiplies it by the factor of the mod: the
    // multiplier of the item and the covered container part.
    [HarmonyPatch(typeof(InventoryHolder), "processStack")]
    internal static class SpoilagePatches
    {
        private const float TimerPeriod = 10f;

        private static SpoilageRule rule;
        private static ContainerRule containers;
        // Keyed by the native item. Each entry keeps the interop object of its item, whose handle keeps the native item
        // alive, so a freed address cannot come back as another item with this factor.
        private static readonly Dictionary<IntPtr, (InventoryItem Item, float Factor)> factors = new Dictionary<IntPtr, (InventoryItem, float)>();
        private static readonly Dictionary<IntPtr, (InventoryItem Item, float Factor)> contentFactors = new Dictionary<IntPtr, (InventoryItem, float)>();
        private static readonly HashSet<string> logged = new HashSet<string>();
        private static FrameTimer timer = new FrameTimer("Spoilage prefix", TimerPeriod);

        // The holder factor and the parent factor are read once for each holder and inventory each frame: the game
        // processes the stacks of one holder together, and a pointer is not reused within one frame.
        private static int frame = -1;
        private static readonly Dictionary<IntPtr, float> holderFactors = new Dictionary<IntPtr, float>();
        private static readonly Dictionary<IntPtr, float> parentFactors = new Dictionary<IntPtr, float>();

        internal static void Reset()
        {
            rule = null;
            containers = null;
            factors.Clear();
            contentFactors.Clear();
            logged.Clear();
            frame = -1;
            holderFactors.Clear();
            parentFactors.Clear();
            timer = new FrameTimer("Spoilage prefix", TimerPeriod);
        }

        [HarmonyPrefix]
        internal static void Prefix(InventoryHolder __instance, InventoryItemStack stack, Inventory inventory, ref float decayFactor)
        {
            var verbose = Plugin.Verbose.Value;
            var start = verbose ? Stopwatch.GetTimestamp() : 0L;
            try
            {
                var item = stack?.Item;
                if (item != null) decayFactor *= FactorOf(item, __instance, decayFactor) * ContainerFactor(__instance, inventory);
            }
            catch (Exception e)
            {
                Plugin.WarnOnce("Spoilage", e);
            }
            if (!verbose) return;
            var elapsed = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
            var line = timer.Add(elapsed, UnityEngine.Time.frameCount, UnityEngine.Time.realtimeSinceStartup);
            if (line != null) Plugin.Debug(line);
        }

        // The factor of the mod for a stack of the item in the inventory of the holder, as the prefix gives it. Null
        // before the game has loaded its data.
        internal static float? ModFactor(InventoryHolder holder, Inventory inventory, InventoryItem item, float gameFactor)
        {
            var factor = FactorOf(item, holder, gameFactor);
            if (rule == null) return null;
            return factor * ContainerFactor(holder, inventory);
        }

        // Cached by the native item, so the identifier, which allocates a managed string, is read once for each item.
        private static float FactorOf(InventoryItem item, InventoryHolder holder, float gameFactor)
        {
            if (factors.TryGetValue(item.Pointer, out var cached)) return cached.Factor;
            if (rule == null)
            {
                var products = CreatureData.Load();
                if (products == null) return 1f;
                rule = new SpoilageRule(products, Plugin.Spoilage, Plugin.AnimalSpoilage);
            }
            var identifier = item.Identifier;
            var factor = rule.Multiplier(identifier);
            factors[item.Pointer] = (item, factor);
            if (logged.Add(identifier))
                Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Spoilage: {0} ({1}) in {2}, game factor {3}, mod factor {4}",
                    identifier, rule.IsAnimalProduct(identifier) ? "animal product" : "item", holder?.entity?.identifier, gameFactor, factor));
            return factor;
        }

        private static float ContainerFactor(InventoryHolder holder, Inventory inventory)
        {
            if (containers == null)
            {
                if (rule == null) return 1f;
                Dictionary<string, float> found;
                try
                {
                    found = ItemDefinitions.LoadContentFactors();
                }
                catch (Exception e)
                {
                    // The item multiplier still applies: the covered containers then count as in the game.
                    Plugin.WarnOnce("Item definitions", e);
                    found = new Dictionary<string, float>();
                }
                if (found == null) return 1f;
                containers = new ContainerRule(found, Plugin.Preservation);
            }
            var now = UnityEngine.Time.frameCount;
            if (now != frame)
            {
                frame = now;
                holderFactors.Clear();
                parentFactors.Clear();
            }
            if (!holderFactors.TryGetValue(holder.Pointer, out var holderFactor))
                holderFactors[holder.Pointer] = holderFactor = holder.contentsDecayFactor.Value;
            var key = inventory?.Pointer ?? IntPtr.Zero;
            if (!parentFactors.TryGetValue(key, out var parentFactor))
            {
                var parent = inventory?.parentStack?.Item;
                parentFactors[key] = parentFactor = parent == null ? 1f : ContentFactorOf(parent);
                var factor = containers.Factor(holderFactor, parentFactor);
                if (factor != 1f && Plugin.Verbose.Value && logged.Add("container " + parent?.Identifier + " " + holder.entity?.identifier))
                    Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Covered container: {0} in {1}, holder factor {2}, mod factor {3}",
                        parent?.Identifier, holder.entity?.identifier, holderFactor, factor));
                return factor;
            }
            return containers.Factor(holderFactor, parentFactor);
        }

        private static float ContentFactorOf(InventoryItem item)
        {
            if (contentFactors.TryGetValue(item.Pointer, out var cached)) return cached.Factor;
            var factor = containers.ContentFactor(item.Identifier);
            contentFactors[item.Pointer] = (item, factor);
            return factor;
        }
    }
}
