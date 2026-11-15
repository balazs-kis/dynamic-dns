namespace DynamicDnsClient.Tests;

public class AppVersionTests
{
    [Fact]
    public void CurrentVersionIsAValidVersionString()
    {
        // e.g. "0.0.0-dev+3a1418ad..." in local builds, "2.2.0+3a1418ad..." in releases
        Assert.Matches(@"^\d+\.\d+\.\d+(-[0-9A-Za-z.-]+)?(\+[0-9a-f]{40})?$", AppVersion.Current);
    }
}
