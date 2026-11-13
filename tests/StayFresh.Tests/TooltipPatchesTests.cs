using StayFresh;
using StayFresh.Patches;
using Xunit;

public class TooltipPatchesTests : GameTestBase
{
    // The expected times use the food factor 0.5, so each test starts at the slowdown 2.
    public TooltipPatchesTests() => Plugin.SpoilageSlowdown.Value = 2f;

    private static readonly World World = new World();

    // An entity with a holder and its inventory 0.
    private static Inventory Place(string entity, float holderFactor = 1f, World world = null)
    {
        var owner = new Entity { identifier = entity, world = world ?? World };
        owner.Components.Add(GameWorld.Holder(entity, holderFactor));
        return new Inventory { Owner = owner };
    }

    private static InventoryItemStack Stack(string item, float decayRate, Inventory inventory, float condition = 50f) =>
        new InventoryItemStack(new InventoryItem(item) { DecayRate = decayRate })
        {
            Condition = condition,
            slot = new InventorySlot { inventory = inventory },
        };

    // The inner inventory of a container stack that lies in the inventory.
    private static Inventory Inside(string container, Inventory inventory) =>
        new Inventory { parentStack = Stack(container, 0f, inventory) };

    private static List<string> Hover(InventoryItemStack stack)
    {
        var handler = new ShowTooltipEventHandler();
        TooltipPatches.Postfix(handler, new TooltipUI(), stack);
        return handler.Lines;
    }

    [Fact]
    public void Food_in_the_bag_shows_the_time_left()
    {
        GameWorld.Load("deer_meat_cooked");

        // 50 / (0.018 x 0.5) scaled seconds, 10 in-game seconds each.
        Assert.Equal(new[] { "Spoils in 15 hours" }, Hover(Stack("deer_meat_cooked", 0.018f, Place("player"))));
    }

    [Fact]
    public void With_the_spoilage_group_off_the_line_shows_the_game_rate()
    {
        FakeState.Reset();
        HarmonyLib.Harmony.ThrowFor = typeof(SpoilagePatches);
        new Plugin().Load();
        GameWorld.Load("deer_meat_cooked");

        // 50 / 0.018 scaled seconds, 10 in-game seconds each: 7.7 hours.
        Assert.Equal(new[] { "Spoils in 7 hours" }, Hover(Stack("deer_meat_cooked", 0.018f, Place("player"))));
    }

    [Fact]
    public void The_line_moves_right_after_the_weight_line()
    {
        GameWorld.Load("deer_meat_cooked");
        var tooltip = new TooltipUI();
        foreach (var text in new[] { "Cooked Deer Meat", "6<sprite=123>", "100% condition", "\n\nRight click for options" }) tooltip.AddLine(text);
        tooltip.AddLine("an old pooled line", shown: false);

        TooltipPatches.Postfix(new ShowTooltipEventHandler(), tooltip, Stack("deer_meat_cooked", 0.018f, Place("player")));

        Assert.Equal(new[] { "Cooked Deer Meat", "6<sprite=123>", "Spoils in 15 hours", "100% condition", "\n\nRight click for options" }, tooltip.Shown());
    }

    [Fact]
    public void The_holder_comes_from_the_holder_view_of_the_entity()
    {
        GameWorld.Load("deer_meat_cooked");
        var owner = new Entity { identifier = "player", world = World };
        owner.Components.Add(new InventoryHolderView { entityComponent = GameWorld.Holder("player") });

        Assert.Equal(new[] { "Spoils in 15 hours" }, Hover(Stack("deer_meat_cooked", 0.018f, new Inventory { Owner = owner })));
    }

    [Fact]
    public void Food_in_a_placed_covered_jar_shows_a_longer_time()
    {
        GameWorld.Load("deer_meat_cooked");
        var jar = Inside("clay_jar_covered", Place("clay_jar_covered", 0.3f));

        Assert.Equal(new[] { "Spoils in 4 days 6 hours" }, Hover(Stack("deer_meat_cooked", 0.018f, jar)));
    }

