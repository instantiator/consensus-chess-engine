using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Graphics;
using ConsensusChessShared.Helpers;
using ConsensusChessShared.Service;
using ConsensusChessShared.Social;
using HandlebarsDotNet;
using Mastonet;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using static ConsensusChessShared.Content.BoardFormatter;
using static ConsensusChessShared.Graphics.BoardGraphicsData;

namespace ConsensusChessShared.Content
{
	public partial class PostBuilder
	{
		public static string UNKNOWN = "(unknown)";

		private static PostTemplates templates = new PostTemplates();

		public PostType Type { get; private set; }
		public Dictionary<string,object> Mappings { get; private set; }
		public string? ReplyToId { get; private set; }
		public string? ToHandle { get; private set; }
		public string? OverrideTemplate { get; private set; }
		public List<Media> Media { get; private set; }
		public Visibility? OverrideMastodonVisibility { get; private set; }

		private EnumTranslator translator;

		public PostBuilder(ServiceConfig config, PostType type)
		{
			Type = type;
			Mappings = new Dictionary<string, object>();
			Media = new List<Media>();
			translator = new EnumTranslator();
			WithObject("Config", config);

		}

		public PostBuilder WithBoard(Board board, BoardFormat textFormat, Move? cause)
		{
			WithObject("Board", board);
			WithObject("BoardFormat", textFormat);
			WithMapping("BoardText", BoardFormatter.FenToPieces(board, textFormat));
			WithMapping("BoardDescription", BoardFormatter.DescribeBoard(board, false, textFormat, cause));
			return this;
		}

		public PostBuilder AndBoardGraphic(BoardStyle  style, BoardFormat altFormat, Move? cause)
		{
			if (!Mappings.ContainsKey("Board")) throw new ArgumentNullException("Board");
            WithObject("BoardStyle", style);
			var board = (Board)Mappings["Board"];
			var renderer = new BoardRenderer(style);
			var bmp = renderer.Render(board);
			WithMedia(new Media(
				filename: "board.png",
				data: bmp.ToPngBytes(),
				alt: BoardFormatter.DescribeBoard(board, true, altFormat, cause)));
			return this;
		}

		public PostBuilder WithMedia(Media media)
		{
			Media.Add(media);
			return this;
		}

        public PostBuilder WithGame(Game game)
		{
			WithObject("Game", game);
			var shortcodes = new List<string>();
			shortcodes.AddRange(game.WhitePostingNodeShortcodes.Select(ss => ss.Value!));
            shortcodes.AddRange(game.BlackPostingNodeShortcodes.Select(ss => ss.Value!));
			WithMapping("StartTimeDescription", translator.DescribeStartTime(game.ScheduledStart));
            WithMapping("FormattedGameMoveDuration", translator.Translate_to_DaysHours(game.MoveDuration));
			WithMapping("SideRules", translator.Translate(game.SideRules));
            WithMapping("SideRulesExplanation", translator.Explain(game.SideRules));
            WithMapping("AllNodes", string.Join(", ", shortcodes.Distinct()));
            WithMapping("BlackParticipantNetworkServers", string.Join(", ", game.BlackParticipantNetworkServers.Select(ss => ss.Value)));
            WithMapping("WhiteParticipantNetworkServers", string.Join(", ", game.WhiteParticipantNetworkServers.Select(ss => ss.Value)));
			WithMapping("GameState", translator.Translate(game.State));
            return this;
		}

		public PostBuilder WithSide(Side side)
		{
			WithObject("Side", side);
			return this;
		}

		public PostBuilder WithMove(Move move)
		{
			WithObject("Move", move);
			WithMapping(
				"FormattedMoveTimeRemaining",
				translator.Translate_to_Hours(move.TimeRemaining));
			return this;
		}

		public PostBuilder WithOptionalItems(IEnumerable<string>? items)
        {
			var summary = items == null || items.Count() == 0
				? ""
				: $"References: {string.Join(", ", items)}";
			WithMapping("Items", string.Join(", ", items ?? new string[0]));
            WithMapping("ItemsSummary", summary);
			return this;
        }

        public PostBuilder WithObject(Object obj)
		{
			WithObject(obj.GetType().Name, obj);
			return this;
		}

		public PostBuilder WithNodeState(NodeState state)
		{
			WithObject("State", state);
			return this;
		}

		public PostBuilder WithSocialStatus(SocialStatus status)
		{
			WithMapping("SocialStatus", translator.Translate(status));
			return this;
		}

		public PostBuilder WithValidationState(VoteValidationState state)
		{
            WithMapping("ValidationDescription", translator.Describe(state));
            return this;
        }

		public PostBuilder WithRejectionReason(CommandRejectionReason reason)
		{
			WithMapping("CommandRejectionDescription", translator.Describe(reason));
			return this;
		}

        public PostBuilder WithGameNotFoundReason(GameNotFoundReason reason)
        {
            WithMapping("GameNotFoundReason", translator.Describe(reason));
            return this;
        }

        public PostBuilder WithDetail(string? detail)
        {
            WithMapping("Detail", detail ?? "");
            return this;
        }

		public PostBuilder WithUsername(SocialUsername? username)
		{
			if (username != null)
			{
				WithObject("Username", username);
			}
			return this;
		}

		public PostBuilder WithVote(Vote vote)
		{
			WithObject("Vote", vote);
			WithMapping("SAN", vote.MoveSAN!);
			return this;
		}

        public PostBuilder WithPreexistingVote(Vote vote)
        {
            WithObject("PreexistingVote", vote);
            WithMapping("PreviousSAN", vote.MoveSAN!);
            return this;
        }

        public PostBuilder WithMoveText(string moveText)
		{
            WithMapping("MoveText", moveText);
            return this;
        }

        public PostBuilder WithObject(string name, Object obj)
		{
			Mappings.Add(name, obj);
            return this;
        }

        public PostBuilder WithMapping(string key, string value)
		{
			Mappings.Add(key, value);
			return this;
		}

		public PostBuilder WithTemplate(string template)
		{
			OverrideTemplate = template;
			return this;
		}

		public PostBuilder WithText(string text)
		{
			WithMapping("Text", text);
			return this;
		}

        public PostBuilder InReplyTo(SocialCommand origin)
		{
			return InReplyTo(origin.SourcePostId, origin.SourceUsername);
		}

        public PostBuilder InReplyTo(Post post)
        {
            return InReplyTo(post.NetworkPostId!, null);
        }

        public PostBuilder InReplyTo(string id, SocialUsername? user)
		{
			ReplyToId = id;
			ToHandle = user?.Full;
			return this;
		}

		public PostBuilder WithOverrideVisibility(Visibility visibility)
		{
			OverrideMastodonVisibility = visibility;
			return this;
		}

		public Post Build()
		{
			var template = OverrideTemplate == null
				? templates.For[Type]
				: Handlebars.Compile(OverrideTemplate);

			var message = template(Mappings).RestoreUnicode();

			if (ToHandle != null)
			{
				message = $"@{ToHandle} {message}";
			}

			var post = new Post()
			{
				Created = DateTime.Now.ToUniversalTime(),
				Message = message,
				NetworkReplyToId = ReplyToId,
				Type = Type,
				Media = Media,
				OverrideMastodonVisibility = OverrideMastodonVisibility
			};
			return post;
		}
	}
}

