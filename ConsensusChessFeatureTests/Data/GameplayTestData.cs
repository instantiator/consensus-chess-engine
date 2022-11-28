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
            FoolsMateSAN,
            FoolsMateCCF
        }

        public enum Prefix
        {
            NoPrefix,
            Move
        }

        public static Dictionary<StartingPosition, string> FEN_positions = new Dictionary<StartingPosition, string>()
        {
            { StartingPosition.Standard, "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" }
        };

        public static Dictionary<Reenactment, string[]> MOVE_sequences = new Dictionary<Reenactment, string[]>()
        {
            { Reenactment.FoolsMateSAN, new[] { "f3", "e6", "g4", "Qh4" } },
            { Reenactment.FoolsMateCCF, new[] { "f2 - f3", "e7 - e6", "g2 - g4", "d8 - h4" } },
        };
    }
}

