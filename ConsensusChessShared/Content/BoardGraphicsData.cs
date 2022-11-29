using System;
using Chess;
using static ConsensusChessShared.Content.BoardRenderer;

namespace ConsensusChessShared.Content
{
	public class BoardGraphicsData
	{
        public enum BoardStyle
        {
            PixelChess,
        }

        public struct PieceData
        {
            public int Width;
            public int Height;
            public int OffsetX;
            public int OffsetY;
            public string Resource;
        }

        public struct BackgroundData
        {
            public int Width;
            public int Height;
            public int GridStartX;
            public int GridCellWidth;
            public int GridStartY;
            public int GridCellHeight;
            public string Resource;
            public int ScaleX;
            public int ScaleY;
        }

        public static Dictionary<BoardStyle, BackgroundData> Backgrounds =
            new Dictionary<BoardStyle, BackgroundData>()
            {
                {
                    BoardStyle.PixelChess,
                    new BackgroundData()
                    {
                        Resource = "ConsensusChessShared.Images.PixelChess.Board.png",
                        Width = 142, Height = 142,
                        GridStartX = 7,
                        GridCellWidth = 16,
                        GridStartY = 36,
                        GridCellHeight = 11,
                        ScaleX = 4,
                        ScaleY = 4
                    }
                }
            };

        public static Dictionary<BoardStyle, Dictionary<char, PieceData>> Pieces =
            new Dictionary<BoardStyle, Dictionary<char, PieceData>>()
            {
                {
                    BoardStyle.PixelChess,
                    new Dictionary<char, PieceData>()
                    {
                        { 'R', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Rook.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'K', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_King.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'N', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Knight.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'P', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Pawn.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'Q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Queen.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'B', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Bishop.png", Width = 16, Height = 32, OffsetY = -1 } },

                        { 'r', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Rook.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'k', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_King.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'n', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Knight.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'p', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Pawn.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Queen.png", Width = 16, Height = 32, OffsetY = -1 } },
                        { 'b', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Bishop.png", Width = 16, Height = 32, OffsetY = -1 } },
                    }
                }
            };


    }
}

