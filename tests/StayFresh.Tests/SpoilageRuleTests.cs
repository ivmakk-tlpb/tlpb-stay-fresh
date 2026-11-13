using StayFresh;
using Xunit;

public class SpoilageRuleTests
{
    private static SpoilageRule Rule() => new SpoilageRule(
        AnimalProducts.From(new[] { new AnimalProducts.CreatureDrops(new[] { "deer_hide" }, null) }, new[] { "deer_meat_raw" }),
        spoilage: 0.8f,
        animal: 0.25f);

    [Theory]
    [InlineData("deer_hide")]
    [InlineData("deer_meat_raw")]
    public void An_animal_product_gets_the_animal_multiplier(string item)
    {
        Assert.Equal(0.25f, Rule().Multiplier(item));
    }

    [Theory]
    [InlineData("deer_meat_cooked")]
    [InlineData("meat_strip_smoked")]
    public void Any_other_item_gets_the_spoilage_multiplier(string item)
    {
        Assert.Equal(0.8f, Rule().Multiplier(item));
    }

    [Fact]
    public void A_container_with_a_liquid_gets_the_multiplier_of_the_container_item()
    {
        // The liquid spoils through the stack of its container, so the rule sees the container item.
        Assert.Equal(0.8f, Rule().Multiplier("clay_jar"));
    }
}
