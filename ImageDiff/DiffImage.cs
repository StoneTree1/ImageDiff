using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Tesseract;
using System.Runtime.CompilerServices;

namespace ImageDiff
{
    public class DiffImage
    {
        public List<LayoutBlock> AreasOfInterest;
        public byte[,] RawImageGreyScale;
        public DiffPixel[,] RawImage;
        private readonly CompareSettings Settings;

        public DiffImage(CompareSettings settings, string imagePath)
        {
            Settings = settings;
            LoadFromFile(imagePath);
            AreasOfInterest=new List<LayoutBlock>();
        }

        public DiffImage(CompareSettings settings, string imagePath, TesseractEngine engine)
        {
            Settings = settings;
            AreasOfInterest = new List<LayoutBlock>();
            LoadFromFile(imagePath, engine);
        }
        public DiffImage(CompareSettings settings)
        {
            AreasOfInterest = new List<LayoutBlock>();
            Settings = settings;
        }

        public void LoadFromFile(string fileName) {
            Bitmap image = (Bitmap)Bitmap.FromFile(fileName);
            RawImage = ProcessPixels(image);
            PreProcessImage();
        }
        public void LoadFromFile(string fileName, TesseractEngine engine)
        {
            Bitmap image = (Bitmap)Bitmap.FromFile(fileName);
            RawImage = ProcessPixels(image);
            //PreProcessImage();
            DetectAreasOfInterestUsingTesseract(engine, fileName);
            PreProcessImageUsingAreasOfInterest();
        }

        private DiffPixel[,] ProcessPixels(Bitmap image)
        {
            BitmapData imageBytes = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);

            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;
            int stride = imageBytes.Stride;

            byte[] buffer = new byte[image.Height * stride];

            Marshal.Copy(imageBytes.Scan0, buffer, 0, buffer.Length);
            var pixels = new DiffPixel[image.Height, image.Width];
            RawImageGreyScale = new byte[image.Height, image.Width];
            for (int i = 0; i < image.Height; i++)
            {
                for (int widthIndex = 0; widthIndex < image.Width; widthIndex++)
                {
                    int rowOffset = i * stride;
                    var byteIndex = (bytesPerPixel * widthIndex) + rowOffset;
                    var pixelColor = Colour.FromArgb(buffer[byteIndex + 2], buffer[byteIndex + 1], buffer[byteIndex]);
                    RawImageGreyScale[i, widthIndex] = pixelColor.AsGrey();
                    pixels[i, widthIndex] = new DiffPixel(pixelColor, i, widthIndex);
                }
            }
            return pixels;
        }

        public void PreProcessImageUsingAreasOfInterest()
        {
            //Can be more specific with the areas later figure out if any should be ignored
            var maxWidth = RawImage.GetLength(1);
            var maxHeight = RawImage.GetLength(0);
            var imageAreas = AreasOfInterest.Where(x=>x.BlockType == PolyBlockType.FlowingImage || x.BlockType == PolyBlockType.HeadingImage || x.BlockType == PolyBlockType.PullOutImage).ToList();
            var otherAreas = AreasOfInterest.Where(x => x.BlockType != PolyBlockType.FlowingImage).ToList();

            //Set all to background
            for (int width = 0; width < maxWidth; width++)
            {
                for (int height = 0; height < maxHeight; height++)
                {
                    RawImage[height, width].IsBackgroundPixel = true;
                }
            }

            //any picked up as layouts can be updated to not background
            foreach(var imageLayout in imageAreas)
            {
                SetLayoutDefaultPixels(imageLayout);
            }
            foreach (var layout in otherAreas)
            {
                SetLayoutDefaultPixels(layout);
            }
        }
        private void SetLayoutDefaultPixels(LayoutBlock layout)
        {
            for (int width = layout.Bounds.X1; width < layout.Bounds.Width+ layout.Bounds.X1; width++)
            {
                for (int height = layout.Bounds.Y1; height< layout.Bounds.Height+ layout.Bounds.Y1; height++)
                {
                    if(layout.BlockType == PolyBlockType.FlowingImage
                        || layout.BlockType == PolyBlockType.HeadingImage
                        || layout.BlockType == PolyBlockType.PullOutImage)
                    {
                        RawImage[height, width].IsImagePixel = true;
                    }//else??
                    else
                    {
                        RawImage[height, width].IsImagePixel = false;
                    }
                    RawImage[height, width].IsBackgroundPixel = false;
                }
            }
        }

