using System;
using System.Globalization;
using HarmonyLib;

namespace StayFresh.Patches
{
    // The tooltip of an item stack that spoils gets one line with the in-game time left. The rate is the rate of the
    // game for the root holder of the stack (holder factor, world option, cold) times the factor of the mod.
    [HarmonyPatch(typeof(ShowTooltipEventHandler), "addItemStackDetailTooltipLines")]
    internal static class TooltipPatches
    {
        // The unit words in the language of the game, read once.
        private static SpoilageTime.Words? words;

        private static readonly System.Collections.Generic.HashSet<string> skipped = new System.Collections.Generic.HashSet<string>();

        internal static void Reset()
        {
            words = null;
            skipped.Clear();
        }

        // The game adds the line at the end, after the hint "Right click for options": move it up (TooltipOrder).
        private static void MoveUp(TooltipLineUI line)
        {
            var own = line?.transform;
            var parent = own?.parent;
            if (parent == null) return;
            var texts = new System.Collections.Generic.List<string>();
            for (int i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                var sibling = child.GetComponent<TooltipLineUI>();
                texts.Add(sibling != null && child.gameObject.activeSelf ? sibling.text?.text : null);
            }
            var target = TooltipOrder.Target(texts, own.GetSiblingIndex());
            if (target >= 0) own.SetSiblingIndex(target);
        }

        // One Verbose line for each item and reason when the tooltip of a stack that spoils gets no line.
        private static void Skip(InventoryItem item, string reason)
        {
            if (!Plugin.Verbose.Value) return;
            var line = "Tooltip: no line for " + item.Identifier + ": " + reason;
            if (skipped.Add(line)) Plugin.Debug(line);
        }

        [HarmonyPostfix]
        internal static void Postfix(ShowTooltipEventHandler __instance, TooltipUI tooltipUI, InventoryItemStack stack)
        {
            try
            {
                var item = stack?.Item;
                if (item == null || item.DecayRate <= 0f) return;
                var inventory = stack.slot?.inventory;
                if (inventory == null) { Skip(item, "no inventory"); return; }
                var owner = inventory.GetRootInventory()?.GetOwnerEntity();
                if (owner == null) { Skip(item, "no owner of the root inventory"); return; }
                // The entity holds an InventoryHolderView, which wraps the InventoryHolder that processes the stacks.
                var component = owner.GetCachedComponent<IInventoryHolder>(false);
                var holder = component?.TryCast<InventoryHolder>() ?? component?.TryCast<InventoryHolderView>()?.entityComponent;
                if (holder == null) { Skip(item, "no holder in " + owner.identifier); return; }

                var holderFactor = holder.contentsDecayFactor.Value;
                var world = owner.world;
                var option = world?.game?.gameOptions.advancedGameOptions?.decayRateFactor ?? 1f;
                var cold = world == null ? 1f : SpoilageTime.ColdFactor(world.temperature);
                var gameFactor = holderFactor * option * cold;
                // With the spoilage group off, the items spoil at the game rate, and the line shows that rate.
                var modFactor = Plugin.Applied.Contains("spoilage") ? SpoilagePatches.ModFactor(holder, inventory, item, gameFactor) : 1f;
                if (modFactor == null) { Skip(item, "the game data is not loaded"); return; }

                var seconds = SpoilageTime.SecondsLeft(stack.Condition, item.DecayRate, stack.PreservedPercentage, gameFactor, modFactor.Value);
                if (seconds == null) { Skip(item, "no spoilage at the moment"); return; }
                words ??= new SpoilageTime.Words(L.T("game.week"), L.T("game.weeks"), L.T("game.day"), L.T("game.days"), L.T("game.hour"), L.T("game.hours"));
                var text = SpoilageTime.Text(SpoilageTime.Format(seconds.Value, words.Value));
                MoveUp(__instance.addTooltipLine(tooltipUI, text, false));
                if (Plugin.Verbose.Value)
                    Plugin.Debug(string.Format(CultureInfo.InvariantCulture,
                        "Tooltip: {0}, condition {1}, decay rate {2}, preserved {3}, game factor {4} (holder {5}, option {6}, cold {7}), mod factor {8}, {9}",
                        item.Identifier, stack.Condition, item.DecayRate, stack.PreservedPercentage, gameFactor, holderFactor, option, cold, modFactor.Value, text));
            }
            catch (Exception e)
            {
                Plugin.WarnOnce("Tooltip", e);
            }
        }
    }
}
