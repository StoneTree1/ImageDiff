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
        public byte G { get; set; }
        public byte B { get; set; }

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
            return new Colour() { R = v1, G = v2, B = v3 };
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
            return Convert.ToByte((R + G + B) / 3);
        }
    }
}
