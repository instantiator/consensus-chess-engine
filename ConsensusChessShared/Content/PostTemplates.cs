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

        private IHandlebars handlebars;

        public PostTemplates()
        {
            handlebars = Handlebars.Create(new HandlebarsConfiguration()
            {
                ThrowOnUnresolvedBindingExpression = true
            });

            For = TemplateSource
                .Select(pair => KeyValuePair.Create(pair.Key, handlebars.Compile(pair.Value)))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public static Dictionary<PostType, string> TemplateSource = new Dictionary<PostType, string>()
        {
            { PostType.SocialStatus, "{{ State.Name }} ({{ State.Shortcode }}): {{ SocialStatus }}" },
            { PostType.CommandResponse, "{{ Text }}" },

            { PostType.Engine_GameCreationResponse, "New {{ Game.SideRules }} game for: {{ AllNodes }}" },
            { PostType.Engine_GameAnnouncement, "New {{ Game.SideRules }} game started.\nWhite: {{ WhiteParticipantNetworkServers }}\nBlack: {{ BlackParticipantNetworkServers }}\nMove duration: {{ Game.MoveDuration }}" },
            { PostType.Engine_GameAbandoned, "Game {{ Game.Shortcode }}: {{ Game.State }}" },
            { PostType.Engine_GameEnded, "Game {{ Game.Shortcode }}: {{ Game.State }}" },
            { PostType.Engine_GameAdvance, "Game advanced: {{ Game.Shortcode }}" },

            { PostType.Node_BoardUpdate, "🆕📢 There's a new board for the {{ Game.Title }} game. {{ BoardDescription }}\n\nReply to this message to vote for {{ Game.CurrentSide }}'s next move. The votes will be tallied after {{ FormattedGameMoveDuration }}." },
            { PostType.Node_BoardReminder, "🎗 A reminder - it's almost time to count the votes for the {{ Game.Title}} game.\n\n{{ BoardDescription }}\n\nIf you're planning to vote for {{ Game.CurrentSide }}, you have {{ FormattedMoveTimeRemaining }} to make your move!" },
            { PostType.Node_VotingInstructions, "ℹ️ How to play:\nVote for the move your side should make by replying to the post with the board on it. Provide coordinates for the square you want to move from, and the square you want to move to - separated by a hyphen.\n\neg. c2 - c4\n\n🔔 Enable notifications from this account to hear about each move as it happens!" },
            { PostType.Node_GameAbandonedUpdate, "The {{ Game.Title }} game was abandoned. This can happen if there are no votes for one side, or if it is actively cancelled by an administrator." },
            { PostType.Node_GameEndedUpdate, "The {{ Game.Title }} game has ended in state: {{ Game.State }}" },

            { PostType.GameNotFound, "This vote can't be processed: {{ GameNotFoundReason }}" },
            { PostType.CommandRejection, "This instruction can't be processed: {{ CommandRejectionReason }} {{ ItemsSummary }}" },
            { PostType.MoveAccepted, "Move accepted - thank you.\n\nThe standard algebraic notation (SAN) for your move is: {{ SAN }}" },
            { PostType.MoveValidation, "Unfortunately, this move couldn't be interpreted. This might be because it's not a valid move, or because it wasn't in the right format.\n\nYou can try something different by replying to the board.\n\nIf you think this is an error, please reach out to {{ AdminContact }}.\n\nDetail: {{ ValidationState }}: {{ MoveText }}\n{{ Detail }}" },

            { PostType.Unspecified, "{{ Text }}" },
        };

	}
}

