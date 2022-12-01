using System;
using ConsensusChessShared.Constants;
using HandlebarsDotNet;

namespace ConsensusChessShared.Content
{
	public class BoardTemplates
	{
        public Dictionary<DescriptionType, HandlebarsTemplate<object, object>> For { get; }

        private IHandlebars handlebars;

        public BoardTemplates()
        {
            handlebars = Handlebars.Create(new HandlebarsConfiguration()
            {
                ThrowOnUnresolvedBindingExpression = true
            });

            For = TemplateSource
                .Select(pair => KeyValuePair.Create(pair.Key, handlebars.Compile(pair.Value)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static Dictionary<DescriptionType, string> TemplateSource = new Dictionary<DescriptionType, string>()
        {
            { DescriptionType.Post, "{{ Board.ActiveSide }} to play...\n{{ BoardLayout }}" },
            { DescriptionType.Alt, "A chess board, with {{ Board.ActiveSide }} to play...\n{{ BoardLayout }}" },
        };
    }
}

