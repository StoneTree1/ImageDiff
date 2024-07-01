using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiff
{
    using System;
    using System.Drawing;

    public class EdgeDetection
    {
        public static CropRect DetectEdges(Bitmap image, float threshold)
        {
            CropRect cropRectangle = new CropRect();
            int lowestX = image.Width;
            int lowestY = image.Height;
            int largestX = 0;
            int largestY = 0;

            for (int y = 0; y < image.Height - 1; ++y)
            {
                for (int x = 0; x < image.Width - 1; ++x)
                {
                    Color currentColor = image.GetPixel(x, y);
                    Color tempXcolor = image.GetPixel(x + 1, y);
                    Color tempYColor = image.GetPixel(x, y + 1);

                    if (CalculateColorDifference(currentColor, tempXcolor) > threshold)
                    {
                        if (lowestX > x) lowestX = x;
                        if (largestX < x) largestX = x;
                    }

                    if (CalculateColorDifference(currentColor, tempYColor) > threshold)
                    {
                        if (lowestY > y) lowestY = y;
                        if (largestY < y) largestY = y;
                    }
                }
            }

            cropRectangle.X = lowestX;
            cropRectangle.Y = lowestY;
            cropRectangle.Width = largestX - lowestX;
            cropRectangle.Height = largestY - lowestY;

            return cropRectangle;
        }

        private static double CalculateColorDifference(Color color1, Color color2)
        {
            return Math.Sqrt(
                Math.Pow(color1.R - color2.R, 2) +
                Math.Pow(color1.G - color2.G, 2) +
                Math.Pow(color1.B - color2.B, 2)
            );
        }
    }

    public struct CropRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

}
