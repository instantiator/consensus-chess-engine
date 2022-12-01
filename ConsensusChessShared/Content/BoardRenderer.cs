using System;
using System.Drawing;
using System.Net.NetworkInformation;
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
            var composition = BoardGraphicsData.Compositions[style];
            var imageInfo = new SKImageInfo(composition.Width, composition.Height);
            using (var bmp = new SKBitmap(imageInfo))
            {
                var canvas = new SKCanvas(bmp);
                canvas.Clear(SKColors.Transparent);

                using (var backgroundBmp = GetImage(composition.Resource))
                    canvas.DrawBitmap(backgroundBmp, 0, 0);

                // render rows backwards so that closer pieces overlay further pieces
                for (short row = 7; row > -1; row--)
                {
                    for (short col = 0; col < 8; col++)
                    {
                        var pc = chessboard[new Position(col, row)]?.ToFenChar();
                        if (pc != null)
                        {
                            var pieceData = composition.Pieces[pc.Value];
                            var piece = GetImage(pieceData.Resource);
                            var renderRow = 7 - row;
                            var x = composition.GridStartX + (col * composition.GridCellWidth) + pieceData.OffsetX;
                            var y = composition.GridStartY + (renderRow * composition.GridCellHeight) - pieceData.Height + pieceData.OffsetY;

                            // draw the glow if required
                            SKPaint? paint =
                                pc == 'k' && chessboard.BlackKingChecked || pc == 'K' && chessboard.WhiteKingChecked
                                ? CreateCheckGlowPaint()
                                : null;
                            if (paint != null)
                                canvas.DrawBitmap(piece, new SKPoint(x, y), paint);

                            // draw the piece
                            canvas.DrawBitmap(piece, new SKPoint(x, y));
                        }
                    } // col
                } // row

                // rescale
                canvas.Flush();
                return Enlarge(bmp, composition.ScaleX, composition.ScaleY);
            }
        }

        public SKPaint CreateCheckGlowPaint()
        {
            var paint = new SKPaint();
            paint.ImageFilter =
                SKImageFilter.CreateDropShadowOnly(
                    dx: 0.0f, dy: 0.0f,
                    sigmaX: 8.0f, sigmaY: 8.0f,
                    SKColors.Red);
            return paint;
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
