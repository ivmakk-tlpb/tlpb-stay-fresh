using StayFresh;
using StayFresh.Patches;
using Xunit;

public class CookingPatchesTests : GameTestBase
{
    private static readonly CookingRecipe Steak = new CookingRecipe("cooked_wolf_meat", new CraftingIngredient { itemIdentifier = "wolf_meat_cooked" });
    private static readonly CookingRecipe BoiledWater = new CookingRecipe("boiled_water", new CraftingIngredient { substanceIdentifier = "potable_water" });

    private static CookingRecipeResult Result(float contamination) => new CookingRecipeResult { contaminatedPercentage = contamination };

    private static CookingCompleteEffect Cooking(CookingRecipe recipe, SubstanceMixture vessel)
    {
        var cooker = new Entity { identifier = "fire_pit" };
        cooker.Components.Add(new ICooker { Mixture = vessel });
        return new CookingCompleteEffect { cookerEntity = cooker, recipe = recipe };
    }

    [Fact]
    public void The_cooking_result_has_no_contamination()
    {
        var result = Result(0.1f);

        CookingPatches.ResultPostfix(Steak, result);

        Assert.Equal(0f, result.contaminatedPercentage);
    }

    [Fact]
    public void With_the_setting_off_the_result_keeps_the_game_value()
    {
        Plugin.CookingRemovesContamination.Value = false;
        var result = Result(0.1f);

        CookingPatches.ResultPostfix(Steak, result);

        Assert.Equal(0.1f, result.contaminatedPercentage);
    }

    [Fact]
    public void A_null_result_or_recipe_causes_no_error()
    {
        CookingPatches.ResultPostfix(Steak, null);
        CookingPatches.ResultPostfix(null, Result(0.1f));

        Assert.Empty(Plugin.Log.Warnings);
    }

    [Fact]
    public void The_first_change_of_a_recipe_writes_one_verbose_line_with_the_old_value()
    {
        Plugin.Verbose.Value = true;

        CookingPatches.ResultPostfix(Steak, Result(0.1f));
        CookingPatches.ResultPostfix(Steak, Result(0.2f));

        var lines = Plugin.Log.Debugs.Where(l => l.StartsWith("Cooking:")).ToList();
        Assert.Equal(new[] { "Cooking: cooked_wolf_meat, contamination 0.1 set to 0" }, lines);
    }

    [Fact]
    public void A_result_with_no_contamination_writes_no_line()
    {
        Plugin.Verbose.Value = true;

        CookingPatches.ResultPostfix(Steak, Result(0f));

        Assert.DoesNotContain(Plugin.Log.Debugs, l => l.StartsWith("Cooking:"));
    }

    [Fact]
    public void A_liquid_result_in_the_vessel_has_no_contamination()
    {
        var vessel = new SubstanceMixture { contaminatedPercentage = 0.3f };

        CookingPatches.CompletePostfix(Cooking(BoiledWater, vessel));

        Assert.Equal(0f, vessel.contaminatedPercentage);
    }

    [Fact]
    public void A_recipe_with_item_outputs_only_leaves_the_vessel()
    {
        var vessel = new SubstanceMixture { contaminatedPercentage = 0.3f };

        CookingPatches.CompletePostfix(Cooking(Steak, vessel));

        Assert.Equal(0.3f, vessel.contaminatedPercentage);
    }

    [Fact]
    public void With_the_setting_off_the_vessel_keeps_the_game_value()
    {
        Plugin.CookingRemovesContamination.Value = false;
        var vessel = new SubstanceMixture { contaminatedPercentage = 0.3f };

        CookingPatches.CompletePostfix(Cooking(BoiledWater, vessel));

        Assert.Equal(0.3f, vessel.contaminatedPercentage);
    }

    [Fact]
    public void A_cooking_with_no_cooker_or_no_vessel_causes_no_error()
    {
        CookingPatches.CompletePostfix(null);
        CookingPatches.CompletePostfix(new CookingCompleteEffect { recipe = BoiledWater });
        CookingPatches.CompletePostfix(new CookingCompleteEffect { cookerEntity = new Entity(), recipe = BoiledWater });
        CookingPatches.CompletePostfix(Cooking(BoiledWater, null));
        CookingPatches.CompletePostfix(Cooking(null, new SubstanceMixture()));

        Assert.Empty(Plugin.Log.Warnings);
    }

    [Fact]
    public void The_first_liquid_change_of_a_recipe_writes_one_verbose_line()
    {
        Plugin.Verbose.Value = true;

        CookingPatches.CompletePostfix(Cooking(BoiledWater, new SubstanceMixture { contaminatedPercentage = 0.3f }));
        CookingPatches.CompletePostfix(Cooking(BoiledWater, new SubstanceMixture { contaminatedPercentage = 0.3f }));

        var lines = Plugin.Log.Debugs.Where(l => l.StartsWith("Cooking:")).ToList();
        Assert.Equal(new[] { "Cooking: boiled_water in fire_pit, liquid contamination 0.3 set to 0" }, lines);
    }

    [Fact]
    public void An_error_gives_one_warning()
    {
        var recipe = new CookingRecipe("broken") { Broken = true };

        CookingPatches.CompletePostfix(Cooking(recipe, new SubstanceMixture()));
        CookingPatches.CompletePostfix(Cooking(recipe, new SubstanceMixture()));

        Assert.Single(Plugin.Log.Warnings);
    }
}
