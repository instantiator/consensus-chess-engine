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
        private Board board;
        private ChessBoard chessboard;

        public BoardRenderer(Board board)
        {
            this.board = board;
            this.chessboard = ChessBoard.LoadFromFen(board.FEN);
        }

        public SKBitmap RenderBoard(BoardStyle style)
        {
            var backgroundData = BoardGraphicsData.Backgrounds[style];

            var imageInfo = new SKImageInfo(backgroundData.Width, backgroundData.Height);
            using (var bmp = new SKBitmap(imageInfo))
            {
                var canvas = new SKCanvas(bmp);
                canvas.Clear(SKColors.Transparent);

                using (var backgroundBmp = GetImage(backgroundData.Resource))
                    canvas.DrawBitmap(backgroundBmp, 0, 0);

                // render rows backwards so that closer pieces overlay further pieces
                for (short row = 7; row > -1; row--)
                {
                    for (short col = 0; col < 8; col++)
                    {
                        var pc = chessboard[new Position(col, row)]?.ToFenChar();
                        if (pc != null)
                        {
                            var pieceData = BoardGraphicsData.Pieces[style][pc.Value];
                            var piece = GetImage(pieceData.Resource);
                            var renderRow = 7 - row;
                            var x = backgroundData.GridStartX + (col * backgroundData.GridCellWidth) + pieceData.OffsetX;
                            var y = backgroundData.GridStartY + (renderRow * backgroundData.GridCellHeight) - pieceData.Height + pieceData.OffsetY;
                            canvas.DrawBitmap(piece, new SKPoint(x, y));
                        }
                    } // col
                } // row

                // rescale
                canvas.Flush();
                return Enlarge(bmp, backgroundData.ScaleX, backgroundData.ScaleY);
            }
        }

        public static SKBitmap Enlarge(SKBitmap source, int scaleX, int scaleY)
        {
            var width = source.Width * scaleX;
            var height = source.Height * scaleY;

            SKBitmap rescaled = new SKBitmap(width, height);
            SKCanvas canvas = new SKCanvas(rescaled);
            canvas.SetMatrix(SKMatrix.CreateScale(scaleX, scaleY));
            canvas.DrawBitmap(source, new SKPoint());
            canvas.ResetMatrix();
            canvas.Flush();

            return rescaled;
        }

        public static SKBitmap GetImage(string resource)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(BoardRenderer))!;
            Stream stream = assembly.GetManifestResourceStream(resource)!;
            return SKBitmap.Decode(stream);
        }
    }
}
