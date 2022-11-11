using System;
namespace ConsensusChessShared.DTO
{
	public interface IDTO
	{
		Guid Id { get; }
		DateTime Created { get; }
	}
}

