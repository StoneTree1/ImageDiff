using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiff
{
    public class Pixel
    {
        public bool isBorderPixel;
        public Rgba32 pixel;
        public bool processed;
        public bool needsHighlight;
        public bool isDifferent;
        public int Row;
        public int Column;
        public bool IsMoved;
        public bool IsBackgroundPixel;
        public bool IsImagePixel;


        public Pixel(Rgba32 pixel, int row, int column, bool processed=false, bool needsHighlight=false)
        {
            Row = row;
            Column = column;
            this.pixel = pixel;
            this.processed = processed;
            this.needsHighlight = needsHighlight;
        }

        public bool IsMatch(Pixel pixel, int threshold = 10)
        {
            if(pixel == null) { return false; }
            var rDif = pixel.pixel.R - this.pixel.R;
            var gDif = pixel.pixel.G - this.pixel.G;
            var bDif = pixel.pixel.B - this.pixel.B;
            if (! (-threshold < rDif && rDif< threshold))
            {
                return false;
            }
            if (!(-threshold < gDif && gDif < threshold))
            {
                return false;
            }
            if (!(-threshold < bDif && bDif < threshold))
            {
                return false;
            }
            return true;// pixel.pixel.Rgb == this.pixel.Rgb;
        }
    }
}
