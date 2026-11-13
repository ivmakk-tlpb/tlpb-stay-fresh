using StayFresh;
using StayFresh.Patches;
using Xunit;

public class CarcassPatchesTests : GameTestBase
{
    // The expected values of the mechanics tests use the factor 0.5, so each test starts at the slowdown 2.
    public CarcassPatchesTests() => Plugin.AnimalSpoilageSlowdown.Value = 2f;

    [Fact]
    public void At_0_a_carcass_does_not_decay_and_its_rates_come_back_after_the_frame()
    {
        Plugin.AnimalSpoilageSlowdown.Value = 0f;
        var view = View("deer_doe", true);

        CarcassPatches.Prefix(view, out var scaled);
        Assert.Equal(0f, view.decayRate.Value);
        Assert.Equal(0f, view.rainDecayRate.Value);
        CarcassPatches.Postfix(view, scaled);

        Assert.Equal(0.0001f, view.decayRate.Value);
        Assert.Equal(0.0002f, view.rainDecayRate.Value);
        Assert.Equal(0.2f, Step(View("deer_doe", true), 1000f), 6);
    }

    private static DecayableView View(string entity, bool creature, float amount = 0.2f)
    {
        var owner = new Entity { identifier = entity };
        if (creature) owner.Components.Add(new ICreature());
        var view = new DecayableView { entity = owner };
        view.SetDecayAmount(amount);
        return view;
    }

    // One frame of the game: the prefix, the Update of the game, and the postfix.
    private static float Step(DecayableView view, float scaledSeconds, float wetness = 0f)
    {
        CarcassPatches.Prefix(view, out var scaled);
        view.Update(scaledSeconds, wetness);
        CarcassPatches.Postfix(view, scaled);
        return view.GetDecayAmount();
    }

    [Fact]
    public void A_carcass_decays_by_the_animal_multiplier()
    {
        Assert.Equal(0.25f, Step(View("deer_doe", true), 1000f), 6);

        Plugin.AnimalSpoilageSlowdown.Value = 5f;
        Assert.Equal(0.22f, Step(View("deer_doe", true), 1000f), 6);
    }

    [Fact]
    public void The_rain_part_of_the_step_is_scaled_too()
    {
        // 1000 x (0.0001 + 0.5 x 0.0002) x 0.5 = 0.1.
        Assert.Equal(0.3f, Step(View("deer_doe", true), 1000f, 0.5f), 6);
    }

    [Fact]
    public void The_rates_of_the_view_are_back_after_the_frame()
    {
        var view = View("deer_doe", true);

        Step(view, 1000f);

        Assert.Equal(0.0001f, view.decayRate.Value);
        Assert.Equal(0.0002f, view.rainDecayRate.Value);
    }

    [Fact]
    public void A_catch_up_step_that_the_game_rate_would_end_leaves_the_carcass_when_the_scaled_step_does_not()
    {
        // The game rate gives 0.98 + 0.03 and the fully decayed signal; the scaled rate gives 0.995.
        var view = View("deer_doe", true, 0.98f);

        Assert.Equal(0.995f, Step(view, 300f), 5);
        Assert.Equal(0, view.FullyDecayedSignals);
    }

    [Fact]
    public void A_scaled_step_that_reaches_1_ends_the_carcass_once()
    {
        var view = View("deer_doe", true, 0.98f);

        Assert.Equal(1f, Step(view, 500f));
        Assert.Equal(1, view.FullyDecayedSignals);
    }

    [Fact]
    public void A_view_with_no_creature_keeps_the_game_step()
    {
        Assert.Equal(0.3f, Step(View("dead_bush", false), 1000f), 6);
    }

    [Fact]
    public void A_view_with_no_entity_keeps_the_game_step_and_is_checked_again()
    {
        var view = new DecayableView();
        view.SetDecayAmount(0.2f);
        Assert.Equal(0.3f, Step(view, 1000f), 6);

        view.entity = new Entity { identifier = "deer_doe" };
        view.entity.Components.Add(new ICreature());
        Assert.Equal(0.35f, Step(view, 1000f), 6);
    }

    [Fact]
    public void The_creature_check_runs_once_for_each_view_and_the_entry_goes_when_the_view_is_destroyed()
    {
        var view = View("deer_doe", true);
        Step(view, 1000f);
        view.entity.Components.Clear();

        Assert.Equal(0.3f, Step(view, 1000f), 6);

        CarcassPatches.DestroyPrefix(view);
        Assert.Equal(0.4f, Step(view, 1000f), 6);
    }

    [Fact]
    public void An_error_keeps_the_game_step_and_gives_one_warning()
    {
        var view = View("deer_doe", true);
        view.rainDecayRate = null;

        CarcassPatches.Prefix(view, out var scaled);
        CarcassPatches.Postfix(view, scaled);
        CarcassPatches.Prefix(view, out _);

        Assert.False(scaled);
        Assert.Equal(0.0001f, view.decayRate.Value);
        Assert.Single(Plugin.Log.Warnings);
    }

    [Fact]
    public void Each_carcass_writes_one_verbose_line_with_the_rates()
    {
        Plugin.Verbose.Value = true;
        var doe = View("deer_doe", true);

        Step(doe, 1f);
        Step(doe, 1f);
        Step(View("deer_stag", true), 1f);

        var lines = Plugin.Log.Debugs.Where(l => l.StartsWith("Carcass:")).ToList();
        Assert.Equal(new[]
        {
            "Carcass: deer_doe, decay rate 0.0001 set to 5E-05, rain decay rate 0.0002 set to 0.0001",
            "Carcass: deer_stag, decay rate 0.0001 set to 5E-05, rain decay rate 0.0002 set to 0.0001",
        }, lines);
    }

    [Fact]
    public void With_verbose_on_the_patch_writes_its_mean_time_every_10_seconds()
    {
        Plugin.Verbose.Value = true;
        var view = View("deer_doe", true);

        UnityEngine.Time.frameCount = 1;
        Step(view, 1f);
        UnityEngine.Time.frameCount = 2;
        UnityEngine.Time.realtimeSinceStartup = 10f;
        Step(view, 1f);

        Assert.Contains(Plugin.Log.Debugs, l => l.StartsWith("Carcass patch: ") && l.EndsWith("ms per frame over 2 frames"));
    }
}
