using ConsensusChessShared.Database;
using ConsensusChessShared.DTO;
using Mastonet;
using Mastonet.Entities;

namespace ConsensusChessIntegrationTests;

[TestClass]
public class ConnectionTests : AbstractIntegrationTests
{

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
