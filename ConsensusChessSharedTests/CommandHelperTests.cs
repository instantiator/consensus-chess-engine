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
    }

    [TestMethod]
    public void RemoveUnwantedTags_preserves_EndSpaces()
    {
        // surround with spaces - spaces are preserved
        var request = "<p><span class=\"h-card\"><a href=\"https://botsin.space/@icgames\" class=\"u-url mention\" rel=\"nofollow noopener noreferrer\" target=\"_blank\">@<span>icgames</span></a></span> shutdown</p>";
        request = $" {request} ";
        var expected = " @icgames shutdown ";
        Assert.AreEqual(expected, CommandHelper.RemoveUnwantedTags(request));
    }

    [TestMethod]
    public void ParseSocialCommand_returns_Words()
    {
        var request = "<p><span class=\"h-card\"><a href=\"https://botsin.space/@icgames\" class=\"u-url mention\" rel=\"nofollow noopener noreferrer\" target=\"_blank\">@<span>icgames</span></a></span> shutdown</p>";
        var expected = new string[] { "@icgames", "shutdown" };
        var actual = CommandHelper.ParseSocialCommand(request).ToList();
        CollectionAssert.AreEquivalent(expected, actual);
    }

    [TestMethod]
    public void ParseSocialCommand_skips_SkipStrings()
    {
        var request = "<p><span class=\"h-card\"><a href=\"https://botsin.space/@icgames\" class=\"u-url mention\" rel=\"nofollow noopener noreferrer\" target=\"_blank\">@<span>icgames</span></a></span> shutdown</p>";
        var skips = new[] { "@icgames" };
        var expected = new string[] { "shutdown" };
        CollectionAssert.AreEquivalent(expected, CommandHelper.ParseSocialCommand(request, skips).ToList());
    }

    public void ParseSocialCommand_trims_EndSpaces()
    {
        var request = "<p><span class=\"h-card\"><a href=\"https://botsin.space/@icgames\" class=\"u-url mention\" rel=\"nofollow noopener noreferrer\" target=\"_blank\">@<span>icgames</span></a></span> shutdown</p>";
        request = $" {request} ";
        var skips = new[] { "@icgames" };
        var expected = new string[] { "shutdown" };
        CollectionAssert.AreEquivalent(expected, CommandHelper.ParseSocialCommand(request, skips).ToList());
    }

    [TestMethod]
    public void ParseSocialCommand_handles_QuotedValues()
    {
        var request = "<p><span class=\"h-card\"><a href=\"https://botsin.space/@icgames\" class=\"u-url mention\" rel=\"nofollow noopener noreferrer\" target=\"_blank\">@<span>icgames</span></a></span> new \"title\" \"a long description\" node-1 node-2 node-3</p>";
        var expected = new string[]
        {
            "@icgames",
            "new",
            "title",
            "a long description",
            "node-1",
            "node-2",
            "node-3"
        };
        var actual = CommandHelper.ParseSocialCommand(request).ToList();
        CollectionAssert.AreEquivalent(expected, actual);
    }
}