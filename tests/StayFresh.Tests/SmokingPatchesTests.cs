using StayFresh;
using StayFresh.Patches;
using Xunit;

public class SmokingPatchesTests : GameTestBase
{
    private static InventoryItem Item(string identifier, InventoryItem hardened = null, bool consumable = false)
    {
        var item = new InventoryItem(identifier) { hardenedItem = hardened };
        if (consumable) item.Tags.Add(ItemTag.CONSUMABLE);
        return item;
    }

    private static InventoryItemStack Stack(InventoryItem item, float contamination) =>
        new InventoryItemStack(item) { ContaminatedPercentage = contamination };

    private static readonly InventoryItem SmokedStrips = Item("meat_strip_smoked", consumable: true);
    private static InventoryItemStack RawStrips(float contamination) => Stack(Item("meat_strip_raw", SmokedStrips), contamination);

    private static void Harden(InventoryItemStack stack, SubstanceMixture mixture = null) =>
        SmokingPatches.Prefix(new ItemHardenedCommand { signalParams = new ItemHardenedSignalParams { hardenedStack = stack, hardenedMixture = mixture } });

    [Fact]
    public void Smoked_meat_strips_get_no_contamination()
    {
        GameWorld.Load("meat_strip_raw");
        var stack = RawStrips(0.2f);

        Harden(stack);

        Assert.Equal(0f, stack.ContaminatedPercentage);
    }

    [Fact]
    public void A_dried_bone_and_a_dried_flower_keep_their_value()
    {
        GameWorld.Load("bone", "chamomile_flower");
        var bone = Stack(Item("bone", Item("bone_dried")), 0.2f);
        var flower = Stack(Item("chamomile_flower", Item("chamomile_flower_dried", consumable: true)), 0.2f);

        Harden(bone);
        Harden(flower);

        Assert.Equal(0.2f, bone.ContaminatedPercentage);
        Assert.Equal(0.2f, flower.ContaminatedPercentage);
    }

    [Fact]
    public void An_item_that_hardens_into_itself_keeps_its_value()
    {
        GameWorld.Load("deer_meat_raw");
        var item = Item("deer_meat_raw", consumable: true);
        item.hardenedItem = item;
        var stack = Stack(item, 0.2f);

        Harden(stack);

        Assert.Equal(0.2f, stack.ContaminatedPercentage);
    }

    [Fact]
    public void The_hardening_of_a_liquid_keeps_its_value()
    {
        GameWorld.Load("meat_strip_raw");
        var stack = RawStrips(0.2f);

        Harden(stack, new SubstanceMixture());

        Assert.Equal(0.2f, stack.ContaminatedPercentage);
    }

    [Fact]
    public void With_the_setting_off_the_game_value_stays()
    {
        GameWorld.Load("meat_strip_raw");
        Plugin.CookingRemovesContamination.Value = false;
        var stack = RawStrips(0.2f);

        Harden(stack);

        Assert.Equal(0.2f, stack.ContaminatedPercentage);
    }

    [Fact]
    public void A_null_parameter_or_stack_causes_no_error()
    {
        GameWorld.Load("meat_strip_raw");

        SmokingPatches.Prefix(null);
        SmokingPatches.Prefix(new ItemHardenedCommand());
        Harden(null);
        Harden(new InventoryItemStack(null));

        Assert.Empty(Plugin.Log.Warnings);
    }

    [Fact]
    public void Before_the_game_loads_the_value_stays()
    {
        var stack = RawStrips(0.2f);

        Harden(stack);

        Assert.Equal(0.2f, stack.ContaminatedPercentage);
    }

    [Fact]
    public void The_first_change_of_an_item_writes_one_verbose_line()
    {
        GameWorld.Load("meat_strip_raw");
        Plugin.Verbose.Value = true;

        Harden(RawStrips(0.2f));
        Harden(RawStrips(0.4f));

        var lines = Plugin.Log.Debugs.Where(l => l.StartsWith("Smoking:")).ToList();
        Assert.Equal(new[] { "Smoking: meat_strip_raw into meat_strip_smoked, contamination 0.2 set to 0" }, lines);
    }

    [Fact]
    public void An_error_gives_one_warning()
    {
        GameWorld.Load("meat_strip_raw");
        var stack = RawStrips(0.2f);
        stack.Broken = true;

        Harden(stack);
        Harden(stack);

        Assert.Single(Plugin.Log.Warnings);
    }
}
