using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiff
{
    internal class ImageUtils
    {
        public string SaveImageAsPng(ImagePixels image, string name)
        {
            var img = GetImage(image);
            var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Images\\{name}.png";
            img.SaveAsPng(path);
            return path;
        }

        public string SaveImageAsBitmap(ImagePixels image, string name)
        {
            try
            {
                var img = GetImage(image);
                var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Images\\{name}.bmp";
                img.SaveAsBmp(path);
                return path;
            }
            catch
            {
                //Thread.Sleep(200);
                //var img = GetImage(image);
                //var path = $"{Application.StartupPath}\\Images\\{name}.bmp";
                //img.SaveAsBmp(path);
                return "";
            }
        }

        private Image<SixLabors.ImageSharp.PixelFormats.Rgba32> GetImage(ImagePixels image)
        {
            var img = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(image.Rows[0].Count, image.Rows.Count, new Rgba32(255, 255, 255));
            //img.Mutate(x => x.BackgroundColor(SixLabors.ImageSharp.Color.White));

            //img.Mutate(x => x.DrawText(message, family.CreateFont(20, SixLabors.Fonts.FontStyle.Regular), SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(10, 10)));
            Image<Rgba32> targetImage = new Image<Rgba32>(img.Size().Width, img.Size().Height);
            List<byte> imageBytes = new List<byte>();
            img.ProcessPixelRows(comparisonAccessor =>
            {
                for (int rowIndex = 0; rowIndex < image.Rows.Count; rowIndex++)
                {
                    Span<Rgba32> pixelRow = comparisonAccessor.GetRowSpan(rowIndex);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int column = 0; column < image.Rows[0].Count; column++)
                    {
                        ref Rgba32 pixel = ref pixelRow[column];
                        if (image.Rows[rowIndex].Pixels.Count > column)
                        {
                            pixel = image.Rows[rowIndex].Pixels[column].pixel;
                        }
                        else
                        {
                            pixel = new Rgba32(0, 0, 0);
                        }
                    }
                }
            });
            return img;
        }
    }
}
