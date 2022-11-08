using ConsensusChessShared.Helpers;

namespace ConsensusChessSharedTests;

[TestClass]
public class CommandHelperTests
{
    [TestMethod]
    public void RemoveUnwantedTags_returns_PlainText()
    {
        var request = "<p><span class=\"h-card\"><a href=\"https://botsin.space/@icgames\" class=\"u-url mention\" rel=\"nofollow noopener noreferrer\" target=\"_blank\">@<span>icgames</span></a></span> shutdown</p>";
        var expected = "@icgames shutdown";
        Assert.AreEqual(expected, CommandHelper.RemoveUnwantedTags(request));

        // surround with spaces - spaces are preserved
        request = $" {request} ";
        expected = " @icgames shutdown ";
        Assert.AreEqual(expected, CommandHelper.RemoveUnwantedTags(request));
    }

    [TestMethod]
    public void ParseSocialCommand_returns_Words()
    {
        var request = "<p><span class=\"h-card\"><a href=\"https://botsin.space/@icgames\" class=\"u-url mention\" rel=\"nofollow noopener noreferrer\" target=\"_blank\">@<span>icgames</span></a></span> shutdown</p>";
        var expected = new string[] { "@icgames", "shutdown" };
        var actual = CommandHelper.ParseSocialCommand(request).ToList();

        CollectionAssert.AreEquivalent(expected, actual);

        var skips = new[] { "@icgames" };
        expected = new string[] { "shutdown" };
        CollectionAssert.AreEquivalent(expected, CommandHelper.ParseSocialCommand(request, skips).ToList());

        request = $" {request} ";
        CollectionAssert.AreEquivalent(expected, CommandHelper.ParseSocialCommand(request, skips).ToList());
    }
}
