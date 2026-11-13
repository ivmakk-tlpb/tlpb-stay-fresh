using StayFresh;
using Xunit;

public class AnimalProductsTests
{
    private static readonly string[] Items =
    {
        "deer_meat_raw", "deer_meat_cooked", "meat_strip_raw", "meat_strip_smoked", "sausage_raw", "sausage_cured",
        "deer_hide", "animal_fat", "rabbit_item_dead", "small_stone",
    };

    private static AnimalProducts Deer() => AnimalProducts.From(
        new[]
        {
            new AnimalProducts.CreatureDrops(new[] { "deer_meat_raw", "deer_hide", "animal_intestines", "animal_fat" }, null),
            new AnimalProducts.CreatureDrops(null, "rabbit_item_dead"),
        },
        Items);

    [Theory]
    [InlineData("deer_hide")]
    [InlineData("animal_fat")]
    [InlineData("animal_intestines")]
    [InlineData("rabbit_item_dead")]
    [InlineData("deer_meat_raw")]
    [InlineData("meat_strip_raw")]
    [InlineData("sausage_raw")]
    public void Harvest_drops_dead_pickups_and_raw_items_are_animal_products(string item)
    {
        Assert.True(Deer().Contains(item));
    }

    [Theory]
    [InlineData("deer_meat_cooked")]
    [InlineData("meat_strip_smoked")]
    [InlineData("sausage_cured")]
    [InlineData("small_stone")]
    [InlineData(null)]
    public void Cooked_smoked_cured_and_other_items_are_not(string item)
    {
        Assert.False(Deer().Contains(item));
    }

    [Fact]
    public void An_empty_creature_list_gives_only_the_raw_items()
    {
        var products = AnimalProducts.From(new AnimalProducts.CreatureDrops[0], Items);

        Assert.True(products.Contains("deer_meat_raw"));
        Assert.False(products.Contains("deer_hide"));
        Assert.Equal(3, products.Count);
    }
}
