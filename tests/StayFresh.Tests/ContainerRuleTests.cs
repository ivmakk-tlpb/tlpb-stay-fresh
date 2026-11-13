using StayFresh;
using Xunit;

public class ContainerRuleTests
{
    private static ContainerRule Rule(float preservation = 2f) => new ContainerRule(new Dictionary<string, float>
    {
        ["clay_jar_covered"] = 0.3f,
        ["glass_bottle_covered"] = 0.3f,
        ["backpack"] = 1f,
    }, preservation);

    [Fact]
    public void A_holder_factor_below_1_gives_1_over_the_multiplier()
    {
        Assert.Equal(0.5f, Rule().Factor(0.3f, "clay_jar_covered"));
        Assert.Equal(0.25f, Rule(4f).Factor(0.3f, null));
    }

    [Fact]
    public void A_parent_covered_container_gives_its_factor_over_the_multiplier()
    {
        Assert.Equal(0.15f, Rule().Factor(1f, "clay_jar_covered"), 6);
    }

    [Fact]
    public void A_parent_with_factor_1_and_no_parent_give_1()
    {
        Assert.Equal(1f, Rule().Factor(1f, "backpack"));
        Assert.Equal(1f, Rule().Factor(1f, null));
        Assert.Equal(1f, Rule().Factor(1f, "unknown_item"));
    }

    [Fact]
    public void With_the_multiplier_1_a_covered_container_in_the_bag_counts_as_in_the_game_world()
    {
        Assert.Equal(1f, Rule(1f).Factor(0.3f, "clay_jar_covered"));
        Assert.Equal(0.3f, Rule(1f).Factor(1f, "clay_jar_covered"));
    }

    [Fact]
    public void The_content_factor_of_an_item_follows_copy_from_and_a_missing_key_gives_1()
    {
        var factors = ContainerRule.ContentFactors(new[]
        {
            new ContainerRule.ItemRow("clay_jar_covered", null, 0.3f),
            new ContainerRule.ItemRow("clay_jar_covered_painted", "clay_jar_covered", null),
            new ContainerRule.ItemRow("chain_b", "chain_a", null),
            new ContainerRule.ItemRow("chain_a", "clay_jar_covered_painted", null),
            new ContainerRule.ItemRow("stone", null, null),
            new ContainerRule.ItemRow("own_value", "clay_jar_covered", 0.5f),
            new ContainerRule.ItemRow("loop_a", "loop_b", null),
            new ContainerRule.ItemRow("loop_b", "loop_a", null),
        });

        Assert.Equal(0.3f, factors["clay_jar_covered"]);
        Assert.Equal(0.3f, factors["clay_jar_covered_painted"]);
        Assert.Equal(0.3f, factors["chain_b"]);
        Assert.Equal(0.5f, factors["own_value"]);
        Assert.False(factors.ContainsKey("stone"));
        Assert.False(factors.ContainsKey("loop_a"));
    }

    [Fact]
    public void The_rule_reads_the_factor_of_an_item()
    {
        Assert.Equal(0.3f, Rule().ContentFactor("glass_bottle_covered"));
        Assert.Equal(1f, Rule().ContentFactor("stone"));
        Assert.Equal(1f, Rule().ContentFactor(null));
    }
}
