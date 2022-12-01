using System;
using System.Drawing;
using System.Reflection;
using ConsensusChessShared.Content;
using ConsensusChessShared.DTO;
using SkiaSharp;
using static ConsensusChessShared.Content.BoardGraphicsData;
using static ConsensusChessShared.Content.BoardRenderer;

namespace ConsensusChessSharedTests
{
	[TestClass]
	public class BoardRendererTests
	{
        [TestMethod]
        public void ImagesAvailable()
        {

            Assembly assembly = Assembly.GetAssembly(typeof(BoardRenderer))!;
            foreach (var resource in BoardGraphicsData.Pieces[BoardStyle.PixelChess])
            {
                Stream stream = assembly.GetManifestResourceStream(resource.Value.Resource)!;
                var img = SKBitmap.Decode(stream);

                Assert.IsNotNull(img);
                Assert.AreEqual(16, img.Width);
                Assert.AreEqual(32, img.Height);
            }
        }

        [TestMethod]
        public void CanRenderBoard()
        {
            var renderer = new BoardRenderer(new Board());
            using (var bmp = renderer.RenderBoard(BoardStyle.PixelChess))
            {
                Assert.IsNotNull(bmp);

                var backgroundData = BoardGraphicsData.Compositions[BoardStyle.PixelChess];
                Assert.AreEqual(backgroundData.Width * backgroundData.ScaleX, bmp.Width);
                Assert.AreEqual(backgroundData.Height * backgroundData.ScaleY, bmp.Height);

                SKData data = SKImage.FromBitmap(bmp).Encode(SKEncodedImageFormat.Png, 100);
                using (var stream = File.OpenWrite("/tmp/image.png"))
                    data.SaveTo(stream);
            }
        }
    }
}

