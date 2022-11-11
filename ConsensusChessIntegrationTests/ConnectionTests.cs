using ConsensusChessShared.Database;

namespace ConsensusChessIntegrationTests;

[TestClass]
public class ConnectionTests
{

    protected ConsensusChessDbContext GetDb()
        => ConsensusChessDbContext.FromEnvironment(Environment.GetEnvironmentVariables());

    [ClassInitialize]
    public static void Init(TestContext context)
    {
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
    public void CheckSocialConnection()
    {
        throw new NotImplementedException();
    }
}
