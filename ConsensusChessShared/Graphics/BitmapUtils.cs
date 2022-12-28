using System;
using ConsensusChessShared.Content;
using SkiaSharp;
using System.Reflection;

namespace ConsensusChessShared.Graphics
{
	public class BitmapUtils
	{
        public static SKBitmap GetBlank(int width, int height)
        {
            var imageInfo = new SKImageInfo(width, height);
            var bmp = new SKBitmap(imageInfo);
            var canvas = new SKCanvas(bmp);
            canvas.Clear(SKColors.Transparent);
            canvas.Save();
            return bmp;
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

