using System;
using System.Drawing;
using System.Reflection;
using Chess;
using ConsensusChessShared.DTO;
using SkiaSharp;
using static ConsensusChessShared.Content.BoardGraphicsData;

namespace ConsensusChessShared.Content
{
    public class BoardRenderer
    {
        public enum BoardStyle
        {
            PixelChess,
        }

        private Board board;
        private ChessBoard chessboard;

        public BoardRenderer(Board board)
        {
            this.board = board;
            this.chessboard = ChessBoard.LoadFromFen(board.FEN);
        }

        public SKBitmap RenderBoard(BoardStyle style)
        {
            var width = 16 * 8;
            var height = 32 * 8;

            var imageInfo = new SKImageInfo(width, height);
            var bmp = new SKBitmap(imageInfo);
            var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.White);

            for (short row = 0; row < 8; row++)
            {
                for (short col = 0; col < 8; col++)
                {
                    var pc = chessboard[new Position(col, row)]?.ToFenChar();

                    if (pc != null)
                    {
                        var pieceData = BoardGraphicsData.Pieces[style][pc.Value];
                        var piece = GetImage(pieceData);
                        var x = col * 16;
                        var y = row * 32;
                        canvas.DrawBitmap(piece, new SKPoint(x, y));
                    }
                } // col
            } // row

            return bmp;
        }

        private SKBitmap GetImage(PieceData data)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(BoardRenderer))!;
            Stream stream = assembly.GetManifestResourceStream(data.Resource)!;
            return SKBitmap.Decode(stream);
        }
    }
}
