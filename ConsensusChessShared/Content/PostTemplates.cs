using System;
using ConsensusChessShared.Constants;
using ConsensusChessShared.DTO;
using HandlebarsDotNet;
using Mastonet.Entities;

namespace ConsensusChessShared.Content
{
	public class PostTemplates
	{
        public Dictionary<PostType, HandlebarsTemplate<object, object>> For { get; }

        public PostTemplates()
        {
            For = TemplateSource
                .Select(pair => KeyValuePair.Create(pair.Key, Handlebars.Compile(pair.Value)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static Dictionary<PostType, string> TemplateSource = new Dictionary<PostType, string>()
        {
            { PostType.SocialStatus, "{{ State.Name }} ({{ State.Shortcode }}): {{ SocialStatus }}" },
            { PostType.Node_BoardUpdate, "New board. You have {{ Game.MoveDuration }} to vote.\n{{ FEN }}" },
            { PostType.CommandResponse, "{{ Text }}" },
            { PostType.Engine_GameCreationResponse, "New {{ Game.SideRules }} game for: {{ AllNodes }}" },
            { PostType.Engine_GameAnnouncement, "New {{ Game.SideRules }} game...\nWhite: {{ WhiteParticipantNetworkServers }}\nBlack: {{ BlackParticipantNetworkServers }}\nMove duration: {{ Game.MoveDuration }}" },
            { PostType.Engine_GameAbandoned, "Game abandoned: {{ Game.Shortcode }}" },
            { PostType.Engine_GameAdvance, "Game advanced: {{ Game.Shortcode }}" },
            { PostType.GameNotFound, "This vote can't be processed: {{ GameNotFoundReason }}" },
            { PostType.CommandRejection, "This instruction can't be processed: {{ CommandRejectionReason }}" },
            { PostType.MoveAccepted, "Move accepted - thank you" },
            { PostType.MoveValidation, "{{ ValidationState }} from {{ Username.Full }}: {{ MoveText }}, {{ Detail }}" },
            { PostType.Unspecified, "{{ Text }}" },
        };

	}
}

