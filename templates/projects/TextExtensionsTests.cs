// awesome-humans/dotnet template | targets: net11.0, csharp-15 | last-reviewed: 2026-11-14 | sources: house:simon-mckenna, ms-learn
// Belongs to Example.Library.Tests. Without a test, `dotnet test` exits with code 8, because MTP fails a session that runs no tests.
using Xunit;

namespace Example.Library.Tests;

public class TextExtensionsTests
{
    [Fact]
    public void TruncateReturnsTextShorterThanMaxLengthUnchanged() =>
        Assert.Equal("Hi", "Hi".Truncate(5));

    // house:simon-mckenna begin
    // **House:** UnitOfWork_Scenario_ExpectedBehaviour names, with Arrange, Act, and Assert comments
    // (opinions/testing.md). The test project's house region suppresses CA1707 for the underscores.
    [Fact]
    public void Truncate_TextLongerThanMaxLength_KeepsFirstMaxLengthCharacters()
    {
        // Act
        string truncated = "Hello, world".Truncate(5);

        // Assert
        Assert.Equal("Hello", truncated);
    }
    // house:simon-mckenna end
}
