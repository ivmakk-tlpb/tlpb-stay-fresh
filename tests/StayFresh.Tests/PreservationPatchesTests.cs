using StayFresh;
using StayFresh.Patches;
using Xunit;

public class PreservationPatchesTests : GameTestBase
{
    private static float Salt(float factor)
    {
        PreservationPatches.Prefix(ref factor);
        return factor;
    }

    [Fact]
    public void Salt_preservation_builds_by_the_preservation_multiplier()
    {
        Assert.Equal(0.5f, Salt(0.25f));

        Plugin.PreservationMultiplier.Value = 4f;
        Assert.Equal(1f, Salt(0.25f));
    }

    [Fact]
    public void A_factor_of_0_stays_0()
    {
        Assert.Equal(0f, Salt(0f));
    }

    [Fact]
    public void The_first_change_writes_one_verbose_line()
    {
        Plugin.Verbose.Value = true;

        Salt(0.25f);
        Salt(0.5f);

        Assert.Equal(new[] { "Salt preservation: factor 0.25 set to 0.5" }, Plugin.Log.Debugs.Where(l => l.StartsWith("Salt")).ToList());
    }
}

public class FermentationPatchesTests : GameTestBase
{
    private static float Ferment(float deltaTime)
    {
        FermentationPatches.Prefix(ref deltaTime);
        return deltaTime;
    }

    [Fact]
    public void Fermentation_runs_by_the_preservation_multiplier()
    {
        Assert.Equal(0.2f, Ferment(0.1f));

        Plugin.PreservationMultiplier.Value = 1f;
        Assert.Equal(0.1f, Ferment(0.1f));
    }

    [Fact]
    public void The_first_change_writes_one_verbose_line()
    {
        Plugin.Verbose.Value = true;

        Ferment(0.1f);
        Ferment(0.2f);

        Assert.Equal(new[] { "Fermentation: time step 0.1 set to 0.2" }, Plugin.Log.Debugs.Where(l => l.StartsWith("Fermentation")).ToList());
    }
}
