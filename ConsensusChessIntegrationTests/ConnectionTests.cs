using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using Mastonet;
using Mastonet.Entities;

namespace ConsensusChessIntegrationTests;

[TestClass]
public class ConnectionTests
{
    private static readonly HttpClient http = new HttpClient();

    protected ConsensusChessDbContext GetDb()
        => ConsensusChessDbContext.FromEnvironment(Environment.GetEnvironmentVariables());

    protected Network GetNetwork()
        => Network.FromEnvironment(Environment.GetEnvironmentVariables());

    protected MastodonClient GetMastodonClient()
    {
        var network = GetNetwork();

        AppRegistration reg = new AppRegistration()
        {
            ClientId = network.AppKey,
            ClientSecret = network.AppSecret,
            Instance = network.NetworkServer,
            Scope = Scope.Read | Scope.Write
        };

        Auth token = new Auth()
        {
            AccessToken = network.AppToken
        };

        return new MastodonClient(reg, token, http);
    }

    [TestMethod]
    public void CheckDatabaseConnection()
    {
        using (var db = GetDb())
        {
            var connection = db.Database.CanConnect();
            Assert.IsTrue(connection);
        }
    }

    [TestMethod]
    public async Task CheckSocialConnection()
    {
        var client = GetMastodonClient();
        var account = await client.GetCurrentUser();
        Assert.AreEqual("instantiator", account.AccountName);
    }
}
