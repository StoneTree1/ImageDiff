
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;

namespace ImageDiff
{
    public class BackgroundRegion
    {
        DiffPixel[,] rawImage;
        public List<Point> members;
        int maxWidth;
        int maxHeight;
        public BackgroundRegion(DiffPixel[,] rawImage)
        {
            this.rawImage = rawImage;
            members = new List<Point>();

            maxWidth = rawImage.GetLength(1);
            maxHeight = rawImage.GetLength(0);
        }

        //public void Add(DiffPixel pixel) 
        //{ 
        //    members.Add(new Point(pixel.Column, pixel.Row)); 
        //}

        public void ScanFrom(DiffPixel diffPixel, int x, int y)
        {
            if (diffPixel.processed)
            {
                //ooops?
                return;
            }
            diffPixel.processed = true;
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(x, y));
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                members.Add(current);
                var toCheck = GetNeighbours(current);
                foreach (var neighbour in toCheck)
                {
                    queue.Enqueue(neighbour);
                }
            }
        }

        public List<Point> GetNeighbours(Point location)
        {
            List<Point> neighbours = new List<Point>();
            for (int i = 0 - 1; i < 2; i++)
            {
                if (i + location.Y >= maxHeight) { break; }
                for (int j = -1; j < 2; j++)
                {
                    if (j == 0 && i == 0) continue;
                    if (j + location.X >= maxWidth) { break; }

                    if (i + location.Y < 0 || j + location.X < 0) { continue; }
                    var neighbour = rawImage[i + location.Y, j + location.X];
                    var current = rawImage[location.Y, location.X];
                    if (neighbour.IsBackgroundPixel && !neighbour.processed)
                    {
                        if (neighbour.IsMatch(current))
                        {
                            rawImage[i + location.Y, j + location.X].processed = true;
                            var nextLocation = new Point(j + location.X, i + location.Y);
                            neighbours.Add(nextLocation);
                        }
                        else
                        {
                            var notmatched = "";
                        }
                    }
                    else
                    {
                        var s = "";
                    }
                }
            }
            return neighbours;
        }

        public void ScanFromOld(DiffPixel diffPixel, int x, int y)
        {
            if (diffPixel.processed)
            {
                //ooops?
                return;
            }
            diffPixel.processed = true;
            ScanFrom(new Point(x,y));
        }

        public void ScanFrom(Point location)
        {            
            List<Point> neighbours = new List<Point>();
            for (int i = 0 - 1; i < 2; i++)
            {
                if (i + location.Y >= maxHeight) { break; }
                for (int j = - 1; j < 2; j++)
                {
                    if (j == 0 && i == 0) continue;
                    if(j + location.X >= maxWidth){ break; }
                    
                    if(i + location.Y <0 || j + location.X < 0) { continue; }
                    var neighbour = rawImage[i + location.Y, j + location.X];
                    var current = rawImage[location.Y, location.X];
                    if (neighbour.IsBackgroundPixel && !neighbour.processed)
                    {
                        if (neighbour.IsMatch(current))
                        {
                            
                            rawImage[i + location.Y, j + location.X].processed = true;
                            var nextLocation = new Point(j + location.X, i + location.Y);
                            members.Add(nextLocation);
                            ScanFrom(nextLocation);
                        }
                    }
                    else
                    {
                        var s = "";
                    }
                }
            }            
        }

    }
}