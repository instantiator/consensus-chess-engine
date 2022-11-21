﻿using System;
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
            { PostType.BoardUpdate, "New board. You have {{ Game.MoveDuration }} to vote.\n{{ FEN }}" },
            { PostType.CommandResponse, "{{ Text }}" },
            { PostType.GameAnnouncement, "New {{ Game.SideRules }} game...\nWhite: {{ WhiteParticipantNetworkServers }}\nBlack: {{ BlackParticipantNetworkServers }}\nMove duration: {{ Game.MoveDuration }}" },
            { PostType.MoveValidation, "{{ Text }}" },
            { PostType.Unspecified, "{{ Text }}" },
        };

	}
}

