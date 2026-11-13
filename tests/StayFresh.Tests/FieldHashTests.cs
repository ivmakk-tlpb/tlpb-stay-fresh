using StayFresh;
using Xunit;

public class FieldHashTests
{
    // "identifier" gives the value of ModelDefinition.identifierFieldHash in the game (session 20261112-013532-070).
    [Theory]
    [InlineData("identifier", -1823948977)]
    [InlineData("contentsDecayFactor", 980457953)]
    [InlineData("copyFromIdentifier", -27596472)]
    public void The_hash_is_the_one_of_the_game(string name, int hash)
    {
        Assert.Equal(hash, FieldHash.Of(name));
    }
}
