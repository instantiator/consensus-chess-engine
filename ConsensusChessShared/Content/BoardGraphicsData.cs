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
            JPCB
        }

        public struct PieceData
        {
            public int? Width; // if null, calculate from the resource
            public int? Height; // if null, calculate from the resource
            public int? OffsetX; // if null, centre the piece in the cell
            public int? OffsetY; // if null, centre the piece in the cell
            public string Resource;
        }

        public struct CompositionData
        {
            public int? Width; // if null, calculate from the grid cell width (*8)
            public int? Height; // if null, calculate from the grid cell height (*8)
            public int? GridCellWidth; // if null, calculate from the black cell resource
            public int? GridCellHeight; // if null, calculate from the white cell resource
            public int GridStartX;
            public int GridStartY;
            public string? BackgroundResource;
            public string? BlackCellResource;
            public string? WhiteCellResource;
            public int ScaleX;
            public int ScaleY;
            public Dictionary<char, PieceData> Pieces;
        }

        public static Dictionary<BoardStyle, Dictionary<char, PieceData>> Pieces =
            new Dictionary<BoardStyle, Dictionary<char, PieceData>>()
            {
                {
                    BoardStyle.PixelChess,
                    new Dictionary<char, PieceData>()
                    {
                        { 'R', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Rook.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'K', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_King.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'N', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Knight.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'P', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Pawn.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'Q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Queen.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'B', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Bishop.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'r', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Rook.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'k', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_King.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'n', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Knight.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'p', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Pawn.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Queen.png", Width = 16, Height = 32, OffsetY = -33 } },
                        { 'b', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Bishop.png", Width = 16, Height = 32, OffsetY = -33 } },
                    }
                },
                {
                    BoardStyle.JPCB,
                    new Dictionary<char, PieceData>()
                    {
                        { 'R', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.w_rook_2x.png" } },
                        { 'K', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.w_king_2x.png" } },
                        { 'N', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.w_knight_2x.png" } },
                        { 'P', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.w_pawn_2x.png" } },
                        { 'Q', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.w_queen_2x.png" } },
                        { 'B', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.w_bishop_2x.png" } },
                        { 'r', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.b_rook_2x.png" } },
                        { 'k', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.b_king_2x.png" } },
                        { 'n', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.b_knight_2x.png" } },
                        { 'p', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.b_pawn_2x.png" } },
                        { 'q', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.b_queen_2x.png" } },
                        { 'b', new PieceData() { Resource = "ConsensusChessShared.Images.JPCB.b_bishop_2x.png" } },
                    }
                }
            };

        public static Dictionary<BoardStyle, CompositionData> Compositions =
            new Dictionary<BoardStyle, CompositionData>()
            {
                // PixelChess, by Dani Maccari
                // See: https://dani-maccari.itch.io/pixel-chess
                {
                    BoardStyle.PixelChess,
                    new CompositionData()
                    {
                        BackgroundResource = "ConsensusChessShared.Images.PixelChess.Board.png",
                        BlackCellResource = null,
                        WhiteCellResource = null,
                        Width = 142, Height = 142,
                        GridStartX = 7,
                        GridCellWidth = 16,
                        GridStartY = 36,
                        GridCellHeight = 11,
                        ScaleX = 4,
                        ScaleY = 4,
                        Pieces = Pieces![BoardStyle.PixelChess]
                    }
                },
                // Graphics published on OpenGameArt by JohnPablok
                // See: https://opengameart.org/content/chess-pieces-and-board-squares
                // See: https://opengameart.org/users/johnpablok
                {
                    BoardStyle.JPCB,
                    new CompositionData()
                    {
                        BackgroundResource = null,
                        BlackCellResource = "ConsensusChessShared.Images.JPCB.square_brown_dark_2x.png",
                        WhiteCellResource = "ConsensusChessShared.Images.JPCB.square_brown_light_2x.png",
                        Width = null, Height = null, // calculated
                        GridCellWidth = null, // calculated
                        GridCellHeight = null, // calculated
                        GridStartX = 0,
                        GridStartY = 0,
                        ScaleX = 1,
                        ScaleY = 1,
                        Pieces = Pieces![BoardStyle.JPCB]
                    }
                }
            };
    }
}

