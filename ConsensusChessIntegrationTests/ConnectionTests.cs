using System.Diagnostics;
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
        var account = await social.GetCurrentUser();
        Assert.AreEqual("instantiator", account.AccountName);
    }

    [TestMethod]
    public async Task CanSendMessage()
    {
        var sent = await SendMessageAsync("This is a test message, please ignore.", Visibility.Direct);
        Assert.IsNotNull(sent);
    }

    [TestMethod]
    public void CanWriteToLog()
    {
        var lines = 0;
        if (File.Exists(logPath)) { lines = File.ReadAllLines(logPath).Count(); }
        WriteLogLine("CanWriteToLog test");
        Assert.IsTrue(File.Exists(logPath));
        Assert.AreEqual(lines + 1, File.ReadAllLines(logPath).Count());
    }

    [Ignore]
    [TestMethod]
    [Obsolete("dotnet test hides output")]
    public void You_perhaps_CanSeeADebugWriteLine()
    {
        Debug.WriteLine("You can see this Debug.WriteLine, right?");
    }

    [Ignore]
    [TestMethod]
    [Obsolete("dotnet test hides output")]
    public void You_perhaps_CanSeeATraceWriteLine()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        Trace.WriteLine("You can see this Trace.WriteLine, right?");
    }

}
