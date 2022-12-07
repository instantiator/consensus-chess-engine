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
            { PostType.SocialStatus, "⚙️ {{ State.Name }} ({{ State.Shortcode }}): {{ SocialStatus }}" },

            { PostType.Engine_GameCreationResponse, "👍 New {{ Game.SideRules }} game for: {{ AllNodes }}" },
            { PostType.Engine_GameAnnouncement, "⚙️ 🆕 New {{ Game.SideRules }} game started.\nWhite: {{ WhiteParticipantNetworkServers }}\nBlack: {{ BlackParticipantNetworkServers }}\nMove duration: {{ Game.MoveDuration }}" },
            { PostType.Engine_GameAbandoned, "⚙️ ⏹ Game {{ Game.Shortcode }}: {{ Game.State }}" },
            { PostType.Engine_GameEnded, "⚙️ ⏹ Game {{ Game.Shortcode }}: {{ Game.State }}" },
            { PostType.Engine_GameAdvance, "⚙️ ⏭ Game advanced: {{ Game.Shortcode }}" },

            { PostType.Node_GameAnnouncement, "📢 🆕 A new game is starting {{ StartTimeDescription }}... \n\n{{ Game.Title }}\n\n{{ Game.Description }}\n\n🔔 Enable notifications from this account, or follow {{ Config.GameTag }}, to join in!" },
            { PostType.Node_BoardUpdate, "🎉 ▶️ There's a new board for the {{ Game.Title }} game. {{ BoardDescription }}\n\nReply to this message to vote for {{ Game.CurrentSide }}'s next move. The votes will be tallied after {{ FormattedGameMoveDuration }}.\n\n{{ Config.GameTag }}" },
            { PostType.Node_BoardReminder, "🎗 A reminder - it's almost time to count the votes for the {{ Game.Title}} game. {{ BoardDescription }}\n\nIf you're planning to vote for {{ Game.CurrentSide }}, you have {{ FormattedMoveTimeRemaining }} to make your move!\n\n🧵 Instructions in thread... {{ Config.GameTag }}" },
            { PostType.Node_VotingInstructions, "ℹ️ How to play:\nPick a side. On your turn, vote for the move your side should make with a reply. Provide coordinates for the square you want to move from, and the square you want to move to - separated by a hyphen. eg.\n\nc2 - c4\n\n{{ Config.GameTag }}" },
            { PostType.Node_FollowInstructions, "🔔 Enable notifications from this account, or follow {{ Config.GameTag }} to hear about each move as it happens!" },
            { PostType.Node_GameAbandonedUpdate, "😶 The {{ Game.Title }} game was abandoned. This can happen if there are no votes for one side, or if it is actively cancelled by an administrator." },
            { PostType.Node_GameEndedUpdate, "🧐 The {{ Game.Title }} game has ended in state: {{ GameState }}\n\n{{ Config.GameTag }}" },

            { PostType.Node_GameNotFound,   "😔 This vote can't be processed: {{ GameNotFoundReason }}" },
            { PostType.CommandRejection,    "😔 {{ CommandRejectionDescription }}\n\n{{ ItemsSummary }}" },
            { PostType.Node_MoveValidation, "😔 {{ ValidationDescription }} Please check your vote, and reply to the board post to try again.\n\nThe votes will be tallied in {{ FormattedMoveTimeRemaining }}.\n\nIf you think this is an error, please reach out to {{ Config.AdminContact }}." },
            { PostType.Node_VoteAccepted,   "⭐️ Vote accepted - thank you.\n\nThe SAN (standard algebraic notation) for your move is: {{ SAN }}\n\nThe votes will be tallied in {{ FormattedMoveTimeRemaining }}." },
            { PostType.Node_VoteSuperceded, "↔️ New vote accepted - thank you.\n\nThe SAN for your new move is: {{ SAN }}\n\nThis supercedes your previous move: {{ PreviousSAN }}\n\nThe votes will be tallied in {{ FormattedMoveTimeRemaining }}." },

            { PostType.CommandResponse, "{{ Text }}" },
            { PostType.Unspecified,     "{{ Text }}" },
        };

	}
}

