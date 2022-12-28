using System;
using System.ComponentModel;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Reflection;
using Chess;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Graphics;
using SkiaSharp;
using static ConsensusChessShared.Graphics.BoardGraphicsData;

namespace ConsensusChessShared.Graphics
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

        public SKBitmap Render(BoardStyle style)
        {
            SKBitmap? background;
            SKBitmap image;
            CalculatedDimensions dimensions;

            var composition = BoardGraphicsData.Compositions[style];
            background = RenderBackground(composition);
            (image, dimensions) = RenderGrid(composition, background);
            image = RenderPieces(composition, image, dimensions);
            image = ResizeImage(composition, image);
            //image = AddMarkers(composition, image);
            background?.Dispose();
            return image;
        }

        private struct CalculatedDimensions
        {
            public int? CalculatedWidth;
            public int? CalculatedHeight;
            public int CalculatedCellWidth;
            public int CalculatedCellHeight;
        }

        private SKBitmap ResizeImage(CompositionData composition, SKBitmap image)
        {
            return BitmapUtils.Enlarge(image, composition.ScaleX, composition.ScaleY);
        }

        //private SKBitmap AddMarkers(CompositionData data, SKBitmap board)
        //{
        //    var result = BitmapUtils.GetBlank(
        //        board.Width + data.MarkerSize!.Value * 2,
        //        board.Height + data.MarkerSize!.Value * 2);

        //    using (var canvas = new SKCanvas(result))
        //    {

        //        var renderRow = 7 - row;

        //        int bx = composition.GridStartX + (cellWidth * col);
        //        int by = composition.GridStartY + (cellHeight * renderRow);


        //        // y-axis
        //        if (col == 0)
        //        {
        //            var sym = $"{row + 1}";
        //            //TODO: how to render the character
        //        }

        //        // TODO: x-axis
        //        if (row == 0)
        //        {
        //            var sym = $"{"ABCDEFGH"[col]}";
        //            //TODO: how to render the character
        //        }




        //        canvas.Save();
        //    }



        //}

        private (SKBitmap,CalculatedDimensions) RenderGrid(CompositionData composition, SKBitmap? background)
        {
            if ((composition.GridCellWidth == null || composition.GridCellHeight == null) &&
                (composition.BlackCellResource == null || composition.WhiteCellResource == null))
            {
                throw new ArgumentNullException("Neither grid width or tile resources are provided.");
            }

            SKBitmap? black = null;
            SKBitmap? white = null;

            if (composition.BlackCellResource != null &&
                composition.WhiteCellResource != null)
            {
                black = BitmapUtils.GetImage(composition.BlackCellResource);
                white = BitmapUtils.GetImage(composition.WhiteCellResource);

                if (black.Width != white.Width ||
                    black.Height != white.Height)
                    throw new Exception("Expected black and white cells to be of equal size.");
            }

            int cellWidth = composition.GridCellWidth ?? black!.Width;
            int cellHeight = composition.GridCellHeight ?? black!.Height;

            if (background == null)
            {
                background = BitmapUtils.GetBlank(
                    composition.GridMarginLeft + (cellWidth * 8) + composition.GridMarginRight,
                    composition.GridMarginTop + (cellHeight * 8) + composition.GridMarginBottom);
            }

            using (var canvas = new SKCanvas(background))
            {

                // render rows backwards so that closer pieces overlay further pieces
                bool blackTile = false;
                for (short row = 7; row > -1; row--)
                {
                    for (short col = 0; col < 8; col++)
                    {
                        var renderRow = 7 - row;

                        int bx = composition.GridMarginLeft + (cellWidth * col);
                        int by = composition.GridMarginTop + (cellHeight * renderRow);

                        // background tile
                        if (black != null && white != null)
                        {
                            var tile = blackTile ? black : white;
                            canvas.DrawBitmap(tile, new SKPoint(bx, by));
                        }
                        blackTile = !blackTile;
                    }
                    blackTile = !blackTile;
                }
                canvas.Save();
            }

            var dimensions = new CalculatedDimensions()
            {
                CalculatedCellHeight = cellHeight,
                CalculatedCellWidth = cellWidth
            };

            return (background, dimensions);
        }

        private SKBitmap? RenderBackground(CompositionData composition)
        {
            if (composition.Width != null && composition.Height != null)
            {
                var background = BitmapUtils.GetBlank(composition.Width!.Value, composition.Height!.Value);
                if (composition.BackgroundResource != null)
                {
                    using (var canvas = new SKCanvas(background))
                    {
                        var overlay = BitmapUtils.GetImage(composition.BackgroundResource);
                        var x = (background.Width / 2) - (overlay.Width / 2);
                        var y = (background.Height / 2) - (overlay.Height / 2);
                        canvas.DrawBitmap(overlay, new SKPoint(x, y));
                        canvas.Save();
                    }
                }
                return background;
            }
            else
            {
                if (composition.BackgroundResource != null)
                {
                    return BitmapUtils.GetImage(composition.BackgroundResource!);
                }
                else
                {
                    return null;
                }
            }
        }

        private SKBitmap RenderPieces(CompositionData composition, SKBitmap grid, CalculatedDimensions calculations)
        {
            using (var canvas = new SKCanvas(grid))
            {
                // render rows backwards so that closer pieces overlay further pieces
                bool blackTile = false;
                for (short row = 7; row > -1; row--)
                {
                    for (short col = 0; col < 8; col++)
                    {
                        var renderRow = 7 - row;

                        int bx = composition.GridMarginLeft + (calculations.CalculatedCellWidth * col);
                        int by = composition.GridMarginTop + (calculations.CalculatedCellHeight * renderRow);

                        // piece
                        var pc = chessboard[new Position(col, row)]?.ToFenChar();
                        if (pc != null)
                        {
                            var pieceData = composition.Pieces[pc.Value];
                            var piece = BitmapUtils.GetImage(pieceData.Resource);

                            var pieceWidth = pieceData.Width ?? piece.Width;
                            var pieceHeight = pieceData.Height ?? piece.Height;

                            int offsetX = pieceData.OffsetX ?? (calculations.CalculatedCellWidth / 2) - (pieceWidth / 2);
                            int offsetY = pieceData.OffsetY ?? (calculations.CalculatedCellHeight / 2) - (pieceHeight / 2);

                            int x = bx + offsetX;
                            int y = by + offsetY;

                            // draw the "in check" glow if required
                            SKPaint? paint =
                                composition.CheckColour != null && pc.ToString()!.ToLower() == "k" &&
                                (chessboard.BlackKingChecked || chessboard.WhiteKingChecked)
                                ? CreateCheckGlowPaint(composition.CheckColour.Value)
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
                canvas.Save();
            }

            return grid;
        }

        private SKPaint CreateCheckGlowPaint(SKColor colour)
        {
            var paint = new SKPaint();
            paint.ImageFilter =
                SKImageFilter.CreateDropShadowOnly(
                    dx: 0.0f, dy: 0.0f,
                    sigmaX: 8.0f, sigmaY: 8.0f,
                    colour);
            return paint;
        }

    }
}
