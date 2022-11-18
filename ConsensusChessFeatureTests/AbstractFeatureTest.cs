using System.Collections.Concurrent;
using ConsensusChessEngine.Service;
using ConsensusChessFeatureTests.Data;
using ConsensusChessFeatureTests.Database;
using ConsensusChessFeatureTests.Service;
using ConsensusChessNode.Service;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ConsensusChessFeatureTests;

public abstract class AbstractFeatureTest
{
    protected string logPath;

    public TestContext TestContext { get; set; }

    public Mock<ILogger> NodeLogMock { get; private set; }
    public Network NodeNetwork { get; private set; }
    public ServiceIdentity NodeId { get; private set; }
    public SqliteDbOperator NodeDbo { get; private set; }
    public Mock<ISocialConnection> NodeSocialMock { get; private set; }

    public Mock<ILogger> EngineLogMock { get; private set; }
    public Network EngineNetwork { get; private set; }
    public ServiceIdentity EngineId { get; private set; }
    public SqliteDbOperator EngineDbo { get; private set; }
    public Mock<ISocialConnection> EngineSocialMock { get; private set; }

    protected AbstractFeatureTest()
    {
        if (Directory.Exists("/logs"))
        {
            logPath = "/logs/feature-tests.log";
        }
        else
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            logPath = Path.Join(path, "feature-tests.log");
        }
        if (File.Exists(logPath)) { File.Delete(logPath); }

        NodeLogMock = new Mock<ILogger>();
        NodeNetwork = Network.FromEnv(FeatureDataGenerator.NodeEnv);
        NodeId = ServiceIdentity.FromEnv(FeatureDataGenerator.NodeEnv);
        NodeDbo = new SqliteDbOperator(NodeLogMock.Object, true);
        NodeSocialMock = new Mock<ISocialConnection>();

        EngineLogMock = new Mock<ILogger>();
        EngineNetwork = Network.FromEnv(FeatureDataGenerator.EngineEnv);
        EngineId = ServiceIdentity.FromEnv(FeatureDataGenerator.EngineEnv);
        EngineDbo = new SqliteDbOperator(EngineLogMock.Object, true);
        EngineSocialMock = new Mock<ISocialConnection>();

        InitNodeSocial(NodeSocialMock);
        InitEngineSocial(EngineSocialMock);
    }

    protected ConsensusChessNodeService CreateNode()
        => new ConsensusChessNodeService(
            NodeLogMock.Object,
            NodeId,
            NodeDbo,
            NodeNetwork,
            NodeSocialMock.Object);

    protected ConsensusChessEngineService CreateEngine()
        => new ConsensusChessEngineService(
            EngineLogMock.Object,
            EngineId,
            EngineDbo,
            EngineNetwork,
            EngineSocialMock.Object);

    protected void InitNodeSocial(Mock<ISocialConnection> mock)
    {
        mock.Setup(sc => sc.CalculateCommandSkips())
            .Returns(new string[]
            {
                $"node",
                $"node@{FeatureDataGenerator.NodeEnv["NETWORK_SERVER"]}",
                $"@node",
                $"@node@{FeatureDataGenerator.NodeEnv["NETWORK_SERVER"]}",
            });
        mock.Setup(sc => sc.AccountName).Returns("node");
        mock.Setup(sc => sc.DisplayName).Returns("Feature Test Node");

        mock.Setup(sc => sc.PostAsync(It.IsAny<Game>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
        mock.Setup(sc => sc.PostAsync(It.IsAny<SocialStatus>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
        mock.Setup(sc => sc.PostAsync(It.IsAny<Game>(), It.IsAny<Board>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
        mock.Setup(sc => sc.PostAsync(It.IsAny<string>(), It.IsAny<PostType>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
    }

    protected void InitEngineSocial(Mock<ISocialConnection> mock)
    {
        mock.Setup(sc => sc.CalculateCommandSkips())
            .Returns(new string[]
            {
                $"engine",
                $"engine@{FeatureDataGenerator.EngineEnv["NETWORK_SERVER"]}",
                $"@engine",
                $"@engine@{FeatureDataGenerator.EngineEnv["NETWORK_SERVER"]}",
            });
        mock.Setup(sc => sc.AccountName).Returns("engine");
        mock.Setup(sc => sc.DisplayName).Returns("Feature Test Engine");

        mock.Setup(sc => sc.PostAsync(It.IsAny<Game>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
        mock.Setup(sc => sc.PostAsync(It.IsAny<SocialStatus>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
        mock.Setup(sc => sc.PostAsync(It.IsAny<Game>(), It.IsAny<Board>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
        mock.Setup(sc => sc.PostAsync(It.IsAny<string>(), It.IsAny<PostType>(), It.IsAny<bool?>())).Returns(Task.FromResult<Post>(new Post()));
    }

    [TestInitialize]
    public async Task TestInit()
    {
        WriteLogHeader($"{TestContext.TestName}");

        // it's a shared database, so only clear it down once
        using (var db = NodeDbo.GetDb())
        {
            WriteLogLine($"Db: {db!.DbPath}");

            // ensure migrations are applied - just in case
            WriteLogLine("Ensuring the db is created...");
            db.Database.Migrate();

            var dbTables = db.Model.GetEntityTypes()
                    .SelectMany(t => t.GetTableMappings())
                    .Select(m => m.Table.Name)
                    .Distinct()
                    .ToList();

            foreach (var table in dbTables)
            {
                var sql = $"DELETE FROM {table};";
                WriteLogLine(sql);
                await db.Database.ExecuteSqlRawAsync(sql);
            }
        }
    }

    [TestCleanup]
    public void TestClose()
    {
        WriteLogLine();
    }

    protected void WriteLogHeader(string header)
    {
        File.AppendAllLines(logPath, new[] { "## " + header, "" });
    }

    protected void WriteLogLine(string log)
    {
        File.AppendAllLines(logPath, new[] { "* " + log });
    }

    protected void WriteLogLine()
    {
        File.AppendAllLines(logPath, new[] { "" });
    }

}
