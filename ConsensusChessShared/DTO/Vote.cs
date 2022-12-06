using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ConsensusChessShared.Constants;

namespace ConsensusChessShared.DTO
{
	public class Vote : IDTO
    {
		public Vote()
		{
            Created = DateTime.Now.ToUniversalTime();
			ValidationState = VoteValidationState.Unchecked;
        }

		public Vote(
			string postId, string raw,
			Participant participant,
            string? san = null,
            VoteValidationState validation = VoteValidationState.Unchecked,
			Post? validationPost = null) : this()
		{
			NetworkMovePostId = postId;
			MoveText = raw;
			MoveSAN = san;
			Participant = participant;
			ValidationState = validation;
			ValidationPost = validationPost;
		}

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public DateTime Created { get; set; }

		public string NetworkMovePostId { get; set; }
		public string MoveText { get; set; }
		public virtual Participant Participant { get; set; }

		public VoteValidationState ValidationState { get; set; }
		public string? MoveSAN { get; set; }
		public virtual Post? ValidationPost { get; set; }
	}
}

