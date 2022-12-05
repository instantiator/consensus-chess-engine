using System;
using System.Text;
using HtmlAgilityPack;

namespace ConsensusChessShared.Helpers
{
    public static class CommandHelper
    {
        public static IEnumerable<string> ParseSocialCommand(string data, IEnumerable<string>? skip = null)
        {
            skip ??= new string[0];
            skip = skip.Select(x => x.ToLower());
            return RemoveUnwantedTags(data)
                .SplitOutsideQuotes(new[] { ' ', ',' }, true, true, true)
                .Select(part => part.Trim(' ', '"')) // trim quotes away
                .Where(x => !string.IsNullOrWhiteSpace(x) && !skip.Contains(x.ToLower()));
        }

        public static string CleanupStatus(string status)
        {
            var content = RemoveUnwantedTags(status);
            return content;
        }

        // See: https://stackoverflow.com/a/12836974
        public static string RemoveUnwantedTags(string data, IEnumerable<string>? keepTags = null)
        {
            if (string.IsNullOrWhiteSpace(data)) return data;
            keepTags ??= new string[0];
            var document = new HtmlDocument();
            document.LoadHtml(data);
            var nodes = new Queue<HtmlNode>(document.DocumentNode.SelectNodes("./*|./text()"));
            while (nodes.Count > 0)
            {
                var node = nodes.Dequeue();
                var parentNode = node.ParentNode;
                if (!keepTags.Contains(node.Name) && node.Name != "#text")
                {
                    var childNodes = node.SelectNodes("./*|./text()");
                    if (childNodes != null)
                    {
                        foreach (var child in childNodes)
                        {
                            nodes.Enqueue(child);
                            parentNode.InsertBefore(child, node);
                        }
                    }
                    parentNode.RemoveChild(node);
                }
            }
            return document.DocumentNode.InnerHtml;
        }
    }
}

