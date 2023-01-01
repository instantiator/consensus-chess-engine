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
        private BoardStyle style;

        public BoardRenderer(BoardStyle style)
        {
            this.style = style;
        }

        public SKBitmap Render(Board board)
        {
            var chessboard = ChessBoard.LoadFromFen(board.FEN);
            var composition = BoardGraphicsData.Compositions[style];

            SKBitmap image;
            image = RenderGrid(composition);
            image = AddPieces(composition, image, chessboard);
            image = AddBackground(composition, image);
            image = ResizeImage(composition, image);
            image = AddMarkers(composition, image);
            return image;
        }

        private SKBitmap RenderGrid(CompositionData composition)
        {
            var black = composition.BlackCellResource != null ? BitmapUtils.GetImage(composition.BlackCellResource) : null;
            var white = composition.WhiteCellResource != null ? BitmapUtils.GetImage(composition.WhiteCellResource) : null;

            var image = BitmapUtils.GetBlank(
                composition.GridPaddingLeft + composition.GridPaddingRight + (composition.GridCellWidth * 8),
                composition.GridPaddingTop + composition.GridPaddingBottom + (composition.GridCellHeight * 8));

            using (var canvas = new SKCanvas(image))
            {
                bool blackTile = false;
                for (short row = 0; row < 8; row++)
                {
                    for (short col = 0; col < 8; col++)
                    {
                        int bx = composition.GridPaddingLeft + composition.GridCellWidth * col;
                        int by = composition.GridPaddingTop + composition.GridCellHeight * row;

                        // background tile
                        var tile = blackTile ? black : white;
                        if (tile != null)
                        {
                            canvas.DrawBitmap(tile, new SKPoint(bx, by));
                        }
                        blackTile = !blackTile;
                    }
                    blackTile = !blackTile;
                }
                canvas.Save();
            }
            return image;
        }

        private SKBitmap AddPieces(CompositionData composition, SKBitmap image, ChessBoard chessboard)
        {
            using (var canvas = new SKCanvas(image))
            {
                // render rows backwards so that closer pieces overlay further pieces
                for (short row = 7; row >= 0; row--)
                {
                    int renderRow = 7 - row;
                    for (short col = 0; col < 8; col++)
                    {
                        int bx = composition.GridPaddingLeft + composition.GridCellWidth * col;
                        int by = composition.GridPaddingTop + composition.GridCellHeight * renderRow;

                        // piece
                        var pc = chessboard[new Position(col, row)]?.ToFenChar();
                        if (pc != null)
                        {
                            var pieceData = composition.Pieces[pc.Value];
                            var piece = BitmapUtils.GetImage(pieceData.Resource);

                            var pieceWidth = piece.Width;
                            var pieceHeight = piece.Height;

                            int offsetX = pieceData.OffsetX ?? (composition.GridCellWidth / 2) - (pieceWidth / 2);
                            int offsetY = pieceData.OffsetY ?? (composition.GridCellHeight / 2) - (pieceHeight / 2);

                            int x = bx + offsetX;
                            int y = by + offsetY;

                            // draw the "in check" glow if required
                            SKPaint? paint =
                                composition.CheckColour != null &&
                                ((pc == 'k' && chessboard.BlackKingChecked) ||
                                 (pc == 'K' && chessboard.WhiteKingChecked))
                                ? CreateShadowPaint(composition.CheckColour.Value, composition.ShadowScale)
                                : null;
                            if (paint != null)
                                canvas.DrawBitmap(piece, new SKPoint(x, y), paint);

                            // draw the piece
                            canvas.DrawBitmap(piece, new SKPoint(x, y));

                        } // piece
                    } // col
                } // row
                canvas.Save();
            }

            return image;
        }

        private SKBitmap AddBackground(CompositionData composition, SKBitmap board)
        {
            if (composition.BackgroundResource != null)
            {
                var background = BitmapUtils.GetImage(composition.BackgroundResource);

                int width, height;
                int backgroundX, backgroundY, boardX, boardY;

                if (composition.GridOffsetX == null)
                {
                    width = Math.Max(background.Width, board.Width);
                    backgroundX = (width / 2) - (background.Width / 2);
                    boardX = (width / 2) - (board.Width / 2);
                }
                else
                {
                    width = Math.Max(background.Width, composition.GridOffsetX.Value + board.Width);
                    backgroundX = 0;
                    boardX = composition.GridOffsetX.Value;
                }

                if (composition.GridOffsetY == null)
                {
                    height = Math.Max(background.Height, board.Height);
                    backgroundY = (height / 2) - (background.Height / 2);
                    boardY = (height / 2) - (board.Height / 2);
                }
                else
                {
                    height = Math.Max(background.Height, composition.GridOffsetY.Value + board.Height);
                    backgroundY = 0;
                    boardY = composition.GridOffsetY.Value;
                }

                var result = BitmapUtils.GetBlank(width, height);
                using (var canvas = new SKCanvas(result))
                {
                    canvas.DrawBitmap(background, backgroundX, backgroundY);
                    canvas.DrawBitmap(board, boardX, boardY);
                    canvas.Save();
                }

                return result;
            }
            else
            {
                return board;
            }
        }

        private SKBitmap ResizeImage(CompositionData composition, SKBitmap image)
        {
            return BitmapUtils.Enlarge(image, composition.ScaleX, composition.ScaleY);
        }

        private SKBitmap AddMarkers(CompositionData composition, SKBitmap board)
        {
            var borderLeft = composition.MarkerBorderLeft * composition.ScaleX;
            var borderRight = composition.MarkerBorderRight * composition.ScaleX;
            var borderTop = composition.MarkerBorderTop * composition.ScaleY;
            var borderBottom = composition.MarkerBorderBottom * composition.ScaleY;

            var gridOffsetX = (composition.GridOffsetX ?? 0) * composition.ScaleX;
            var gridOffsetY = (composition.GridOffsetY ?? 0) * composition.ScaleY;

            var gridPadLeft = composition.GridPaddingLeft * composition.ScaleX;
            var gridPadRight = composition.GridPaddingRight * composition.ScaleX;
            var gridPadTop = composition.GridPaddingTop * composition.ScaleY;
            var gridPadBottom = composition.GridPaddingBottom * composition.ScaleY;

            var gridCellWidth = composition.GridCellWidth * composition.ScaleX;
            var gridCellHeight = composition.GridCellHeight * composition.ScaleY;

            var result = BitmapUtils.GetBlank(
                board.Width + borderLeft + borderRight,
                board.Height + borderTop + borderBottom);

            using (var canvas = new SKCanvas(result))
            {
                canvas.DrawBitmap(board, borderLeft, borderTop);

                var circleScale = 0.7f;

                for (int row = 0; row < 8; row++)
                {
                    for (int col = 0; col < 8; col++)
                    {
                        //var renderRow = 7 - row;
                        int bx = borderLeft + gridOffsetX + gridPadLeft + (gridCellWidth * col);
                        int by = borderTop + gridOffsetY + gridPadTop + (gridCellHeight * row);

                        var rowSymbol = $"{"87654321"[row]}";
                        var colSymbol = $"{"ABCDEFGH"[col]}";

                        float? cx = null;
                        float? cy = null;
                        float? cr = null;

                        // y-axis
                        string? text = null;

                        // left side
                        if (col == 0 && composition.MarkerBorderLeft > 0)
                        {
                            cx = borderLeft / 2;
                            cy = by + (gridCellHeight / 2);
                            cr = circleScale * borderLeft / 2;
                            text = rowSymbol;
                        }
                        // right side
                        if (col == 7 && composition.MarkerBorderRight > 0)
                        {
                            cx = result.Width - (borderRight / 2);
                            cy = by + (gridCellHeight / 2);
                            cr = circleScale * borderRight / 2;
                            text = rowSymbol;
                        }

                        if (text != null)
                        {
                            RenderMarker(canvas,
                                cx!.Value, cy!.Value, cr!.Value,
                                text!,
                                composition.MarkerColour!.Value,
                                composition.MarkerBackground!.Value,
                                composition.MarkerShadow!.Value,
                                composition.ShadowScale);
                        }

                        // x-axis
                        text = null;

                        // top
                        if (row == 0 && composition.MarkerBorderTop > 0)
                        {
                            cx = bx + (gridCellWidth / 2);
                            cy = borderTop / 2;
                            cr = circleScale * borderTop / 2;
                            text = colSymbol;
                        }
                        // bottom
                        if (row == 7 && composition.MarkerBorderBottom > 0)
                        {
                            cx = bx + (gridCellWidth / 2);
                            cy = result.Height - (borderBottom / 2);
                            cr = circleScale * borderBottom / 2;
                            text = colSymbol;
                        }

                        if (text != null)
                        {
                            RenderMarker(canvas,
                                cx!.Value, cy!.Value, cr!.Value,
                                text!,
                                composition.MarkerColour!.Value,
                                composition.MarkerBackground!.Value,
                                composition.MarkerShadow!.Value,
                                composition.ShadowScale);
                        }
                    }
                }
                canvas.Save();
            }
            return result;
        }

        private void RenderMarker(SKCanvas canvas, float cx, float cy, float cr, string text, SKColor fc, SKColor bc, SKColor sc, float shadowScale)
        {
            var background = new SKPaint() { Color = bc, IsAntialias = true };
            var foreground = new SKPaint() { Color = fc, IsAntialias = true };
            var shadow = CreateShadowPaint(sc, shadowScale);

            var textBounds = new SKRect();
            foreground.TextSize = cr * 1.5f;
            foreground.FakeBoldText = true;
            foreground.MeasureText(text, ref textBounds);

            float tx = cx - textBounds.MidX;
            float ty = cy - textBounds.MidY;

            canvas.DrawCircle(cx, cy, cr, shadow);
            canvas.DrawCircle(cx, cy, cr, background);
            canvas.DrawText(text, tx, ty, foreground);
        }

        private SKPaint CreateShadowPaint(SKColor colour, float scale)
        {
            var paint = new SKPaint() { IsAntialias = true };
            paint.ImageFilter =
                SKImageFilter.CreateDropShadowOnly(
                    dx: 0.0f, dy: 0.0f,
                    sigmaX: 8.0f * scale, sigmaY: 8.0f * scale,
                    colour);
            return paint;
        }
    }
}
