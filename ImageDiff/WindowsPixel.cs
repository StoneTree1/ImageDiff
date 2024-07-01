using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiff
{
    public class Colour
    {
        public byte R { get; set; }
        public byte G {  get; set; }
        public byte B {  get; set; }

        public void SetWhite()
        {
            R = 255;
            G = 255;
            B = 255;
        }
        public void SetRed()
        {
            R = 255;
            G = 0;
            B = 0;
        }

        public static Colour FromArgb(byte v1, byte v2, byte v3)
        {
            return new Colour() { R= v1, G = v2, B= v3 };  
        }
        public static Colour White()
        {
            return new Colour() { R = 255, G = 255, B = 255 };
        }
        public static Colour Red()
        {
            return new Colour() { R = 255, G = 0, B = 0 };
        }

        public Color ToColor()
        {
            return Color.FromArgb(R, G, B);
        }

        internal byte AsGrey()
        {
            return Convert.ToByte( (R + G + B) / 3);
        }
    }
    public class WindowsPixel
    {
        public bool isBorderPixel;
        public Colour Colour;
        public bool processed;
        public bool needsHighlight;
        public bool isDifferent;
        public int Row;
        public int Column;
        public bool IsMoved;
        public bool IsBackgroundPixel;
        public bool IsImagePixel;

        public WindowsPixel(Colour col, int row, int column, bool processed = false, bool needsHighlight = false)
        {
            Row = row;
            Column = column;
            //this.pixel = pixel;
            Colour = col;
            this.processed = processed;
            this.needsHighlight = needsHighlight;
        }
        public WindowsPixel(byte r, byte g, byte b, int row, int column, bool processed=false, bool needsHighlight=false)
        {
            Row = row;
            Column = column;
            //this.pixel = pixel;
            Colour= new Colour() { R = r, G = g, B = b };
            this.processed = processed;
            this.needsHighlight = needsHighlight;
        }

        public bool IsMatch(WindowsPixel pixel, int threshold = 10)
        {
            if(pixel == null) { return false; }
            var rDif = Math.Abs( pixel.Colour.R - this.Colour.R);
            var gDif = Math.Abs(pixel.Colour.G - this.Colour.G);
            var bDif = Math.Abs(pixel.Colour.B - this.Colour.B);
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

        public bool IsMatch(Colour col, int threshold = 10)
        {
            //if (pixel == null) { return false; }
            var rDif = col.R- this.Colour.R;
            var gDif = col.G - this.Colour.G;
            var bDif = col.B - this.Colour.B;
            if (!(-threshold < rDif && rDif < threshold))
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
