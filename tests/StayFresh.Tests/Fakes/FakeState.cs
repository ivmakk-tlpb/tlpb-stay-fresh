using HarmonyLib;

// Puts the static state of the fake game types back to a known start.
internal static class FakeState
{
    public static void Reset()
    {
        UnityEngine.Object.Scene.Clear();
        UnityEngine.Time.frameCount = 0;
        UnityEngine.Time.realtimeSinceStartup = 0f;
        StayFresh.Patches.SpoilagePatches.Reset();
        StayFresh.Patches.CookingPatches.Reset();
        StayFresh.Patches.SmokingPatches.Reset();
        StayFresh.Patches.PreservationPatches.Reset();
        StayFresh.Patches.FermentationPatches.Reset();
        StayFresh.Patches.TooltipPatches.Reset();
        StayFresh.Patches.CarcassPatches.Reset();
        Harmony.Patched.Clear();
        Harmony.Unpatched.Clear();
        Harmony.ThrowFor = null;
    }
}
