using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;

namespace StayFresh.Patches
{
    // Cooking makes food with no contamination. An item result takes the contamination of the result that
    // CookingManager computes, also for the preview of the cooking window. A liquid result goes into the mixture of
    // the cooking vessel, which keeps its own contamination, so the end of the cooking clears that mixture.
    internal static class CookingPatches
    {
        private static readonly HashSet<string> logged = new HashSet<string>();

        internal static void Reset() { logged.Clear(); }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CookingManager), "GetResultForIngredients")]
        internal static void ResultPostfix(CookingRecipe recipe, CookingRecipeResult __result)
        {
            try
            {
                if (!Plugin.CookingRemovesContamination.Value || __result == null) return;
                var old = __result.contaminatedPercentage;
                if (old == 0f) return;
                __result.contaminatedPercentage = 0f;
                if (Plugin.Verbose.Value && recipe != null && logged.Add(recipe.Identifier))
                    Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Cooking: {0}, contamination {1} set to 0", recipe.Identifier, old));
            }
            catch (Exception e)
            {
                Plugin.WarnOnce("Cooking", e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CookingCompleteEffect), "Execute")]
        internal static void CompletePostfix(CookingCompleteEffect __instance)
        {
            try
            {
                if (!Plugin.CookingRemovesContamination.Value || __instance == null) return;
                var recipe = __instance.recipe;
                if (recipe == null || !HasLiquidOutput(recipe)) return;
                var cooker = __instance.cookerEntity;
                var vessel = cooker?.GetCachedComponent<ICooker>(false)?.GetContainerSubstanceMixture();
                if (vessel == null) return;
                var old = vessel.contaminatedPercentage;
                if (old == 0f) return;
                vessel.contaminatedPercentage = 0f;
                if (Plugin.Verbose.Value && logged.Add("liquid " + recipe.Identifier))
                    Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Cooking: {0} in {1}, liquid contamination {2} set to 0",
                        recipe.Identifier, cooker.identifier, old));
            }
            catch (Exception e)
            {
                Plugin.WarnOnce("Cooking liquid", e);
            }
        }

        private static bool HasLiquidOutput(CookingRecipe recipe)
        {
            var outputs = recipe.GetOutputIngredients();
            for (int i = 0; outputs != null && i < outputs.Count; i++)
                if (!string.IsNullOrEmpty(outputs[i]?.substanceIdentifier)) return true;
            return false;
        }
    }
}
