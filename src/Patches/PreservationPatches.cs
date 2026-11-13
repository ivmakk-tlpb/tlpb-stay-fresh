using System.Globalization;
using HarmonyLib;

namespace StayFresh.Patches
{
    // Salt preservation builds PreservationMultiplier times faster. The game keeps its limit of 1 on the preserved share.
    [HarmonyPatch(typeof(InventoryItemStack), "AddPreservation")]
    internal static class PreservationPatches
    {
        private static bool logged;

        internal static void Reset() { logged = false; }

        [HarmonyPrefix]
        internal static void Prefix(ref float preservationFactor)
        {
            var old = preservationFactor;
            preservationFactor *= Plugin.Preservation;
            if (logged || old == 0f || !Plugin.Verbose.Value) return;
            logged = true;
            Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Salt preservation: factor {0} set to {1}", old, preservationFactor));
        }
    }
}
