using System;
using Microsoft.EntityFrameworkCore;

namespace ConsensusChessShared.DTO
{
    public class Commitment : AbstractDTO
	{
		public Game Game { get; set; }
		public Side Side { get; set; }
	}
}