            // todo: Merge this into process pixels??!
        /// <summary>
        /// Scan whole image and try determine if pixel is background/text/image
        /// </summary>
        /// <param name="newImage"></param>
            public void PreProcessImage()
        {
            var width = RawImage.GetLength(1);
            var height = RawImage.GetLength(0);
            for (int maxWidth = 0; maxWidth < width; maxWidth += Settings.BlockWidth)
            {
                for (int maxHeight = 0; maxHeight < height; maxHeight += Settings.BlockHeight)
                {
                    Dictionary<string, int> pixelBlock = new Dictionary<string, int>();
                    for (int i = 0; i < Settings.BlockHeight; i++)
                    {
                        for (int j = 0; j < Settings.BlockWidth; j++)
                        {
                            if (maxWidth + j >= width || maxHeight + i >= height)
                            {
                                continue;
                            }
                            var pixel = RawImage[maxHeight + i, maxWidth + j];
                            var key = $"{pixel.Colour.R}_{pixel.Colour.G}_{pixel.Colour.B}";
                            if (pixelBlock.ContainsKey(key))
                            {
                                pixelBlock[key]++;
                            }
                            else
                            {
                                pixelBlock.Add(key, 1);
                            }
                        }
                    }
                    bool blockHasBackgroundColor = false;
                    Colour backgroundColor = Colour.White();
                    var threshold = Settings.BlockWidth * Settings.BlockHeight * 0.45;
                    if (pixelBlock.Any(x => x.Value >= threshold))
                    {
                        var key = pixelBlock.First(x => x.Value >= threshold);
                        var vals = key.Key.Split("_");
                        blockHasBackgroundColor = true;
                        backgroundColor = Colour.FromArgb(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));
                    }

                    for (int i = 0; i < Settings.BlockHeight; i++)
                    {
                        for (int j = 0; j < Settings.BlockWidth; j++)
                        {
                            if (maxWidth + j >= width || maxHeight + i >= height)
                            {
                                continue;
                            }
                            if (blockHasBackgroundColor || pixelBlock.Keys.Count == 1)
                            {
                                if (RawImage[maxHeight + i, maxWidth + j].IsMatch(backgroundColor))
                                {
                                    RawImage[maxHeight + i, maxWidth + j].IsBackgroundPixel = true;
                                }
                            }
                            else if (pixelBlock.Keys.Count == 1)
                            {
                                RawImage[maxHeight + i, maxWidth + j].IsBackgroundPixel = true;
                            }
                            else
                            {
                                //if no background pixels is likely an image pixel
                                var variancePercentage = (double)pixelBlock.Keys.Count / (Settings.BlockHeight * Settings.BlockWidth);
                                if (variancePercentage > Settings.ImageDetectionThreshold)
                                {
                                    RawImage[maxHeight + i, maxWidth + j].IsImagePixel = true;
                                }
                                else
                                {
                                    if (maxHeight > 100)
                                    {
                                        var breakp = "";
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public Bitmap FastGetImageFromDiffPixals()
        {
            int width = RawImage.GetLength(1);
            int height = RawImage.GetLength(0);
            //rebuild image for testing
            Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            DirectBitmap output = new DirectBitmap(width, height);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (RawImage[i, j].needsHighlight && RawImage[i, j].IsMoved)
                    {
                        var col = Color.FromArgb(255, 0, RawImage[i, j].Colour.AsGrey(), 0);
                        output.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].IsMoved)
                    {
                        var col = Color.FromArgb(255, 0, RawImage[i, j].Colour.AsGrey(), 0);
                        output.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].needsHighlight)
                    {
                        var col = Color.FromArgb(255, RawImage[i, j].Colour.AsGrey(), 0, 0);
                        output.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].IsImagePixel)
                    {
                        //yellw shift?
                        var col = Color.FromArgb(255, RawImage[i, j].Colour.AsGrey(), RawImage[i, j].Colour.AsGrey(), 0);
                        output.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].IsBackgroundPixel)
                    {
                        output.SetPixel(j, i, RawImage[i, j].Colour.ToColor());
                        //saveImage.SetPixel(j, i, Color.Black);
                    }
                    else if (RawImage[i, j].processed)
                    {
                        // saveImage.SetPixel(j, i, Color.Black);
                    }
                    else
                    {
                        var col = Color.FromArgb(255, 0, 0, RawImage[i, j].Colour.AsGrey());
                        output.SetPixel(j, i, col);
                    }
                    //output.SetPixel(j, i, Color.FromArgb(128, RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j]));
                }
            }
            return output.Bitmap;

           
        }

        public Bitmap GetImageFromDiffPixals()
        {
            int width = RawImage.GetLength(1);
            int height = RawImage.GetLength(0);
            //rebuild image for testing
            Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (RawImage[i, j].needsHighlight && RawImage[i, j].IsMoved)
                    {
                        var col = Color.FromArgb(255, 0, RawImage[i, j].Colour.AsGrey(), 0);
                        saveImage.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].IsMoved)
                    {
                        var col = Color.FromArgb(255, 0, RawImage[i, j].Colour.AsGrey(), 0);
                        saveImage.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].needsHighlight)
                    {
                        var col = Color.FromArgb(255, RawImage[i, j].Colour.AsGrey(), 0, 0);
                        saveImage.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].IsImagePixel)
                    {
                        //yellw shift?
                        var col = Color.FromArgb(255, RawImage[i, j].Colour.AsGrey(), RawImage[i, j].Colour.AsGrey(), 0);
                        saveImage.SetPixel(j, i, col);
                    }
                    else if (RawImage[i, j].IsBackgroundPixel)
                    {
                        saveImage.SetPixel(j, i, RawImage[i, j].Colour.ToColor());
                        //saveImage.SetPixel(j, i, Color.Black);
                    }
                    else if (RawImage[i, j].processed)
                    {
                       // saveImage.SetPixel(j, i, Color.Black);
                    }
                    else
                    {
                        var col = Color.FromArgb(255, 0, 0, RawImage[i, j].Colour.AsGrey());
                        saveImage.SetPixel(j, i, col);
                    }
                }
            }
            return saveImage;
        }

        public Bitmap GetGreyScale(bool setTransperancy)
        {
            if (setTransperancy)
            {
                int width = RawImage.GetLength(1);
                int height = RawImage.GetLength(0);
                //rebuild image for testing
                Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
                DirectBitmap output = new DirectBitmap(width, height);
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        output.SetPixel(j, i, Color.FromArgb(128, RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j]));
                    }
                }
                return output.Bitmap;

            }
            else
            {
                int width = RawImage.GetLength(1);
                int height = RawImage.GetLength(0);
                //rebuild image for testing
                Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                DirectBitmap output = new DirectBitmap(width, height);
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        output.SetPixel(j, i, Color.FromArgb(RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j]));
                    }
                }
                return output.Bitmap;
            }
        }

        public void DetectAreasOfInterestUsingTesseract(TesseractEngine engine, string file)
        {
            using (var img = Pix.LoadFromFile(file))//.LoadFromFile(testImage)
            {
                using (var page = engine.Process(img))
                {
                    using (var iter = page.GetIterator())
                    {
                        iter.Begin();
                        do
                        {
                            var bounds = new Tesseract.Rect();
                            iter.TryGetBoundingBox(PageIteratorLevel.Block, out bounds);
                            var text = iter.GetText(PageIteratorLevel.Block);
                            AreasOfInterest.Add(new LayoutBlock() { Bounds = bounds, Text = text, BlockType = iter.BlockType });

                            //File.AppendAllText($"C:\\tmp\\{fileName.Replace(".png", ".txt").Replace(".jpeg", ".txt").Replace(".webp", ".txt")}", $"\n{text}");
                        } while (iter.Next(PageIteratorLevel.Block));
                    }
                }
            }
        }
        public void DetectAreasOfInterestUsingTesseract(TesseractEngine engine, byte[] imageBytes)
        {
            using (var img = Pix.LoadFromMemory(imageBytes))//.LoadFromFile(testImage)
            {
                using (var page = engine.Process(img))
                {
                    using (var iter = page.GetIterator())
                    {
                        iter.Begin();
                        do
                        {
                            var bounds = new Tesseract.Rect();
                            iter.TryGetBoundingBox(PageIteratorLevel.Block, out bounds);
                            var text = iter.GetText(PageIteratorLevel.Block);
                            AreasOfInterest.Add(new LayoutBlock() { Bounds = bounds, Text = text, BlockType = iter.BlockType });

                            //File.AppendAllText($"C:\\tmp\\{fileName.Replace(".png", ".txt").Replace(".jpeg", ".txt").Replace(".webp", ".txt")}", $"\n{text}");
                        } while (iter.Next(PageIteratorLevel.Block));
                    }
                }
            }
        }

        public Bitmap CompareTo(DiffImage baselineImage)
        {
            var offsets = new List<Point>();
            foreach (var layout in AreasOfInterest)
            {
                var otherLayout = layout.FindClosestMatch(baselineImage.AreasOfInterest);
                offsets.Add(new Point(otherLayout.Bounds.X1 - layout.Bounds.X1, otherLayout.Bounds.Y1 - layout.Bounds.Y1));
            }
            var query = offsets
           .GroupBy(p => p)
           .Select(group => new { Point = group.Key, Count = group.Count() })
           .OrderByDescending(x => x.Count);

            // Get the most common Point
            var mostCommonPoint = query.First().Point;

            foreach (var layout in AreasOfInterest)
            {
                var otherLayout = layout.FindClosestMatch(baselineImage.AreasOfInterest);

                var matched = CompareBounds(layout, otherLayout, baselineImage);
                //determine if want to compare from a different offset
                bool checkDifferentOffset = true;
                if (!matched && checkDifferentOffset)
                {
                    //+/-?
                    List<double> matchWeights = new List<double>();
                    int count = 1;
                    while (count < 15 && !matched)
                    {
                        for (int i = 0 - count; i < count + 1; i++)
                        {
                            for (int j = count - 1; j < count + 1; j++)
                            {
                                if (j == 0 && i == 0) continue;
                                var matchWeight = 0.0;
                                matched = CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(i, j), out matchWeight);

                                matchWeights.Add(matchWeight);
                                if (matched)
                                {
                                    break;
                                }
                            }
                            if (matched)
                            {
                                break;
                            }
                        }
                        count++;
                    }
                    var closestMatch = matchWeights.Max();
                    var s = "breeak";
                }
                if (!matched)
                {
                    //set                        
                    for (int i = 0; i < layout.Bounds.Width; i++)
                    {
                        for (int j = 0; j < layout.Bounds.Height; j++)
                        {
                            RawImage[j + layout.Bounds.Y1, i + layout.Bounds.X1].needsHighlight = true;
                        }
                    }
                }
                else
                {
                    bool xmatcxhes = layout.Bounds.X1 == otherLayout.Bounds.X1;
                    bool ymatcxhes = layout.Bounds.Y1 == otherLayout.Bounds.Y1;
                    var offset = new Point(layout.Bounds.X1 - otherLayout.Bounds.X1, layout.Bounds.Y1 - otherLayout.Bounds.Y1);

                    if (offset != Point.Empty)
                    {
                        for (int i = 0; i < layout.Bounds.Width; i++)
                        {
                            for (int j = 0; j < layout.Bounds.Height; j++)
                            {
                                RawImage[j + layout.Bounds.Y1, i + layout.Bounds.X1].IsMoved = true;
                            }
                        }
                    }
                    else
                    {
                        //matched at same coordinates
                    }
                }
            }
            return GetImageFromDiffPixals();
        }
        
        public bool CompareBoundsWithOffset(LayoutBlock thisLocation, LayoutBlock otherLocation, DiffImage otherImage, Point offest, out double matchWeight)
        {
            //var offset = new Point(thisLocation.Bounds.X1 - otherLocation.Bounds.X1, thisLocation.Bounds.Y1 - otherLocation.Bounds.Y1);

            int otherWidth = otherImage.RawImage.GetLength(1);
            int otherHeight = otherImage.RawImage.GetLength(0);
            //to scale or not to scale???
            //to retry at adjusted offsets?
            if (thisLocation.BlockType == PolyBlockType.FlowingImage || thisLocation.BlockType == PolyBlockType.HeadingImage || thisLocation.BlockType == PolyBlockType.PullOutImage)
            {
                //is an image can do a percentage match 
                int matchedCount = 0;
                double threshold = 0.75;
                int checkedCount = 0;
                var totalCount = thisLocation.Bounds.Width * thisLocation.Bounds.Height;
                for (int i = 0; i < thisLocation.Bounds.Width; i++)
                {
                    if (i+offest.X+ thisLocation.Bounds.X1 >= otherWidth) { break; }
                    if (i + offest.X + thisLocation.Bounds.X1 <0) { continue; }
                    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                    {
                        RawImage[j, i].processed = true;
                        var thisPixel = RawImage[j+ thisLocation.Bounds.Y1, i+ thisLocation.Bounds.X1];
                        if (j + thisLocation.Bounds.Y1 + offest.Y >= otherHeight) { break; }//break;??
                        if (j + thisLocation.Bounds.Y1 + offest.Y <0) { continue; }//break;??
                        checkedCount++;
                        var otherPixel = otherImage.RawImage[j+ otherLocation.Bounds.Y1 + offest.Y, i + otherLocation.Bounds .X1+ offest.X];
                        if (thisPixel.IsMatch(otherPixel))
                        {
                            matchedCount++;
                        }
                    }
                }
                matchWeight = (double)matchedCount / checkedCount;
                if (matchWeight>threshold)
                {
                    return true;
                }
                return false;
            }
            else
            {
                int matchedCount = 0;
                double threshold = 0.99;
                int checkedCount = 0;
                for (int i = 0; i < thisLocation.Bounds.Width; i++)
                {
                    if (i + offest.X+ thisLocation.Bounds.X1 >= otherWidth) { break; }
                    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                    {
                        if (RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1].IsBackgroundPixel) { continue; }
                        if (j + offest.Y+ thisLocation.Bounds.Y1 >= otherHeight) { break; }//break;??
                        checkedCount++;
                        var thisPixel = RawImage[j+ thisLocation.Bounds.Y1, i+ thisLocation.Bounds.X1];
                        var otherPixel = otherImage.RawImage[j+ otherLocation.Bounds.Y1 + offest.Y, i + otherLocation.Bounds.X1+ offest.X];
                        if (thisPixel.IsMatch(otherPixel))
                        {
                            matchedCount++;
                        }
                    }
                }

                matchWeight = (double)matchedCount / checkedCount;
                if (matchWeight >= threshold)
                {
                    return true;
                }
                return false;
            }
        }
        public bool CompareBounds(LayoutBlock thisLocation, LayoutBlock otherLocation, DiffImage otherImage)
        {
            //var offset = new Point(thisLocation.Bounds.X1 - otherLocation.Bounds.X1, thisLocation.Bounds.Y1 - otherLocation.Bounds.Y1);

            int otherWidth = otherImage.RawImage.GetLength(1);
            int otherHeight = otherImage.RawImage.GetLength(0);
            //to scale or not to scale???
            //to retry at adjusted offsets?
            if (thisLocation.BlockType == PolyBlockType.FlowingImage || thisLocation.BlockType == PolyBlockType.HeadingImage || thisLocation.BlockType == PolyBlockType.PullOutImage)
            {
                //is an image can do a percentage match 
                int matchedCount = 0;
                double threshold = 0.75;
                int checkedCount = 0;
                var totalCount = thisLocation.Bounds.Width * thisLocation.Bounds.Height;
                for (int i = 0; i < thisLocation.Bounds.Width; i++)
                {
                    if (i + thisLocation.Bounds.X1 >= otherWidth) { break; }
                    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                    {
                        RawImage[j, i].processed = true;
                        var thisPixel = RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1];
                        if (j + thisLocation.Bounds.Y1 >= otherHeight) { break; }//break;??
                        checkedCount++;
                        var otherPixel = otherImage.RawImage[j + otherLocation.Bounds.Y1, i + otherLocation.Bounds.X1];
                        if (thisPixel.IsMatch(otherPixel))
                        {
                            matchedCount++;
                        }
                    }
                }
                var matchWeight = (double)matchedCount / checkedCount;
                if (matchWeight > threshold)
                {
                    return true;
                }
                return false;
            }
            else
            {
                int matchedCount = 0;
                double threshold = 0.99;
                int checkedCount = 0;
                for (int i = 0; i < thisLocation.Bounds.Width; i++)
                {
                    if (i  + thisLocation.Bounds.X1 >= otherWidth) 
                    {
                        break; 
                    }
                    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                    {
                        if (RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1].IsBackgroundPixel) 
                        { 
                            continue; 
                        }
                        if (j +  thisLocation.Bounds.Y1 >= otherHeight) 
                        { 
                            break; 
                        }//break;??
                        checkedCount++;
                        var thisPixel = RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1];
                        var otherPixel = otherImage.RawImage[j + otherLocation.Bounds.Y1, i + otherLocation.Bounds.X1];
                        if (thisPixel.IsMatch(otherPixel))
                        {
                            matchedCount++;
                        }
                        else
                        {
                            var s = "";
                        }
                    }
                }

                if ((double)matchedCount / checkedCount >= threshold)
                {
                    return true;
                }
                return false;
            }
        }

        public Image GetSubtractionImage(Bitmap otherImage, Point offset)
        {
            int width = RawImage.GetLength(1);
            int height = RawImage.GetLength(0);
            //rebuild image for testing
            Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            DirectBitmap output = new DirectBitmap(width, height);
            for (int i = 0; i < height; i++)
            {
                if (i+offset.Y >= otherImage.Height || i+offset.Y<0) continue;
                for (int j = 0; j < width; j++)
                {
                    if (j+offset.X >= otherImage.Width|| j+offset.X<0) continue;
                    var col = otherImage.GetPixel(j+offset.X, i+offset.Y);
                    var r = Math.Abs(col.R - RawImage[i, j].Colour.R);
                    var g = Math.Abs(col.G - RawImage[i, j].Colour.G);
                    var b = Math.Abs(col.B - RawImage[i, j].Colour.B);
                    output.SetPixel(j, i, Color.FromArgb(255, r, g, b));
                }
            }
            return output.Bitmap;

        }
    }
}
