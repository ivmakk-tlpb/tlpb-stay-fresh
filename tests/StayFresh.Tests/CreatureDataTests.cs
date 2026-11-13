using StayFresh.Game;
using Xunit;

public class CreatureDataTests : GameTestBase
{
    [Fact]
    public void It_reads_the_harvest_drops_the_dead_pickups_and_the_raw_items_of_the_game()
    {
        GameWorld.Load("deer_meat_raw", "meat_strip_raw", "deer_meat_cooked");

        var products = CreatureData.Load();

        Assert.True(products.Contains("deer_hide"));
        Assert.True(products.Contains("animal_fat"));
        Assert.True(products.Contains("rabbit_item_dead"));
        Assert.True(products.Contains("meat_strip_raw"));
        Assert.False(products.Contains("deer_meat_cooked"));
    }

    [Fact]
    public void Before_the_game_loads_it_gives_null()
    {
        Assert.Null(CreatureData.Load());
    }
}
