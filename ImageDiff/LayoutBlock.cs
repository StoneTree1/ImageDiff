using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace ImageDiff
{
    public class LayoutBlock
    {
        public Rect Bounds { get; set; }
        public string Text {  get; set; }
        public PolyBlockType BlockType { get; set; }

        public LayoutBlock FindClosestMatch(List<LayoutBlock> blocks)
        {
            if (blocks.Exists(x => x.Bounds.Equals( Bounds)))
            {
                return blocks.First(x=> x.Bounds.Equals( Bounds));
            }
            double currentScore = 0;
            var thisLoction = new Point(Bounds.X2, Bounds.Y2);
            var thisSize = Bounds.Width*Bounds.Height;
            var ClosestMatch = blocks[0];

            foreach (LayoutBlock block in blocks)
            {
                var xs = (block.Bounds.X1 - Bounds.X1)* (block.Bounds.X1 - Bounds.X1);
                var ys=(block.Bounds.Y1 -Bounds.Y1)* (block.Bounds.Y1 -Bounds.Y1);
                var dist = Math.Sqrt(xs + ys);
                var size =  block.Bounds.Width*block.Bounds.Height;

                var sizeDiff = Math.Abs( size - thisSize);
                double sizeCloseness = (double)sizeDiff / thisSize;
                int sizeScore = 100;
                if (sizeCloseness > 0.8)
                {
                    sizeScore = 10;
                }
                else if (sizeCloseness > 0.6)
                {
                    sizeScore = 20;
                }
                else if (sizeCloseness > 0.4)
                {
                    sizeScore = 30;
                }
                else if (sizeCloseness > 0.2)
                {
                    sizeScore = 50;
                }
                else if (sizeCloseness > 0.1)
                {
                    sizeScore = 70;
                }
                else if (sizeCloseness > 0.05)
                {
                    sizeScore = 90;
                }
                var score = (100 / dist) * sizeScore;
                if (score > currentScore)
                {
                    currentScore = score;
                    ClosestMatch = block;
                }
            }

            return ClosestMatch;
        }
    }
}
