using System;
using SkiaSharp;

namespace ConsensusChessShared.Helpers
{
	public static class GraphicsExtensions
	{
        public static byte[] ToPngBytes(this SKBitmap bmp)
        {
            var i = SKImage.FromBitmap(bmp);
            var data = i.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }
    }
}

