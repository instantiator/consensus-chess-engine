using System;
namespace ConsensusChessShared.DTO
{
	public abstract class AbstractDTO
	{
        protected AbstractDTO()
        {
            Id = Guid.NewGuid();
            Created = DateTime.Now.ToUniversalTime();
        }

		public Guid Id { get; set; }
        public DateTime Created { get; set; }
    }
}

