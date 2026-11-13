using StayFresh;
using StayFresh.Patches;
using Xunit;

public class SpoilagePatchesTests : GameTestBase
{
    // The expected values of the mechanics tests use the food factor 0.5, so each test starts at the slowdown 2.
    public SpoilagePatchesTests() => Plugin.SpoilageSlowdown.Value = 2f;

    private static float Run(InventoryHolder holder, InventoryItemStack stack, float decayFactor = 1f, Inventory inventory = null)
    {
        SpoilagePatches.Prefix(holder, stack, inventory ?? new Inventory(), ref decayFactor);
        return decayFactor;
    }

    [Fact]
    public void Food_in_a_placed_covered_jar_spoils_by_1_over_the_preservation_multiplier_more()
    {
        GameWorld.Load("deer_meat_cooked");

        Assert.Equal(0.25f, Run(GameWorld.Holder("clay_jar_covered", 0.3f), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("clay_jar_covered")), 6);
    }

    [Fact]
    public void Food_in_a_covered_jar_in_the_bag_gets_the_jar_factor_over_the_preservation_multiplier()
    {
        GameWorld.Load("deer_meat_cooked");

        Assert.Equal(0.075f, Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("clay_jar_covered")), 6);
        Assert.Equal(0.075f, Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("glass_bottle_covered")), 6);
    }

    [Fact]
    public void Food_in_a_container_with_no_content_factor_gets_only_the_item_multiplier()
    {
        GameWorld.Load("deer_meat_cooked", "backpack");

        Assert.Equal(0.5f, Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("backpack")));
    }

    [Fact]
    public void The_holder_and_the_parent_are_read_once_each_frame()
    {
        GameWorld.Load("deer_meat_cooked");
        var holder = GameWorld.Holder("player");
        var inside = GameWorld.Inside("clay_jar_covered");
        UnityEngine.Time.frameCount = 1;

        Run(holder, Stack("deer_meat_cooked"), 1f, inside);
        Run(holder, Stack("deer_meat_cooked"), 1f, inside);
        Assert.Equal(1, holder.contentsDecayFactor.Reads);
        Assert.Equal(1, inside.parentStack.Item.IdentifierReads);

        UnityEngine.Time.frameCount = 2;
        Run(holder, Stack("deer_meat_cooked"), 1f, inside);
        Assert.Equal(2, holder.contentsDecayFactor.Reads);
    }

    [Fact]
    public void An_error_in_the_item_definitions_keeps_the_item_multiplier_with_one_warning()
    {
        var manager = GameWorld.Load("deer_meat_cooked");
        var broken = GameWorld.Definition(("identifier", "broken"));
        broken.Broken = true;
        ((ModelDefinitionManager)manager.modelDefinitionManager).Items.Add("broken", broken);

        Assert.Equal(0.5f, Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("clay_jar_covered")));
        Assert.Single(Plugin.Log.Warnings);
        Assert.StartsWith("Item definitions: ArgumentException", Plugin.Log.Warnings[0]);
    }

    [Fact]
    public void A_null_inventory_counts_as_no_parent()
    {
        GameWorld.Load("deer_meat_cooked");
        float factor = 1f;

        SpoilagePatches.Prefix(GameWorld.Holder("player"), Stack("deer_meat_cooked"), null, ref factor);

        Assert.Equal(0.5f, factor);
    }

    [Fact]
    public void The_first_covered_container_case_writes_one_verbose_line()
    {
        GameWorld.Load("deer_meat_cooked");
        Plugin.Verbose.Value = true;

        Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("clay_jar_covered"));
        Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("clay_jar_covered"));
        Run(GameWorld.Holder("clay_jar_covered", 0.3f), Stack("deer_meat_cooked"), 1f, GameWorld.Inside("clay_jar_covered"));

        var lines = Plugin.Log.Debugs.Where(l => l.StartsWith("Covered container:")).ToList();
        Assert.Equal(new[]
        {
            "Covered container: clay_jar_covered in player, holder factor 1, mod factor 0.15",
            "Covered container: clay_jar_covered in clay_jar_covered, holder factor 0.3, mod factor 0.5",
        }, lines);
    }

    private static InventoryItemStack Stack(string item) => new InventoryItemStack(new InventoryItem(item));

    [Fact]
    public void Food_spoils_by_the_spoilage_multiplier()
    {
        GameWorld.Load("deer_meat_cooked");

        Assert.Equal(0.4f, Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 0.8f));
    }

    [Fact]
    public void An_animal_product_spoils_by_the_animal_multiplier()
    {
        GameWorld.Load("deer_hide");
        Plugin.AnimalSpoilageSlowdown.Value = 4f;

        Assert.Equal(0.25f, Run(GameWorld.Holder("wooden_chest"), Stack("deer_hide")));
    }

    [Fact]
    public void At_0_food_does_not_spoil()
    {
        GameWorld.Load("deer_meat_cooked");
        Plugin.SpoilageSlowdown.Value = 0f;

        Assert.Equal(0f, Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"), 0.8f));
    }

    [Fact]
    public void At_0_an_animal_product_does_not_spoil()
    {
        GameWorld.Load("deer_meat_raw");
        Plugin.AnimalSpoilageSlowdown.Value = 0f;

        Assert.Equal(0f, Run(GameWorld.Holder("deer_doe"), Stack("deer_meat_raw")));
    }

    [Fact]
    public void An_animal_product_spoils_by_the_default_animal_multiplier()
    {
        GameWorld.Load("deer_hide");

        Assert.Equal(0.2f, Run(GameWorld.Holder("wooden_chest"), Stack("deer_hide")), 6);
    }

    [Fact]
    public void The_factor_is_cached_by_the_item_so_the_identifier_is_read_once()
    {
        GameWorld.Load("deer_meat_cooked");
        var stack = Stack("deer_meat_cooked");

        Run(GameWorld.Holder("player"), stack);
        Run(GameWorld.Holder("player"), stack);

        Assert.Equal(1, stack.Item.IdentifierReads);
    }

    [Fact]
    public void A_null_stack_or_item_keeps_the_factor()
    {
        GameWorld.Load();

        Assert.Equal(0.7f, Run(GameWorld.Holder("player"), null, 0.7f));
        Assert.Equal(0.7f, Run(GameWorld.Holder("player"), new InventoryItemStack(null), 0.7f));
    }

    [Fact]
    public void Before_the_game_loads_the_factor_stays_and_nothing_is_cached()
    {
        var stack = Stack("deer_meat_cooked");
        Assert.Equal(1f, Run(GameWorld.Holder("player"), stack));

        GameWorld.Load("deer_meat_cooked");

        Assert.Equal(0.5f, Run(GameWorld.Holder("player"), stack));
    }

    [Fact]
    public void An_error_keeps_the_factor_and_gives_one_warning()
    {
        GameWorld.Load();
        var stack = Stack("deer_meat_cooked");
        stack.Broken = true;

        Assert.Equal(1f, Run(GameWorld.Holder("player"), stack));
        Run(GameWorld.Holder("player"), stack);

        Assert.Single(Plugin.Log.Warnings);
    }

    [Fact]
    public void The_first_change_of_each_item_writes_one_verbose_line()
    {
        GameWorld.Load("deer_meat_cooked");
        Plugin.Verbose.Value = true;

        Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"));
        Run(GameWorld.Holder("wooden_chest"), Stack("deer_meat_cooked"));

        var lines = Plugin.Log.Debugs.Where(l => l.StartsWith("Spoilage:")).ToList();
        Assert.Equal(new[] { "Spoilage: deer_meat_cooked (item) in player, game factor 1, mod factor 0.5" }, lines);
    }

    [Fact]
    public void The_verbose_line_names_an_animal_product()
    {
        GameWorld.Load("deer_hide");
        Plugin.Verbose.Value = true;
        Plugin.AnimalSpoilageSlowdown.Value = 2f;

        Run(GameWorld.Holder("wooden_chest"), Stack("deer_hide"));

        Assert.Contains("Spoilage: deer_hide (animal product) in wooden_chest, game factor 1, mod factor 0.5", Plugin.Log.Debugs);
    }

    [Fact]
    public void With_verbose_on_the_prefix_writes_its_mean_time_every_10_seconds()
    {
        GameWorld.Load("deer_meat_cooked");
        Plugin.Verbose.Value = true;
        var stack = Stack("deer_meat_cooked");

        UnityEngine.Time.frameCount = 1;
        Run(GameWorld.Holder("player"), stack);
        UnityEngine.Time.frameCount = 2;
        UnityEngine.Time.realtimeSinceStartup = 10f;
        Run(GameWorld.Holder("player"), stack);

        Assert.Contains(Plugin.Log.Debugs, l => l.StartsWith("Spoilage prefix: ") && l.EndsWith("ms per frame over 2 frames"));
    }

    [Fact]
    public void With_verbose_off_no_time_line()
    {
        GameWorld.Load("deer_meat_cooked");
        UnityEngine.Time.realtimeSinceStartup = 10f;

        Run(GameWorld.Holder("player"), Stack("deer_meat_cooked"));

        Assert.Empty(Plugin.Log.Debugs);
    }
}
