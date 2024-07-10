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
            var thisSize = Bounds.Width*Bounds.Height;
            var ClosestMatch = blocks[0];

            foreach (LayoutBlock block in blocks)
            {
                var textScore = GetTextScore(block.Text);
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
                var score = ((100 / dist) * sizeScore)+textScore;
                if (score > currentScore)
                {
                    currentScore = score;
                    ClosestMatch = block;
                }
            }
            return ClosestMatch;
        }

        private int GetTextScore(string blockText)
        {
            int score = 1000;
            if(blockText == Text)
            {
                return 100000;
            }
            var thisWords = Text.Split(' ');
            var blockWords = blockText.Split(" ");

            var countDiff = Math.Abs(thisWords.Length - blockWords.Length);
            if (countDiff != 0)
            {
                double dd = (1 - ((double)countDiff / thisWords.Length));
                if (dd < 0.5)
                {
                    return 0;
                }
                score = (int)(score * dd);
            }
            double matches = 0;
            for(int i=0; i < thisWords.Length; i++)
            {
                int start = i - 3;
                int end = i + 3;
                if (start < 0) start = 0;
                if(end >=blockWords.Length) end = blockWords.Length-1;
                bool found = false;
                for(int j=start; j < end; j++)
                {
                    if (thisWords[i] == blockWords[j])
                    {
                        found = true; 
                        break;
                    }
                }
                if(found)  
                {
                    matches++;
                }
            }
            double matchPercenatge = matches/thisWords.Length;
            return (int)(score*matchPercenatge);
        }
    }
}
