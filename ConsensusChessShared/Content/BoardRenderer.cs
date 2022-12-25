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

        public SKBitmap GetBlank(int width, int height)
        {
            var imageInfo = new SKImageInfo(width, height);
            var bmp = new SKBitmap(imageInfo);
            var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.Transparent);
            canvas.Save();
            return bmp;
        }

        public SKBitmap RenderBoard(BoardStyle style)
        {
            var composition = BoardGraphicsData.Compositions[style];

            // 1. establish width and generate basic background

            int? width = composition.Width;
            int? height = composition.Height;

            if (composition.Width != null) { width = composition.Width.Value; }
            if (composition.Height != null) { width = composition.Height.Value; }

            SKBitmap? black = null;
            SKBitmap? white = null;

            if (composition.BlackCellResource != null &&
                composition.WhiteCellResource != null)
            {
                black = GetImage(composition.BlackCellResource);
                white = GetImage(composition.WhiteCellResource);

                if (black.Width != white.Width ||
                    black.Height != white.Height)
                    throw new Exception("Expected black and white cells to be of equal size.");
            }

            int cellWidth = composition.GridCellWidth ?? black!.Width;
            int cellHeight = composition.GridCellHeight ?? black!.Height;

            if (width == null) { width = cellWidth * 8; }
            if (height == null) { height = cellHeight * 8; }

            // create the working bitmap
            using (var bmp = composition.BackgroundResource != null
                ? GetImage(composition.BackgroundResource)
                : GetBlank(width!.Value, height!.Value))
            {
                if (width == null || width < bmp.Width) { width = bmp.Width; }
                if (height == null || height < bmp.Height) { height = bmp.Height; }

                var canvas = new SKCanvas(bmp);

                // render rows backwards so that closer pieces overlay further pieces
                bool blackTile = false;
                for (short row = 7; row > -1; row--)
                {
                    for (short col = 0; col < 8; col++)
                    {
                        var renderRow = 7 - row;

                        // background tile
                        if (black != null && white != null)
                        {
                            int bx = composition.GridStartX + (cellWidth * col);
                            int by = composition.GridStartY + (cellHeight * renderRow);
                            var tile = blackTile ? black : white;
                            canvas.DrawBitmap(tile, new SKPoint(bx, by));
                        }

                        // piece
                        var pc = chessboard[new Position(col, row)]?.ToFenChar();
                        if (pc != null)
                        {
                            var pieceData = composition.Pieces[pc.Value];
                            var piece = GetImage(pieceData.Resource);

                            var pieceWidth = pieceData.Width ?? piece.Width;
                            var pieceHeight = pieceData.Height ?? piece.Height;

                            int offsetX = pieceData.OffsetX ?? (cellWidth / 2) - (pieceWidth / 2);
                            int offsetY = pieceData.OffsetY ?? (cellHeight / 2) - (pieceHeight / 2);

                            int x = composition.GridStartX + (col * cellWidth) + offsetX;
                            int y = composition.GridStartY + (renderRow * cellHeight) + offsetY;

                            // draw the "in check" glow if required
                            SKPaint? paint =
                                pc == 'k' && chessboard.BlackKingChecked || pc == 'K' && chessboard.WhiteKingChecked
                                ? CreateCheckGlowPaint()
                                : null;
                            if (paint != null)
                                canvas.DrawBitmap(piece, new SKPoint(x, y), paint);

                            // draw the piece
                            canvas.DrawBitmap(piece, new SKPoint(x, y));

                        }
                        // prep for next cell
                        blackTile = !blackTile;
                    } // col
                      // prep for next row
                    blackTile = !blackTile;
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
