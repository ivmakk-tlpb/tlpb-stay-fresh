using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using HarmonyLib;

namespace StayFresh.Patches
{
    // A carcass decays by the factor of AnimalSpoilageSlowdown: for the frame of its Update, the prefix scales the decay rate and
    // the rain decay rate of the view, and the postfix puts them back. The game then computes the step with its scaled
    // time, its rain, its limit of 1, and its fully decayed signal, also for a large catch-up step after the carcass was
    // out of range. A view is a carcass when its entity has a creature component, checked once for each view.
    internal static class CarcassPatches
    {
        private const float TimerPeriod = 10f;

        private static readonly Dictionary<IntPtr, bool> carcasses = new Dictionary<IntPtr, bool>();
        private static float gameRate;
        private static float gameRainRate;
        // The carcass views with their Verbose line, so each carcass writes one line at its first scaled frame.
        private static readonly HashSet<IntPtr> logged = new HashSet<IntPtr>();
        private static FrameTimer timer = new FrameTimer("Carcass patch", TimerPeriod);

        internal static void Reset()
        {
            carcasses.Clear();
            logged.Clear();
            timer = new FrameTimer("Carcass patch", TimerPeriod);
        }

        // __state is true while the rates of the view are scaled: the game runs one Update at a time.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DecayableView), "Update")]
        internal static void Prefix(DecayableView __instance, out bool __state)
        {
            __state = false;
            var start = Plugin.Verbose.Value ? Stopwatch.GetTimestamp() : 0L;
            try
            {
                if (IsCarcass(__instance))
                {
                    var rate = __instance.decayRate;
                    var rainRate = __instance.rainDecayRate;
                    gameRate = rate.Value;
                    gameRainRate = rainRate.Value;
                    var multiplier = Plugin.AnimalSpoilage;
                    rate.Value = gameRate * multiplier;
                    rainRate.Value = gameRainRate * multiplier;
                    __state = true;
                    if (Plugin.Verbose.Value && logged.Add(__instance.Pointer))
                    {
                        Plugin.Debug(string.Format(CultureInfo.InvariantCulture, "Carcass: {0}, decay rate {1} set to {2}, rain decay rate {3} set to {4}",
                            __instance.entity?.identifier, gameRate, rate.Value, gameRainRate, rainRate.Value));
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.WarnOnce("Carcass", e);
            }
            Measure(start);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DecayableView), "Update")]
        internal static void Postfix(DecayableView __instance, bool __state)
        {
            if (!__state) return;
            var start = Plugin.Verbose.Value ? Stopwatch.GetTimestamp() : 0L;
            try
            {
                __instance.decayRate.Value = gameRate;
                __instance.rainDecayRate.Value = gameRainRate;
            }
            catch (Exception e)
            {
                Plugin.WarnOnce("Carcass", e);
            }
            Measure(start);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DecayableView), "OnDestroy")]
        internal static void DestroyPrefix(DecayableView __instance)
        {
            if (__instance == null) return;
            carcasses.Remove(__instance.Pointer);
            logged.Remove(__instance.Pointer);
        }

        // A view with no entity yet is not cached, so it is checked again in a later frame.
        private static bool IsCarcass(DecayableView view)
        {
            if (carcasses.TryGetValue(view.Pointer, out var carcass)) return carcass;
            var entity = view.entity;
            if (entity == null) return false;
            carcass = entity.GetCachedComponent<ICreature>(false) != null;
            carcasses[view.Pointer] = carcass;
            return carcass;
        }

        private static void Measure(long start)
        {
            if (start == 0L) return;
            var elapsed = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
            var line = timer.Add(elapsed, UnityEngine.Time.frameCount, UnityEngine.Time.realtimeSinceStartup);
            if (line != null) Plugin.Debug(line);
        }
    }
}
