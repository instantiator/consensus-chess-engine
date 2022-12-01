﻿using System;
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

            { PostType.Node_BoardUpdate, "New board for game {{ Game.Title }}:\n{{ BoardDescription }}\nYou have {{ FormattedGameMoveDuration }} to vote." },
            { PostType.Node_GameAbandonedUpdate, "{{ Game.Title }} was abandoned." },
            { PostType.Node_GameEndedUpdate, "{{ Game.Title }} ended: {{ Game.State }}" },

            { PostType.GameNotFound, "This vote can't be processed: {{ GameNotFoundReason }}" },
            { PostType.CommandRejection, "This instruction can't be processed: {{ CommandRejectionReason }} {{ ItemsSummary }}" },
            { PostType.MoveAccepted, "Move accepted - thank you" },
            { PostType.MoveValidation, "{{ ValidationState }} from {{ Username.Full }}: {{ MoveText }}, {{ Detail }}" },

            { PostType.Unspecified, "{{ Text }}" },
        };

	}
}

