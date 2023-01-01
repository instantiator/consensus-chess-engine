using System;
using Chess;
using SkiaSharp;

namespace ConsensusChessShared.Graphics
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
            public int? OffsetX; // if null, centre the piece in the cell
            public int? OffsetY; // if null, centre the piece in the cell
            public string Resource;
        }

        public struct CompositionData
        {
            public int GridCellWidth;
            public int GridCellHeight;

            public int? GridOffsetX; // if null, centre the board on the background
            public int? GridOffsetY; // if null, centre the board on the background

            public int GridPaddingLeft;
            public int GridPaddingRight;
            public int GridPaddingTop;
            public int GridPaddingBottom;

            public string? BackgroundResource;

            public string? BlackCellResource;
            public string? WhiteCellResource;

            public int MarkerBorderLeft;
            public int MarkerBorderRight;
            public int MarkerBorderTop;
            public int MarkerBorderBottom;

            public int ScaleX;
            public int ScaleY;

            public SKColor? CheckColour;
            public SKColor? MarkerColour;
            public SKColor? MarkerBackground;
            public SKColor? MarkerShadow;
            public float ShadowScale;

            public Dictionary<char, PieceData> Pieces;
        }

        public static Dictionary<BoardStyle, Dictionary<char, PieceData>> Pieces =
            new Dictionary<BoardStyle, Dictionary<char, PieceData>>()
            {
                {
                    BoardStyle.PixelChess,
                    new Dictionary<char, PieceData>()
                    {
                        { 'R', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Rook.png", OffsetY = -22 } },
                        { 'K', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_King.png", OffsetY = -22 } },
                        { 'N', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Knight.png", OffsetY = -22 } },
                        { 'P', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Pawn.png", OffsetY = -22 } },
                        { 'Q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Queen.png", OffsetY = -22 } },
                        { 'B', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.W_Bishop.png", OffsetY = -22 } },
                        { 'r', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Rook.png", OffsetY = -22 } },
                        { 'k', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_King.png", OffsetY = -22 } },
                        { 'n', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Knight.png", OffsetY = -22 } },
                        { 'p', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Pawn.png", OffsetY = -22 } },
                        { 'q', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Queen.png", OffsetY = -22 } },
                        { 'b', new PieceData() { Resource = "ConsensusChessShared.Images.PixelChess.B_Bishop.png", OffsetY = -22 } },
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
                        BackgroundResource = "ConsensusChessShared.Images.PixelChess.Board_top_border.png",
                        BlackCellResource = null,
                        WhiteCellResource = null,
                        GridCellWidth = 16, GridCellHeight = 11,
                        GridOffsetX = 0, GridOffsetY = -15,
                        GridPaddingLeft = 7, GridPaddingRight = 7,
                        GridPaddingTop = 32, GridPaddingBottom = 0,
                        ScaleX = 4,
                        ScaleY = 4,
                        Pieces = Pieces![BoardStyle.PixelChess],
                        CheckColour = SKColors.DarkRed,
                        MarkerBorderLeft = 11, MarkerBorderRight = 0,
                        MarkerBorderTop = 0, MarkerBorderBottom = 11,
                        MarkerColour = SKColors.White,
                        MarkerBackground = SKColors.DimGray,
                        MarkerShadow = SKColors.Transparent,
                        ShadowScale = 0.5f
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
                        GridCellWidth = 901, GridCellHeight = 901,
                        GridOffsetX = null, GridOffsetY = null,
                        GridPaddingLeft = 0, GridPaddingRight = 0,
                        GridPaddingTop = 0, GridPaddingBottom = 0,
                        ScaleX = 1,
                        ScaleY = 1,
                        Pieces = Pieces![BoardStyle.JPCB],
                        CheckColour = SKColors.DarkRed,
                        MarkerBorderLeft = 400, MarkerBorderRight = 400,
                        MarkerBorderTop = 400, MarkerBorderBottom = 400,
                        MarkerColour = SKColors.White,
                        MarkerBackground = SKColors.SaddleBrown,
                        MarkerShadow = SKColors.Transparent,
                        ShadowScale = 25f
                    }
                }
            };
    }
}

