using System;
namespace ConsensusChessFeatureTests.Data
{
	public class GameplayTestData
	{
        public enum StartingPosition
        {
            Standard
        }

        public enum Reenactment
        {
            FoolsMate
        }

        public static Dictionary<StartingPosition, string> FEN_positions = new Dictionary<StartingPosition, string>()
        {
            { StartingPosition.Standard, "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" }
        };

        public static Dictionary<Reenactment, string[]> MOVE_sequences = new Dictionary<Reenactment, string[]>()
        {
            { Reenactment.FoolsMate, new[] { "f3", "e6", "g4", "Qh4" } }
        };
    }
}

