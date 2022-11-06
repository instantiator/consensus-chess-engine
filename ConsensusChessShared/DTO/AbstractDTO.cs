using System;
namespace ConsensusChessShared.DTO
{
	public abstract class AbstractDTO
	{
		public Guid Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}

