using System.Collections.Concurrent;
using ConsensusChessEngine.Service;
using ConsensusChessFeatureTests.Data;
using ConsensusChessFeatureTests.Database;
using ConsensusChessFeatureTests.Service;
using ConsensusChessNode.Service;
using ConsensusChessShared.Constants;
using ConsensusChessShared.Content;
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
    public static string logPath;

    public TestContext? TestContext { get; set; }

    public SqliteDbOperator Dbo { get; private set; }

    public Mock<ILogger> NodeLogMock { get; private set; }
    public Network NodeNetwork { get; private set; }
    public ServiceIdentity NodeId { get; private set; }
    public ServiceConfig NodeConfig { get; private set; }
    public Mock<ISocialConnection> NodeSocialMock { get; private set; }

    public Mock<ILogger> EngineLogMock { get; private set; }
    public Network EngineNetwork { get; private set; }
    public ServiceIdentity EngineId { get; private set; }
    public ServiceConfig EngineConfig { get; private set; }
    public Mock<ISocialConnection> EngineSocialMock { get; private set; }

    protected Dictionary<string, Func<SocialCommand, Task>> receivers;
    protected ConcurrentBag<Post> postsSent;

    protected TimeSpan fastPollOverride = TimeSpan.FromMilliseconds(1);
    protected TimeSpan spinWaitTimeout = TimeSpan.FromSeconds(10);

    protected Task? nodeTask;
    protected Task? engineTask;

    protected AbstractFeatureTest()
    {
        postsSent = new ConcurrentBag<Post>();
    }

    [AssemblyInitialize]
    public static void InitLogsOnce(TestContext context)
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
    }

    #region Start and finish tests

    [TestInitialize]
    public async Task TestInit()
    {
        WriteLogHeader($"{TestContext!.TestName}");

        NodeLogMock = new Mock<ILogger>();
        NodeNetwork = Network.FromEnv(FeatureDataGenerator.NodeEnv);
        NodeId = ServiceIdentity.FromEnv(FeatureDataGenerator.NodeEnv);
        NodeConfig = ServiceConfig.FromEnv(FeatureDataGenerator.NodeEnv);
        NodeSocialMock = new Mock<ISocialConnection>();

        EngineLogMock = new Mock<ILogger>();
        EngineNetwork = Network.FromEnv(FeatureDataGenerator.EngineEnv);
        EngineId = ServiceIdentity.FromEnv(FeatureDataGenerator.EngineEnv);
        EngineConfig = ServiceConfig.FromEnv(FeatureDataGenerator.EngineEnv);
        EngineSocialMock = new Mock<ISocialConnection>();

        receivers = new Dictionary<string, Func<SocialCommand, Task>>();
        postsSent = new ConcurrentBag<Post>();

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

    [TestCleanup]
    public void TestClose()
    {
        WriteLogLine();
    }

    #endregion

    #region Init support

    private ConsensusChessNodeService CreateNode()
        => new ConsensusChessNodeService(
            NodeLogMock.Object,
            NodeId,
            Dbo,
            NodeNetwork,
            NodeSocialMock.Object,
            NodeConfig,
            fastPollOverride);

    private ConsensusChessEngineService CreateEngine()
        => new ConsensusChessEngineService(
            EngineLogMock.Object,
            EngineId,
            Dbo,
            EngineNetwork,
            EngineSocialMock.Object,
            EngineConfig,
            fastPollOverride);

    protected async Task<ConsensusChessNodeService> StartNodeAsync()
    {
        var node = CreateNode();
        var cancel = new CancellationTokenSource();
        var token = cancel.Token;
        await node.StartAsync(token);
        WaitAndAssert(() => File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH),"Healthcheck file not created.");
        nodeTask = node.ExecuteAsync(token); // don't await - this is a background process
        SpinWait.SpinUntil(() => receivers.ContainsKey(NodeId.Shortcode));
        return node;
    }

    protected async Task<ConsensusChessEngineService> StartEngineAsync()
    {
        var engine = CreateEngine();
        var cancel = new CancellationTokenSource();
        var token = cancel.Token;
        await engine.StartAsync(token);
        SpinWait.SpinUntil(() => File.Exists(AbstractConsensusService.HEALTHCHECK_READY_PATH));
        engineTask = engine.ExecuteAsync(token); // don't await - this is a background process
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

        mock.Setup(sc => sc.Username).Returns(SocialUsername.From(account, $"Mx. {account}", network));

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

        mock.Setup(sc => sc.StartListeningForCommandsAsync(It.IsAny<Func<SocialCommand, Task>>(), It.IsAny<bool>()))
            .Callback<Func<SocialCommand, Task>, bool>((r, b) => receivers.Add(shortcode, r))
            .Returns(Task.CompletedTask)
            .Verifiable();
    }

    #endregion Init

    #region Logging

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

    #endregion

    #region Actions

    protected async Task<Game> StartGameWithDbAsync(string? fen = null)
    {
        var game = new Game("game-shortcode", "title", "description",
                new[] { NodeNetwork.NetworkServer },
                new[] { NodeNetwork.NetworkServer },
                new[] { NodeId.Shortcode },
                new[] { NodeId.Shortcode },
                SideRules.MoveLock,
                null, null);

        game.CurrentBoard.FEN = fen ?? game.CurrentBoard.FEN;

        using (var db = Dbo.GetDb())
        {
            db.Games.Add(game);
            await db.SaveChangesAsync();
        }

        return game;
    }

    protected async Task ExpireCurrentMoveShortlyAsync(Game game, TimeSpan? expiry = null)
    {
        using (var db = Dbo.GetDb())
        {
            var game1 = db.Games.Single(g => g.Shortcode == game.Shortcode);
            game1.CurrentMove.Deadline = DateTime.Now.Add(expiry ?? TimeSpan.FromMilliseconds(1)).ToUniversalTime();
            await db.SaveChangesAsync();
        }
    }

    protected void WaitAndAssert(Func<bool> criteria, string? onFailDescription = null, TimeSpan? timeoutOverride = null)
    {
        SpinWait.SpinUntil(criteria, timeoutOverride ?? spinWaitTimeout);
        Assert.IsTrue(criteria.Invoke(), onFailDescription ?? "Timed out waiting for criteria");
    }

    protected Post WaitAndAssert_NodePostsBoard(Game game, string nodeShortcode)
    {
        WaitAndAssert(() =>
        {
            using (var db = Dbo.GetDb())
                return db.Games
                    .Single(g => g.Shortcode == game.Shortcode)
                    .CurrentBoard
                    .BoardPosts
                    .Where(p => p.NodeShortcode == nodeShortcode)
                    .Count(p => p.Type == PostType.Node_BoardUpdate) == 1
                && db.Games
                    .Single(g => g.Shortcode == game.Shortcode)
                    .CurrentBoard
                    .BoardPosts
                    .Where(p => p.NodeShortcode == nodeShortcode)
                    .Count(p => p.Type == PostType.Node_VotingInstructions) == 1;
        });

        // get the last post
        return WaitAndAssert_Posts(
            shortcode: nodeShortcode,
            ofType: PostType.Node_BoardUpdate)
            .OrderBy(p => p.Created)
            .Last();
    }

    protected void WaitAndAssert_Moves(Game game, int moves, int made)
    {
        WaitAndAssert(() =>
        {
            using (var db = Dbo.GetDb())
                return db.Games
                    .Single(g => g.Shortcode == game.Shortcode)
                    .Moves.Count() == moves;
        });
        WaitAndAssert(() =>
        {
            using (var db = Dbo.GetDb())
                return db.Games
                    .Single(g => g.Shortcode == game.Shortcode)
                    .Moves.Count(m => m.SelectedSAN != null) == made;
        });
    }

    protected Post WaitAndAssert_NodeRepliesTo(SocialCommand command, PostType? ofType = null)
        => WaitAndAssert_Post(command, NodeId.Shortcode, ofType);

    protected Post WaitAndAssert_EngineRepliesTo(SocialCommand command, PostType? ofType = null)
    => WaitAndAssert_Post(command, EngineId.Shortcode, ofType);

    protected Post WaitAndAssert_Post(SocialCommand? command = null, string? shortcode = null, PostType? ofType = null, int? count = null)
        => WaitAndAssert_Posts(command, shortcode, ofType, count).Single();

    protected IEnumerable<Post> WaitAndAssert_Posts(SocialCommand? command = null, string? shortcode = null, PostType? ofType = null, int? count = null)
    {
        int found = 0;
        WaitAndAssert(() =>
        {
            if (count == null)
            {
                return postsSent.Any(post
                    => post.Succeeded
                    && (command == null || post.NetworkReplyToId == command.SourcePostId)
                    && (shortcode == null || post.NodeShortcode == shortcode)
                    && (ofType == null || post.Type == ofType));
            }
            else
            {
                found = postsSent.Count(post
                    => post.Succeeded
                    && (command == null || post.NetworkReplyToId == command.SourcePostId)
                    && (shortcode == null || post.NodeShortcode == shortcode)
                    && (ofType == null || post.Type == ofType));
                return found == count.Value;
            }
        }, $"{shortcode} did not post as expected. Expected: {count?.ToString() ?? "(any)"}");

        return postsSent.Where(post
            => post.Succeeded
            && (command == null || post.NetworkReplyToId == command.SourcePostId)
            && (shortcode == null || post.NodeShortcode == shortcode)
            && (ofType == null || post.Type == ofType))
            .OrderBy(p => p.Created);
    }

    protected async Task<SocialCommand> SendToEngineAsync(string text, bool authorised = true)
    {
        var message = $"{EngineSocialMock.Object.Username!.AtFull} {text}";
        var command = FeatureDataGenerator.GenerateCommand(message, EngineNetwork, authorised: authorised);
        await receivers[EngineId.Shortcode].Invoke(command);
        return command;
    }

    protected async Task<SocialCommand> SendToNodeAsync(string text)
    {
        var message = $"{NodeSocialMock.Object.Username!.AtFull} {text}";
        var command = FeatureDataGenerator.GenerateCommand(message, NodeNetwork);
        await receivers[NodeId.Shortcode].Invoke(command);
        return command;
    }

    protected async Task<SocialCommand> ReplyToNodeAsync(Post boardPost, string text, string? from = null)
    {
        var message = $"{NodeSocialMock.Object.Username!.AtFull} {text}";
        var command = FeatureDataGenerator.GenerateCommand(message, NodeNetwork, from: from, inReplyTo: boardPost.NetworkPostId);
        await receivers[NodeId.Shortcode].Invoke(command);
        return command;
    }

    #endregion
}
