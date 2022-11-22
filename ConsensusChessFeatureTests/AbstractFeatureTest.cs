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

[TestClass]
public abstract class AbstractFeatureTest
{
    protected string logPath;

    public TestContext TestContext { get; set; }

    public SqliteDbOperator Dbo { get; private set; }

    public Mock<ILogger> NodeLogMock { get; private set; }
    public Network NodeNetwork { get; private set; }
    public ServiceIdentity NodeId { get; private set; }
    public Mock<ISocialConnection> NodeSocialMock { get; private set; }

    public Mock<ILogger> EngineLogMock { get; private set; }
    public Network EngineNetwork { get; private set; }
    public ServiceIdentity EngineId { get; private set; }
    public Mock<ISocialConnection> EngineSocialMock { get; private set; }

    protected Dictionary<string, Func<SocialCommand, Task>> receivers;
    protected List<Post> postsSent;

    protected TimeSpan FastPollOverride = TimeSpan.FromSeconds(0.5);

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
        postsSent = new List<Post>();
    }

    [TestInitialize]
    public async Task TestInit()
    {
        WriteLogHeader($"{TestContext.TestName}");

        NodeLogMock = new Mock<ILogger>();
        NodeNetwork = Network.FromEnv(FeatureDataGenerator.NodeEnv);
        NodeId = ServiceIdentity.FromEnv(FeatureDataGenerator.NodeEnv);
        NodeSocialMock = new Mock<ISocialConnection>();

        EngineLogMock = new Mock<ILogger>();
        EngineNetwork = Network.FromEnv(FeatureDataGenerator.EngineEnv);
        EngineId = ServiceIdentity.FromEnv(FeatureDataGenerator.EngineEnv);
        EngineSocialMock = new Mock<ISocialConnection>();

        receivers = new Dictionary<string, Func<SocialCommand, Task>>();
        postsSent = new List<Post>();

        Dbo = new SqliteDbOperator(NodeLogMock.Object, TestContext!.TestName, DateTime.Now);

        // it's a shared database, so only clear it down once
        using (var db = Dbo.GetDb())
        {
            WriteLogLine($"Db: {db!.DbPath}");

            // ensure migrations are applied - just in case
            WriteLogLine("Ensuring the db is created...");
            Dbo.InitDb(db);
            WriteLogLine("Ensuring the db is blank...");
            await Dbo.WipeDataAsync(db);
        }

        // initialise the social mocks
        WriteLogLine("Initialising social mocks...");
        InitSocialMock(NodeSocialMock, "node", NodeNetwork, NodeId.Shortcode);
        InitSocialMock(EngineSocialMock, "engine", EngineNetwork, EngineId.Shortcode);
    }

    private ConsensusChessNodeService CreateNode()
        => new ConsensusChessNodeService(
            NodeLogMock.Object,
            NodeId,
            Dbo,
            NodeNetwork,
            NodeSocialMock.Object,
            FastPollOverride);

    private ConsensusChessEngineService CreateEngine()
        => new ConsensusChessEngineService(
            EngineLogMock.Object,
            EngineId,
            Dbo,
            EngineNetwork,
            EngineSocialMock.Object,
            FastPollOverride);

    protected async Task<ConsensusChessNodeService> StartNodeAsync()
    {
        var node = CreateNode();
        var cancel = new CancellationTokenSource();
        var token = cancel.Token;
        node.StartAsync(token);
        SpinWait.SpinUntil(() => File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH));
        node.ExecuteAsync(token);
        SpinWait.SpinUntil(() => receivers.ContainsKey(NodeId.Shortcode));
        return node;
    }

    protected async Task<ConsensusChessEngineService> StartEngineAsync()
    {
        var engine = CreateEngine();
        var cancel = new CancellationTokenSource();
        var token = cancel.Token;
        engine.StartAsync(token);
        SpinWait.SpinUntil(() => File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH));
        engine.ExecuteAsync(token);
        SpinWait.SpinUntil(() => receivers.ContainsKey(EngineId.Shortcode));
        return engine;
    }

    protected void InitSocialMock(Mock<ISocialConnection> mock, string account, Network network, string shortcode)
    {
        mock.Setup(sc => sc.CalculateCommandSkips())
            .Returns(new string[]
            {
                $"{account}",
                $"{account}@{network.NetworkServer}",
                $"@{account}",
                $"@{account}@{network.NetworkServer}",
            });

        mock.Setup(sc => sc.AccountName).Returns($"{account}");
        mock.Setup(sc => sc.DisplayName).Returns($"Mock for: {account}");

        Func<Post, bool?, Task<Post>> postFunc =
            (post, dry)
                => Task.FromResult(FeatureDataGenerator.SimulatePost(post, shortcode, NodeNetwork));

        var storePost = (Post post, bool? hush) =>
        {
            post.Succeeded = true;
            postsSent.Add(post);
            return post;
        };

        mock.Setup(sc => sc.PostAsync(It.IsAny<Post>(), It.IsAny<bool?>()))
            .Returns(postFunc)
            .Callback((Post post, bool? hush) => Task.FromResult(storePost(post, hush)))
            .Verifiable();

        Func<string, PostType, bool, Task<Post>> postTextFunc =
            (msg, type, dry)
                => Task.FromResult(FeatureDataGenerator.GeneratePost(shortcode, NodeNetwork, msg, type));

        mock.Setup(sc => sc.PostAsync(It.IsAny<string>(), It.IsAny<PostType>(), It.IsAny<bool?>()))
            .Returns(postTextFunc)
            .Verifiable();

        mock.Setup(sc => sc.StartListeningForCommandsAsync(It.IsAny<Func<SocialCommand, Task>>(), It.IsAny<bool>()))
            .Callback<Func<SocialCommand, Task>, bool>((r, b) => receivers.Add(shortcode, r))
            .Returns(Task.CompletedTask)
            .Verifiable();
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
