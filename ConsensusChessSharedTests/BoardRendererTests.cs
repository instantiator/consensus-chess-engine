using System;
using System.Drawing;
using System.Reflection;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using ConsensusChessShared.Graphics;
using ConsensusChessSharedTests.Data;
using SkiaSharp;
using static ConsensusChessShared.Graphics.BoardGraphicsData;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class BoardRendererTests
	{
        [DataRow(BoardStyle.PixelChess)]
        [DataRow(BoardStyle.JPCB)]
        [DataTestMethod]
        public void ImagesAvailable(BoardStyle style)
        {

            Assembly assembly = Assembly.GetAssembly(typeof(BoardRenderer))!;
            foreach (var resource in BoardGraphicsData.Pieces[style])
            {
                Stream stream = assembly.GetManifestResourceStream(resource.Value.Resource)!;
                var img = SKBitmap.Decode(stream);

                Assert.IsNotNull(img);
                Assert.IsTrue(img.Width > 2);
                Assert.IsTrue(img.Height > 2);
            }
        }

        [DataRow(BoardStyle.PixelChess)]
        [DataRow(BoardStyle.JPCB)]
        [DataTestMethod]
        public void CanRenderBoard(BoardStyle style)
        {
            var renderer = new BoardRenderer(style);
            using (var bmp = renderer.Render(new Board()))
            {
                Assert.IsNotNull(bmp);

                SKData data = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);
                using (var stream = File.OpenWrite($"/tmp/CanRenderBoard-{style}.png"))
                    data.SaveTo(stream);
            }
        }

        [DataRow(BoardStyle.PixelChess)]
        [DataRow(BoardStyle.JPCB)]
        [DataTestMethod]
        public void CanRenderCheck(BoardStyle style)
        {
            var board = Board.FromFEN(SampleDataGenerator.FEN_FoolsMate);
            var renderer = new BoardRenderer(style);
            using (var bmp = renderer.Render(board))
            {
                Assert.IsNotNull(bmp);

                var backgroundData = BoardGraphicsData.Compositions[BoardStyle.PixelChess];

                SKData data = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);
                using (var stream = File.OpenWrite($"/tmp/CanRenderCheck-{style}.png"))
                    data.SaveTo(stream);
            }
        }
    }
}