    [Fact]
    public void The_world_option_and_the_cold_count()
    {
        GameWorld.Load("deer_meat_cooked");
        var world = new World { temperature = 0f };
        world.game.gameOptions.advancedGameOptions = new AdvancedGameOptions { decayRateFactor = 2f };

        Assert.Equal(new[] { "Spoils in 1 day 14 hours" }, Hover(Stack("deer_meat_cooked", 0.018f, Place("player", 1f, world))));
    }

    [Fact]
    public void At_0_food_shows_no_line()
    {
        GameWorld.Load("deer_meat_cooked");
        Plugin.SpoilageSlowdown.Value = 0f;

        Assert.Empty(Hover(Stack("deer_meat_cooked", 0.018f, Place("player"))));
    }

    [Fact]
    public void A_stone_shows_no_line()
    {
        GameWorld.Load("stone");

        Assert.Empty(Hover(Stack("stone", 0f, Place("player"))));
    }

    [Fact]
    public void A_stack_with_no_holder_or_no_slot_shows_no_line()
    {
        GameWorld.Load("deer_meat_cooked");

        Assert.Empty(Hover(Stack("deer_meat_cooked", 0.018f, new Inventory())));
        Assert.Empty(Hover(new InventoryItemStack(new InventoryItem("deer_meat_cooked") { DecayRate = 0.018f }) { Condition = 50f }));
        Assert.Empty(Hover(null));
    }

    [Fact]
    public void With_verbose_on_a_missing_line_names_its_reason_once()
    {
        GameWorld.Load("deer_meat_cooked");
        Plugin.Verbose.Value = true;
        var noHolder = new Inventory { Owner = new Entity { identifier = "rock" } };

        Hover(Stack("deer_meat_cooked", 0.018f, new Inventory()));
        Hover(Stack("deer_meat_cooked", 0.018f, new Inventory()));
        Hover(Stack("deer_meat_cooked", 0.018f, noHolder));
        Hover(new InventoryItemStack(new InventoryItem("deer_meat_cooked") { DecayRate = 0.018f }));
        Hover(Stack("deer_meat_cooked", 0.018f, Place("player"), 0f));

        Assert.Equal(new[]
        {
            "Tooltip: no line for deer_meat_cooked: no owner of the root inventory",
            "Tooltip: no line for deer_meat_cooked: no holder in rock",
            "Tooltip: no line for deer_meat_cooked: no inventory",
            "Tooltip: no line for deer_meat_cooked: no spoilage at the moment",
        }, Plugin.Log.Debugs.Where(l => l.StartsWith("Tooltip: no line")).ToList());
    }

    [Fact]
    public void Before_the_game_loads_no_line_and_the_reason_with_verbose()
    {
        Plugin.Verbose.Value = true;

        Assert.Empty(Hover(Stack("deer_meat_cooked", 0.018f, Place("player"))));
        Assert.Contains("Tooltip: no line for deer_meat_cooked: the game data is not loaded", Plugin.Log.Debugs);
    }

    [Fact]
    public void Before_the_game_loads_no_line()
    {
        Assert.Empty(Hover(Stack("deer_meat_cooked", 0.018f, Place("player"))));
    }

    [Fact]
    public void Each_tooltip_writes_one_verbose_line_with_the_factors()
    {
        GameWorld.Load("deer_meat_cooked");
        Plugin.Verbose.Value = true;
        var stack = Stack("deer_meat_cooked", 0.018f, Place("player"));

        Hover(stack);
        Hover(stack);

        var lines = Plugin.Log.Debugs.Where(l => l.StartsWith("Tooltip:")).ToList();
        Assert.Equal(2, lines.Count);
        Assert.Equal("Tooltip: deer_meat_cooked, condition 50, decay rate 0.018, preserved 0, game factor 1 (holder 1, option 1, cold 1), mod factor 0.5, Spoils in 15 hours", lines[0]);
    }

    [Fact]
    public void An_error_gives_one_warning()
    {
        GameWorld.Load("deer_meat_cooked");
        var stack = Stack("deer_meat_cooked", 0.018f, Place("player"));
        stack.Broken = true;

        Hover(stack);
        Hover(stack);

        Assert.Single(Plugin.Log.Warnings);
    }
}
