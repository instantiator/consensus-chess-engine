using System;
using Chess;
using static ConsensusChessShared.Content.BoardRenderer;

namespace ConsensusChessShared.Content
{
	public class BoardGraphicsData
	{
        public struct PieceData
        {
            public string Resource;
        }

        public static Dictionary<BoardStyle, Dictionary<char, PieceData>> Pieces =
            new Dictionary<BoardStyle, Dictionary<char, PieceData>>()
            {
                {
                    BoardStyle.PixelChess,
                    new Dictionary<char, PieceData>()
                    {
                        { 'R', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Rook.png" } },
                        { 'K', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_King.png" } },
                        { 'N', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Knight.png" } },
                        { 'P', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Pawn.png" } },
                        { 'Q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Queen.png" } },
                        { 'B', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Bishop.png" } },

                        { 'r', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Rook.png" } },
                        { 'k', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_King.png" } },
                        { 'n', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Knight.png" } },
                        { 'p', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Pawn.png" } },
                        { 'q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Queen.png" } },
                        { 'b', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Bishop.png" } },
                    }
                }
            };


    }
}

