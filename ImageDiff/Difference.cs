using System;
using System.Drawing;

namespace ImageDiff
{
    public class Difference
    {
        public Rectangle Area { get; set; }
        public DifferenceType Type { get; set; }

        public enum DifferenceType
        {
            None = 0,
            Moved = 1,
            Changed = 2,
            ImageChanged = 3,
            BackgroundChanged = 4
        }
    }
}
