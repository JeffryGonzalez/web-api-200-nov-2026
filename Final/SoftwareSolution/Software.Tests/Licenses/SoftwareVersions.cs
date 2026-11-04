using System.Text.Json;
using Software.Api.Licenses;

namespace Software.Tests.Licenses;

// A unit test: no host and no database. A converter is just a thing with right and wrong answers.
public class SoftwareVersions
{
    [Fact]
    public void WritesAString()
    {
        Assert.Equal("\"2026.1.3\"", JsonSerializer.Serialize(new SoftwareVersion(2026, 1, 3)));
    }

    [Fact]
    public void ReadsAString()
    {
        Assert.Equal(new SoftwareVersion(2026, 1, 3), JsonSerializer.Deserialize<SoftwareVersion>("\"2026.1.3\""));
    }

    [Theory]
    [InlineData("\"2026.1\"")]
    [InlineData("\"not.a.version\"")]
    public void RejectsAnythingElse(string json)
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SoftwareVersion>(json));
    }
}
