using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Social;
using HandlebarsDotNet;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;
using static ConsensusChessShared.Content.BoardFormatter;

namespace ConsensusChessShared.Content
{
	public class PostBuilder
	{
		public static string UNKNOWN = "(unknown)";

		private static PostTemplates templates = new PostTemplates();

		public PostType Type { get; private set; }
		public Dictionary<string,object> Mappings { get; private set; }
		public long? ReplyToId { get; private set; }
		public string? ToHandle { get; private set; }
		public string? OverrideTemplate { get; private set; }

		public PostBuilder(PostType type)
		{
			Type = type;
			Mappings = new Dictionary<string, object>();
		}

		public PostBuilder WithBoard(Board board, BoardFormat format)
		{
			WithMapping("Board", BoardFormatter.FenToPieces(board, format));
			return this;
		}

		public PostBuilder WithGame(Game game)
		{
			WithObject("Game", game);
			var shortcodes = new List<string>();
			shortcodes.AddRange(game.WhitePostingNodeShortcodes.Select(ss => ss.Value!));
            shortcodes.AddRange(game.BlackPostingNodeShortcodes.Select(ss => ss.Value!));
			WithMapping("AllNodes", string.Join(", ", shortcodes.Distinct()));
            WithMapping("BlackParticipantNetworkServers", string.Join(", ", game.BlackParticipantNetworkServers.Select(ss => ss.Value)));
            WithMapping("WhiteParticipantNetworkServers", string.Join(", ", game.WhiteParticipantNetworkServers.Select(ss => ss.Value)));
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
			WithMapping("SocialStatus", status.ToString());
			return this;
		}

		public PostBuilder WithValidationState(VoteValidationState state)
		{
            WithMapping("ValidationState", state.ToString());
            return this;
        }

		public PostBuilder WithRejectionReason(CommandRejectionReason reason)
		{
			WithMapping("CommandRejectionReason", reason.ToString());
			return this;
		}

        public PostBuilder WithGameNotFoundReason(GameNotFoundReason reason)
        {
            WithMapping("GameNotFoundReason", reason.ToString());
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

        private PostBuilder InReplyTo(long? id, SocialUsername user)
		{
			ReplyToId = id;
			ToHandle = user.Full;
			return this;
		}

		public Post Build()
		{
			var template = OverrideTemplate == null
				? templates.For[Type]
				: Handlebars.Compile(OverrideTemplate);

			var message = template(Mappings);

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
			};
			return post;
		}
	}
}

