using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Principal;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Exceptions;
using ConsensusChessShared.Social;
using Mastonet.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;

namespace ConsensusChessShared.Content
{
	public class PostBuilder
	{
		public static string UNKNOWN = "(unknown)";

		private static PostTemplates Templates = new PostTemplates();

		public PostType Type { get; private set; }
		public Dictionary<string,object> Mappings { get; private set; }
		public long? ReplyToId { get; private set; }
		public string? OverrideTemplate { get; private set; }

		public PostBuilder(PostType type)
		{
			Type = type;
			Mappings = new Dictionary<string, object>();
		}

		public PostBuilder WithBoard(Board board)
		{
			WithMapping("FEN", BoardFormatter.FenToPieces(board));
			return this;
		}

		public PostBuilder WithGame(Game game)
		{
			WithObject("Game", game);
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

        public PostBuilder WithAccount(string? account)
		{
            WithMapping("Account", account ?? UNKNOWN);
            return this;
        }

        public PostBuilder WithSAN(string SAN)
		{
            WithMapping("SAN", SAN);
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
			return InReplyTo(origin.SourceId);
		}

        public PostBuilder InReplyTo(long? id)
		{
			ReplyToId = id;
			return this;
		}

		public Post Build()
		{
			var templateStr = PostTemplates.TemplateSource[Type];
			var message = OverrideTemplate ?? Templates.For[Type](Mappings);
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

