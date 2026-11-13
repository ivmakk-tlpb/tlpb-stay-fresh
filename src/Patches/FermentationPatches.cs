using System.Globalization;
using HarmonyLib;

namespace StayFresh.Patches
{
    // A liquid in a covered container ferments PreservationMultiplier times faster. The only caller is the spoilage of a
    // stack whose item ferments its contents, so the fermentation elsewhere stays as in the game.
    [HarmonyPatch(typeof(SubstanceMixture), "UpdateFermentation")]
    internal static class FermentationPatches
    {
        private static bool logged;

        internal static void Reset() { logged = false; }

        [HarmonyPrefix]
        internal static void Prefix(ref float deltaTime)
        {
            var old = deltaTime;
            deltaTime *= Plugin.Preservation;
            if (logged || !Plugin.Verbose.Value) return;
            logged = true;
            Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Fermentation: time step {0} set to {1}", old, deltaTime));
        }
    }
}
