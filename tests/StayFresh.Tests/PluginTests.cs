using BepInEx.Configuration;
using StayFresh;
using Xunit;

public class PluginTests : GameTestBase
{
    [Fact]
    public void The_guid_names_the_mod()
    {
        Assert.Equal("com.ivmakk.thelastplagueblight.stayfresh", Plugin.PluginGuid);
    }

    [Fact]
    public void Cooking_removes_contamination_by_default_and_debug_lines_are_off()
    {
        Assert.True(Plugin.CookingRemovesContamination.Value);
        Assert.False(Plugin.Verbose.Value);
    }

    [Theory]
    [InlineData("SpoilageSlowdown", 5f, 0f, 20f)]
    [InlineData("AnimalSpoilageSlowdown", 5f, 0f, 20f)]
    [InlineData("PreservationMultiplier", 2f, 1f, 10f)]
    public void A_setting_has_its_default_and_range(string name, float value, float min, float max)
    {
        var entry = Entry(name);
        Assert.Equal(value, entry.Value);
        var range = (AcceptableValueRange<float>)entry.Description.AcceptableValues;
        Assert.Equal(min, range.MinValue);
        Assert.Equal(max, range.MaxValue);
    }

    [Theory]
    [InlineData(-1f, 0f)]
    [InlineData(0f, 0f)]
    [InlineData(0.5f, 1f)]
    [InlineData(1f, 1f)]
    [InlineData(2f, 0.5f)]
    [InlineData(5f, 0.2f)]
    [InlineData(20f, 0.05f)]
    [InlineData(50f, 0.05f)]
    public void The_spoilage_slowdown_is_read_as_a_factor(float value, float factor)
    {
        Plugin.SpoilageSlowdown.Value = value;
        Assert.Equal(factor, Plugin.Spoilage, 6);
    }

    [Theory]
    [InlineData(-1f, 0f)]
    [InlineData(0f, 0f)]
    [InlineData(0.01f, 1f)]
    [InlineData(0.5f, 1f)]
    [InlineData(1f, 1f)]
    [InlineData(4f, 0.25f)]
    [InlineData(5f, 0.2f)]
    [InlineData(20f, 0.05f)]
    [InlineData(50f, 0.05f)]
    public void The_animal_spoilage_slowdown_is_read_as_a_factor(float value, float factor)
    {
        Plugin.AnimalSpoilageSlowdown.Value = value;
        Assert.Equal(factor, Plugin.AnimalSpoilage, 6);
    }

    [Theory]
    [InlineData(0.5f, 1f)]
    [InlineData(4f, 4f)]
    [InlineData(50f, 10f)]
    public void The_preservation_multiplier_is_limited_when_it_is_read(float value, float read)
    {
        Plugin.PreservationMultiplier.Value = value;
        Assert.Equal(read, Plugin.Preservation);
    }

    [Fact]
    public void The_debug_helper_writes_only_while_verbose_is_on()
    {
        Plugin.Debug("hidden");
        Plugin.Verbose.Value = true;
        Plugin.Debug("shown");

        Assert.Equal(new[] { "shown" }, Plugin.Log.Debugs);
    }

    [Fact]
    public void The_plugin_logs_that_it_loaded_with_no_warning()
    {
        Assert.Contains("Stay Fresh loaded.", Plugin.Log.Infos);
        Assert.Empty(Plugin.Log.Warnings);
    }

    [Fact]
    public void The_mod_has_four_patch_groups()
    {
        Assert.Equal(new[] { "cooking", "spoilage", "carcass", "tooltip" }, Plugin.Groups.Select(g => g.Name));
    }

    [Fact]
    public void A_repeated_error_gives_one_warning()
    {
        Plugin.WarnOnce("Part", new InvalidOperationException("broken"));
        Plugin.WarnOnce("Part", new InvalidOperationException("broken"));

        Assert.Single(Plugin.Log.Warnings);
    }

    private static ConfigEntry<float> Entry(string name)
    {
        return name switch
        {
            "SpoilageSlowdown" => Plugin.SpoilageSlowdown,
            "AnimalSpoilageSlowdown" => Plugin.AnimalSpoilageSlowdown,
            _ => Plugin.PreservationMultiplier,
        };
    }
}

public class PluginGroupTests : GameTestBase
{
    [Fact]
    public void The_spoilage_patches_are_in_the_spoilage_group()
    {
        Assert.Contains(typeof(StayFresh.Patches.SpoilagePatches), Plugin.Groups.Single(g => g.Name == "spoilage").Types);
        Assert.Contains(typeof(StayFresh.Patches.SpoilagePatches), HarmonyLib.Harmony.Patched);
    }

    [Fact]
    public void The_cooking_patches_are_in_the_cooking_group()
    {
        Assert.Contains(typeof(StayFresh.Patches.CookingPatches), Plugin.Groups.Single(g => g.Name == "cooking").Types);
        Assert.Contains(typeof(StayFresh.Patches.CookingPatches), HarmonyLib.Harmony.Patched);
    }

    [Fact]
    public void The_preservation_and_fermentation_patches_are_in_the_spoilage_group()
    {
        var types = Plugin.Groups.Single(g => g.Name == "spoilage").Types;
        Assert.Contains(typeof(StayFresh.Patches.PreservationPatches), types);
        Assert.Contains(typeof(StayFresh.Patches.FermentationPatches), types);
    }

    [Fact]
    public void The_smoking_patches_are_in_the_cooking_group()
    {
        Assert.Contains(typeof(StayFresh.Patches.SmokingPatches), Plugin.Groups.Single(g => g.Name == "cooking").Types);
        Assert.Contains(typeof(StayFresh.Patches.SmokingPatches), HarmonyLib.Harmony.Patched);
    }

    [Fact]
    public void The_carcass_patches_are_in_the_carcass_group()
    {
        Assert.Contains(typeof(StayFresh.Patches.CarcassPatches), Plugin.Groups.Single(g => g.Name == "carcass").Types);
    }

    [Fact]
    public void The_tooltip_patches_are_in_the_tooltip_group()
    {
        Assert.Contains(typeof(StayFresh.Patches.TooltipPatches), Plugin.Groups.Single(g => g.Name == "tooltip").Types);
    }

    [Fact]
    public void A_missing_tooltip_target_removes_only_that_group_with_one_warning()
    {
        FakeState.Reset();
        HarmonyLib.Harmony.ThrowFor = typeof(StayFresh.Patches.TooltipPatches);
        new Plugin().Load();

        Assert.Equal(new[] { Plugin.PluginGuid + ".tooltip" }, HarmonyLib.Harmony.Unpatched);
        Assert.Equal(new[] { "Stay Fresh shows no spoilage time in the tooltip: Target not found" }, Plugin.Log.Warnings);
        Assert.Contains(typeof(StayFresh.Patches.SpoilagePatches), HarmonyLib.Harmony.Patched);
    }

    [Fact]
    public void A_missing_spoilage_target_removes_only_that_group_with_one_warning()
    {
        FakeState.Reset();
        HarmonyLib.Harmony.ThrowFor = typeof(StayFresh.Patches.SpoilagePatches);
        new Plugin().Load();

        Assert.Equal(new[] { Plugin.PluginGuid + ".spoilage" }, HarmonyLib.Harmony.Unpatched);
        Assert.Equal(new[] { "Stay Fresh leaves the spoilage of items as in the game: Target not found" }, Plugin.Log.Warnings);
    }
}
