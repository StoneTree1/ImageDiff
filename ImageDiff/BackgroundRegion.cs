
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ImageDiff
{
    public class BackgroundRegion
    {
        public List<DiffPixel> region;
        public BackgroundRegion()
        { region = new List<DiffPixel>(); }

        public void Add(DiffPixel pixel) 
        { 
            region.Add(pixel); 
        }

        public void ScanFrom(DiffPixel diffPixel, DiffPixel[,] rawImage)
        {
            int width = rawImage.GetLength(1);
            int height = rawImage.GetLength(0);
            diffPixel.processed = true;
            Add(diffPixel);
            for (int i = 0 - 1; i < 2; i++)
            {
                if (i + diffPixel.Row > height) { break; }
                for (int j = - 1; j < 2; j++)
                {
                    if (j == 0 && i == 0) continue;
                    if(j + diffPixel.Column > width){ break; }
                    var matchWeight = 0.0;
                    var neighbour = rawImage[i+diffPixel.Row, j+diffPixel.Column];
                    if (neighbour.isBorderPixel && !neighbour.processed)
                    {
                        if (neighbour.IsMatch(diffPixel))
                        {
                            ScanFrom(neighbour, rawImage);
                        }
                    }
                }
            }            
        }

    }
}