
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Tesseract;

namespace ImageDiff
{
    public class ImageDiffWindows
    {
        //const int BlockSize = 10;  // Size of the blocks

        int MovedSectionCount = 0;
        public bool displayingBaseline;
        public WindowsPixel[,] comparisonPixels;
        public WindowsPixel[,] baselinePixels;
        public WindowsPixel[,] outputImage;
        //public int searchHeight = 240;
        //public int searchWidth = 20;
        //public int rowBuffer = 10;
        //public int columnBuffer = 10;
        //public int traversalBorder = 2;
        //public Logger logger;
        //bool LoggingOn = false;
        //string currentScannerImage = "";
        ComparisonLevel ComparisonLevel;
        //Bitmap diffImage;
        //Bitmap newImage;
        //Bitmap baselienImage;
        private readonly CompareSettings Settings;

        public ImageDiffWindows(CompareSettings settings)
        {
            Settings = settings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newImage"></param>
        /// <param name="baselineImage"></param>
        /// <param name="isDifferent"></param>
        /// <returns></returns>
        public Bitmap DoCompare(byte[] newImageRawBytes, byte[] baselineImageBytes, out bool isDifferent)
        {
            try
            {
                bool LoggingOn = false;
                //currentScannerImage = "";
                //preprocess images
                var newImage = new Bitmap(new MemoryStream(newImageRawBytes));
                var baselienImage = new Bitmap(new MemoryStream(baselineImageBytes));

                //scale baseline to same width as screenshot must have been taken with different sized browser
                if(newImage.Width != baselienImage.Width)
                {
                    baselienImage = ScaleImageToWidth(baselienImage, newImage.Width);
                }

                int width = Math.Min(newImage.Width, baselienImage.Width);
                int height = Math.Min(newImage.Height, baselienImage.Height);
                int maxWidth = Math.Max(newImage.Width, baselienImage.Width);
                int maxHeight = Math.Max(newImage.Height, baselienImage.Height);

                //diffImage = new Bitmap(maxWidth, maxHeight, PixelFormat.Format24bppRgb);
                BitmapData newImageBytes = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height), ImageLockMode.ReadOnly, newImage.PixelFormat);
                BitmapData baselineBytes = baselienImage.LockBits(new Rectangle(0, 0, baselienImage.Width, baselienImage.Height), ImageLockMode.ReadOnly, baselienImage.PixelFormat);

                if (Debugger.IsAttached)
                {
                    newImage.Save("C:\\tmp\\NewImageForCompare.jpg");
                    baselienImage.Save("C:\\tmp\\BaselineImageForCompare.jpg");
                }
                //BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, diffImage.Width, diffImage.Height), ImageLockMode.WriteOnly, diffImage.PixelFormat);
                //int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(newImage.PixelFormat) / 8;
                //int stride1 = newImageBytes.Stride;
                //int stride2 = baselineBytes.Stride;
                //int diffStride = diffData.Stride;

                //byte[] buffer1 = new byte[newImageBytes.Height * stride1];
                //byte[] buffer2 = new byte[baselineBytes.Height * stride2];
                //byte[] bufferDiff = new byte[diffData.Height * diffStride];

                //Marshal.Copy(newImageBytes.Scan0, buffer1, 0, buffer1.Length);
                //Marshal.Copy(baselineBytes.Scan0, buffer2, 0, buffer2.Length);
                //Marshal.Copy(diffData.Scan0, bufferDiff, 0, bufferDiff.Length);
                isDifferent = false;

                //Get image pixels as array
                var baselinePixels = ProcessPixels(baselineBytes);
                var comparisonPixels = ProcessPixels(newImageBytes);
                newImage.UnlockBits(newImageBytes);
                baselienImage.UnlockBits(baselineBytes);
                //diffImage.UnlockBits(diffData);

                outputImage = (WindowsPixel[,])comparisonPixels.Clone();
                Bitmap diffImage = null;
                if (Settings.CompareType == CompareType.Fast)
                {
                    isDifferent = PerformFastComparison(comparisonPixels, baselinePixels);
                    diffImage = GetImageFromPixals(comparisonPixels);
                }
                else if (Settings.CompareType == CompareType.FastWithBackgroundDetection)
                {
                    isDifferent = PerformFastComparisonWithBackgroundDetection(comparisonPixels, baselinePixels);

                    diffImage = GetImageFromPixals(comparisonPixels);
                }
                else if (Settings.CompareType == CompareType.DetectMovement)
                {

                    //if (newImageBytes.Height > baselienImage.Height)
                    //{
                    //    outputImage = (WindowsPixel[,])comparisonPixels.Clone();
                    //    isDifferent = PerformComparison(comparisonPixels, baselinePixels);
                    //    diffImage = GetImageFromPixals(comparisonPixels);
                    //}
                    //else
                    //{
                        outputImage = (WindowsPixel[,])comparisonPixels.Clone();
                        isDifferent = PerformComparison(comparisonPixels, baselinePixels);
                        diffImage = GetImageFromPixals(baselinePixels);
                    //}                
                }

                return diffImage;

            }
            catch (Exception ex)
            {
                var dd = "";
            }
            isDifferent=false;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="largerImage"></param>
        /// <param name="SmallerImage"></param>
        /// <exception cref="NotImplementedException"></exception>
        private bool PerformFastComparison(WindowsPixel[,] comparison, WindowsPixel[,] baseline)
        {
            bool extrtaAtTop = false;
            var width = comparison.GetLength(1);
            var height = comparison.GetLength(0);
            var baselineWidth = baseline.GetLength(1);
            var baselineHeight = baseline.GetLength(0);
            int newTopRowsCount = 0;
            int newBottomRowsCount = 0;
            bool matches = true;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (j >= baselineWidth || i >= baselineHeight)
                    {
                        matches = false;
                        outputImage[i, j].processed = true;
                        outputImage[i, j].IsMoved = false;
                        outputImage[i, j].needsHighlight = true;
                    }
                    else if (!comparison[i, j].IsMatch(baseline[i, j]))
                    {
                        matches = false;
                        outputImage[i, j].processed = true;
                        outputImage[i, j].IsMoved = false;
                        outputImage[i, j].needsHighlight = true;
                    }
                    else
                    {
                        outputImage[i, j].processed = true;
                        outputImage[i, j].IsMoved = false;
                        outputImage[i, j].needsHighlight = false;
                    }
                }
            }
            return matches;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="largerImage"></param>
        /// <param name="SmallerImage"></param>
        /// <exception cref="NotImplementedException"></exception>
        private bool PerformFastComparisonWithBackgroundDetection(WindowsPixel[,] comparison, WindowsPixel[,] baseline)
        {
            var width = comparison.GetLength(1);
            var height = comparison.GetLength(0);
            var baselineWidth = baseline.GetLength(1);
            var baselineHeight = baseline.GetLength(0);
            //int blockHeight = 10;
            //int blockWidth = 10;
            bool isDifferent = false;

            try
            {
                for (int maxWidth = 0; maxWidth < width; maxWidth += 10)
                {
                    for (int maxHeight = 0; maxHeight < height; maxHeight += 10)
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
                                var pixel = comparison[maxHeight + i, maxWidth + j];
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
                        Colour backgroundColor = new Colour();
                            backgroundColor.SetWhite();
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

                                if (maxWidth + j >= baselineWidth || maxHeight + i >= baselineHeight || !comparison[maxHeight + i, maxWidth + j].IsMatch(baseline[maxHeight+i, maxWidth+ j],10))
                                {
                                    isDifferent = true;
                                    outputImage[maxHeight + i, maxWidth + j].processed = true;
                                    outputImage[maxHeight + i, maxWidth + j].IsMoved = false;
                                    if (blockHasBackgroundColor && outputImage[maxHeight + i, maxWidth + j].IsMatch( backgroundColor, 10))
                                    {
                                        outputImage[maxHeight + i, maxWidth + j].needsHighlight = false;
                                    }
                                    else
                                    {
                                        outputImage[maxHeight + i, maxWidth + j].needsHighlight = true;
                                    }
                                }
                                else
                                {
                                    outputImage[maxHeight + i, maxWidth + j].processed = true;
                                    outputImage[maxHeight + i, maxWidth + j].IsMoved = false;
                                    outputImage[maxHeight + i, maxWidth + j].needsHighlight = false;
                                }
                            }

                        }
                    }
                }
            }
            catch(Exception ex)
            {
                var es = "";
            }
            return isDifferent;
        }

          /// <summary>
        /// 
        /// </summary>
        /// <param name="largerImage"></param>
        /// <param name="SmallerImage"></param>
        /// <exception cref="NotImplementedException"></exception>
        private bool PerformComparison(WindowsPixel[,] newImage, WindowsPixel[,] baseline)
        {
            bool emitLogs = false;
            //comparisonPixels = newImage;
            baselinePixels = baseline;
            comparisonPixels = PreProcessImage(newImage, new Size(10,10));
            var ppi = GetImageFromPixals(outputImage);
            ppi.Save($"C:\\tmp\\PreProcessImage_{DateTime.Now.Ticks}.jpg");

            bool extrtaAtTop = false;
            var width = comparisonPixels.GetLength(1);
            var height = comparisonPixels.GetLength(0);
            var smallerWidth = baselinePixels.GetLength(1);
            var smallerHeight = baselinePixels.GetLength(0);
            int newTopRowsCount = 0;
            int newBottomRowsCount = 0;
            var lastMovedOffset = new Point();

            bool hasChanges = false;
            for (int i = 0; i < height; i++)
            {
                if (i >= smallerHeight)
                {
                    break;
                }

                for (int j = 0; j < width; j++)
                {
                    if (j >= smallerWidth)
                    {
                        break;
                    }
                    if (baselinePixels[i, j].processed)
                    {
                        continue;
                    }
                    if (comparisonPixels[i, j].processed)
                    {
                        continue;
                    }
                    if (!comparisonPixels[i, j].IsImagePixel && !comparisonPixels[i, j].IsBackgroundPixel && !comparisonPixels[i, j].IsMatch(baselinePixels[i, j]))
                    {
                        var pix = comparisonPixels[i, j];
                        Point locationInBaseline;
                        var startMoveScan = DateTime.Now;
                        bool hasMoved = FindInBaseline(new Point(j, i), new Point(j, i), out locationInBaseline, lastMovedOffset);
                        
                        var MovedScanDuration = DateTime.Now - startMoveScan;
                        LogEmiter.LogMessage($"MoveScan took {MovedScanDuration.TotalSeconds} seconds. Result: {hasMoved}");
                        if (hasMoved)
                        {
                            //offset comparisonPixels->baseline
                            lastMovedOffset = new Point(locationInBaseline.X - j, locationInBaseline.Y - i);
                            var backColor = Colour.White();
                            var hasBackgroundColor = GetBackgroundColor(comparisonPixels, new Point(j, i), out backColor);
                            var startMoveComparison = DateTime.Now;
                            var movedBounds = PerformMovedComparison(locationInBaseline, new Point(j, i), hasBackgroundColor, backColor);
                            var MovedComparisonDuration = DateTime.Now - startMoveComparison;
                            try
                            {
                                var testPixel = baseline[i + 2, j + 2];
                                var testPixel2 = comparisonPixels[locationInBaseline.Y + 2, locationInBaseline.X + 2];
                                if (!testPixel.processed)
                                {
                                    var s = "";
                                }
                                if (!testPixel2.processed)
                                {
                                    var s = "";
                                }
                            }
                            catch (Exception e) { }
                            LogEmiter.LogMessage($"PerformMovedComparison took {MovedComparisonDuration.TotalSeconds} seconds. width: {movedBounds.Width}, height: {movedBounds.Height}");
                            if (i == 0 && movedBounds.Width > width * 0.8)
                            {
                                newTopRowsCount = movedBounds.Top;
                            }
                            if (movedBounds.Bottom + i >= smallerHeight && movedBounds.Width > width * 0.8)
                            {
                                newBottomRowsCount = height - (movedBounds.Bottom + i);
                            }
                        }
                        else
                        {
                            hasChanges = true;
                            outputImage[i, j].processed = true;
                            outputImage[i, j].IsMoved = false;
                            outputImage[i, j].needsHighlight = true;
                            comparisonPixels[i, j].processed = true;
                            comparisonPixels[i, j].IsMoved = false;
                            comparisonPixels[i, j].needsHighlight = true;
                            baselinePixels[i, j].processed = true;
                            baselinePixels[i, j].IsMoved = false;
                            baselinePixels[i, j].needsHighlight = true;
                        }
                        //Get a suitable(not too large or small) sized rectangle and check if its moved location
                        //if moved perform a a scan using offsets to determine the extent of the move
                        //Highlight moved portion as green and mark as scanned?
                    }
                }
            }

            //Mark extra rows top/bottom as different
            for (int i = 0; i < newTopRowsCount; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    outputImage[i, j].processed = true;
                    outputImage[i, j].needsHighlight = true;
                }
            }
            for (int i = height- newBottomRowsCount; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    outputImage[i, j].processed = true;
                    outputImage[i, j].needsHighlight = true;
                }
            }
            return hasChanges;
        }

        /// <summary>
        /// Scan whole image and try determine if pixel is background/text/image
        /// </summary>
        /// <param name="newImage"></param>
        public WindowsPixel[,] PreProcessImage(WindowsPixel[,] newImage, Size blockSize)
        {
            var width = newImage.GetLength(1);
            var height = newImage.GetLength(0);
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
                            var pixel = newImage[maxHeight + i, maxWidth + j];
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
                                if (newImage[maxHeight + i, maxWidth + j].IsMatch(backgroundColor))
                                {
                                    newImage[maxHeight + i, maxWidth + j].IsBackgroundPixel = true;
                                }
                            }
                            else if (pixelBlock.Keys.Count == 1)
                            {
                                newImage[maxHeight + i, maxWidth + j].IsBackgroundPixel = true;
                            }
                            else
                            {
                                //if no background pixels is likely an image pixel
                                var variancePercentage = (double)pixelBlock.Keys.Count / (Settings.BlockHeight * Settings.BlockWidth);
                                if (variancePercentage > Settings.ImageDetectionThreshold)
                                {
                                    newImage[maxHeight + i, maxWidth + j].IsImagePixel = true;
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
            return newImage;
                    //WindowsPixel[,] preProcessed = (WindowsPixel[,])newImage.Clone();
                    //Dictionary<string, int> counts = new Dictionary<string, int>();
                    //GetStartingBackgroundInfo(preProcessed, new Point(), out counts);
                    //var width = newImage.GetLength(1);
                    //var height = newImage.GetLength(0);
                    //Queue<Pixel> queue = new Queue<Pixel>();

            // for (int w = 0; w < width; w++)
            // {
            //    for (int h = 0; h < height; h += blockSize.Height)
            //    {
            //        if (h > blockSize.Width)
            //        {
            //            if (h > 0)
            //            {
            //                var oldPixel = preProcessed[h-1, w];
            //            }
            //        }

            //        //for height adjustment
            //        for (int i = 0; i < blockSize.Height; i++)
            //        {
            //            if (h + 1 < blockSize.Height)
            //            {
            //                //just add dont remove
            //            }
            //            var oldPixel = preProcessed[blockSize.Height, w];
            //            var key = $"{pixel.Colour.R}_{pixel.Colour.G}_{pixel.Colour.B}";
            //            counts[key]--;
            //            var newPixel = preProcessed[h + i, w];
            //            var key = $"{pixel.Colour.R}_{pixel.Colour.G}_{pixel.Colour.B}";
            //            counts[key]--;
            //        }
            //    }
            //}
                }

        public WindowsPixel[,] PreProcessImageForTextRecognition(WindowsPixel[,] newImage, Size size)
        {
            var width = newImage.GetLength(1);
            var height = newImage.GetLength(0);
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
                            var pixel = newImage[maxHeight + i, maxWidth + j];
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
                    var threshold = Settings.BlockWidth * Settings.BlockHeight * 0.3;
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
                                if (newImage[maxHeight + i, maxWidth + j].IsMatch(backgroundColor))
                                {
                                    newImage[maxHeight + i, maxWidth + j].IsBackgroundPixel = true;
                                }
                                else
                                {

                                }
                            }
                            else if (pixelBlock.Keys.Count == 1)
                            {
                                newImage[maxHeight + i, maxWidth + j].IsBackgroundPixel = true;
                            }
                            else
                            {
                                //if no background pixels is likely an image pixel
                                var variancePercentage = (double)pixelBlock.Keys.Count / (Settings.BlockHeight * Settings.BlockWidth);
                                if (variancePercentage > Settings.ImageDetectionThreshold)
                                {
                                    newImage[maxHeight + i, maxWidth + j].IsImagePixel = true;
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
            return newImage;
        }

        public List<Rect> ProcessForText(string filePath)
        {
            List<Rect> rects = new List<Rect>();
            using (var engine = new TesseractEngine("C:\\tmp\\tessdata", "eng", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(filePath))
                {
                    using (var page = engine.Process(img))
                    {
                        var text = page.GetText();
                        var layout = page.AnalyseLayout();

                        while (layout.Next(PageIteratorLevel.Block))
                        {
                            var bounds = new Tesseract.Rect();
                            layout.TryGetBoundingBox(PageIteratorLevel.Block, out bounds);
                            rects.Add(bounds);
                        }
                    }
                }
            }
            return rects;
        }

        private bool GetStartingBackgroundInfo(WindowsPixel[,] SmallerImage, Point start, out Dictionary<string, int> pixelCounts)
        {
            int compareHeight = Settings.BlockHeight;
            int compareWidth = Settings.BlockWidth;

            pixelCounts = new Dictionary<string, int>();
            for (int scanRow = 0; scanRow < compareHeight; scanRow++)
            {
                for (int scanCol = 0; scanCol < compareWidth; scanCol++)
                {
                    //record pixel counts for background pixel determination later                   
                    var pixel = SmallerImage[scanCol, scanRow];
                    var key = $"{pixel.Colour.R}_{pixel.Colour.G}_{pixel.Colour.B}";
                    if (pixelCounts.ContainsKey(key))
                    {
                        pixelCounts[key]++;
                    }
                    else
                    {
                        pixelCounts.Add(key, 1);
                    }
                    //break;
                }
            }
            var threshold = compareHeight * compareWidth * 0.8;
            if (pixelCounts.Any(x => x.Value >= threshold))
            {
                var key = pixelCounts.First(x => x.Value >= threshold);
                var vals = key.Key.Split("_");
                var backgroundColor = Color.FromArgb(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));
                return true;
            }
      
            return false;
        }


        private bool GetBackgroundColor(WindowsPixel[,] SmallerImage, Point baselineStart, out Colour backgroundColor)
        {
            int compareHeight = Settings.BlockHeight;
            int compareWidth = Settings.BlockWidth;
            if (compareHeight + baselineStart.X > SmallerImage.GetLength(0))
            {
                compareHeight = SmallerImage.GetLength(0) - baselineStart.X;
            }
            if (compareWidth + baselineStart.X > SmallerImage.GetLength(0))
            {
                compareWidth = SmallerImage.GetLength(1) - baselineStart.X;
            }
            Dictionary<string, int> counts = new Dictionary<string, int>();
            for (int scanRow = 0; scanRow < compareHeight; scanRow++)
            {
                for (int scanCol = 0; scanCol < compareWidth; scanCol++)
                {
                    //record pixel counts for background pixel determination later                   
                    var pixel = SmallerImage[scanCol, scanRow];
                    var key = $"{pixel.Colour.R}_{pixel.Colour.G}_{pixel.Colour.B}";
                    if (counts.ContainsKey(key))
                    {
                        counts[key]++;
                    }
                    else
                    {
                        counts.Add(key, 1);
                    }
                    //break;
                }
            }
            var threshold = compareHeight * compareWidth * 0.8;
            if (counts.Any(x => x.Value >= threshold))
            {
                var key = counts.First(x => x.Value >= threshold);
                var vals = key.Key.Split("_");
                backgroundColor = Colour.FromArgb(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));
                return true;
            }
            backgroundColor = Colour.White();
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newImageStart"></param>
        /// <param name="baselineStart"></param>
        /// <param name="movedTo"></param>
        /// <param name="lastOffset"></param>
        /// <returns></returns>
        private bool FindInBaseline(Point newImageStart, Point baselineStart, out Point movedTo, Point lastOffset)
        {
            // var baselineSearchArea = new Rectangle(
            int baselineStartX = baselineStart.X - (Settings.SearchWidth / 2);
            int baselineStartY = baselineStart.Y - (Settings.SearchWidth / 2);
            int baselineEndX = baselineStart.X + (Settings.SearchWidth / 2);
            int baselineEndY = baselineStart.Y + (Settings.SearchWidth / 2);
            //     );
            int compareHeight = Settings.BlockHeight;
            int compareWidth = Settings.BlockWidth;
            int baselineWidth = baselinePixels.GetLength(1);
            int baselineHeight = baselinePixels.GetLength(0);

            int heightDiff = comparisonPixels.GetLength(0) - baselineHeight;
            //loop over a larger perimeter of the larger image
            //int newStartX = newImageStart.X - (Settings.SearchWidth / 2);
            //int newStartY = newImageStart.Y - (Settings.SearchHeight / 2);
           // int newEndX = newImageStart.X + (Settings.SearchWidth / 2);
           // int newEndY = newImageStart.Y + (Settings.SearchHeight / 2);
            //newEndY += Math.Abs(heightDiff);
            if (baselineStartX < 0)
            {
                baselineStartX = 0;
                if (lastOffset.X == 0 && baselineStart.X == 0)
                {
                    baselineStartX = 1;
                }
            }
            if (baselineStartY < 0)
            {
                baselineStartY = 0;
            }
            if (baselineEndX > baselineWidth)
            {
                baselineEndX = baselineWidth;//-1??
            }
            if (baselineEndY > baselineHeight)
            {
                baselineEndY = baselineHeight;//-1??
            }
            //if (compareHeight + baselineStart.Y > baselineWidth)
            //{
            //    compareHeight = baselineWidth - baselineStart.Y;
            //}
            //if (compareWidth + baselineStart.X > baselineHeight)
            //{
            //    compareWidth = baselineHeight - baselineStart.X;
            //}
            bool matched = false;
            //precheck
            if (lastOffset.X != 0 || lastOffset.Y != 0)
            {
                var checkFirst = new Point(baselineStart.X + lastOffset.X, baselineStart.Y + lastOffset.Y);
                matched = CheckAllMatch(newImageStart, checkFirst);
                if (matched)
                {
                    movedTo = checkFirst;
                    return true;
                }
            }

            //this is looping over baseline
            LogEmiter.LogMessage($"Checking pixels for movement StartLocation: X:{baselineStart.X}, Y:{baselineStart.Y}");
            for (int i = baselineStartX; i < baselineEndX; i++)//width
            {
                for (int j = baselineStartY; j < baselineEndY; j++)//height
                {
                    matched = CheckAllMatch(newImageStart, new Point(i, j));
                    if (matched)
                    {
                        //it has moved fromn here in the baseline
                        movedTo = new Point(i, j);
                        return true;
                    }
                }
            }
            movedTo = new Point(0, 0);
            return false;
        }

        /// <summary>
        /// Given a mismatched pixel, determine if the pixels surrounding it have moved location. Additionally determine what color the background pixel may be
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="movedTo"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool HasMoved( Point newImageStart, Point baselineStart, out Point movedTo, Point lastOffset)
        {
            int compareHeight = Settings.BlockHeight;
            int compareWidth = Settings.BlockWidth;
            int baselineWidth = baselinePixels.GetLength(1);
            int baselineHeight = baselinePixels.GetLength(0);

            int heightDiff = comparisonPixels.GetLength(0) - baselineHeight;
            //loop over a larger perimeter of the larger image
            int newStartX = newImageStart.X - (Settings.SearchWidth / 2);
            int newStartY = newImageStart.Y - (Settings.SearchHeight / 2);
            int newEndX = newImageStart.X + (Settings.SearchWidth / 2);
            int newEndY = newImageStart.Y + (Settings.SearchHeight / 2);
            newEndY += Math.Abs(heightDiff);
            if (newStartX < 0)
            {
                newStartX = 0;
                if (lastOffset.X == 0 && baselineStart.X == 0)
                {
                    newStartX = 1;
                }
            }
            if(newStartY < 0)
            {
                newStartY = 0;
            }
            if (newEndY >= comparisonPixels.GetLength(0))
            {
                newEndY = comparisonPixels.GetLength(0);
            }
            if (newEndX >= comparisonPixels.GetLength(1))
            {
                newEndX = comparisonPixels.GetLength(1);
            }
            if(compareHeight+baselineStart.Y > baselineWidth)
            {
                compareHeight = baselineWidth - baselineStart.Y;
            }
            if (compareWidth + baselineStart.X > baselineHeight)
            {
                compareWidth = baselineHeight - baselineStart.X;
            }
            bool matched = false;
            //precheck
            if (lastOffset.X != 0 || lastOffset.Y != 0)
            {
                matched = CheckAllMatch( new Point(newImageStart.X, newImageStart.Y), baselineStart);
                if (matched)
                {
                    movedTo = new Point(newImageStart.X, newImageStart.Y);
                    return true;
                }
            }

            //this is looping over newr
            LogEmiter.LogMessage($"Checking pixels for movement StartLocation: X:{baselineStart.X}, Y:{baselineStart.Y}");
            for (int i=newStartX; i<newEndX; i++)//width
            {
                for (int j = newStartY; j < newEndY; j++)//height
                {
                    matched = CheckAllMatch( new Point(i, j), baselineStart);
                    //for (int scanRow=0;  scanRow< compareHeight; scanRow++)
                    //{
                    //    //Now loop over smaller image to see if it matches here
                    //    for (int scanCol=0; scanCol < compareWidth; scanCol++)
                    //    {
                    //        //record pixel counds for background pixel determination later
                    //        try
                    //        {
                    //            bool matchedROw = true;
                    //            if (newImage[j + scanCol, i + scanRow].IsMatch(baseline[scanCol + baselineStart.Y, scanRow + baselineStart.X]))
                    //            {
                    //                //LogEmiter.LogMessage($"location: {j + scanCol},{i + scanRow} matched");
                    //                //matched = true; 
                    //            }
                    //            else
                    //            {
                    //                //LogEmiter.LogMessage($"location: {j + scanCol},{i + scanRow} mismatched");
                    //                matched = false;
                    //                break;
                    //            }
                    //        }catch(Exception ex)
                    //        {
                    //            var exx = "";
                    //        }
                    //    }
                    //    if (!matched)
                    //    {
                    //        break;//if any pixel doesnt match then whole block didnt match
                    //    }
                    //}
                    if (matched)
                    {
                        //it has moved here in the new image
                        movedTo = new Point(i, j);
                        return true;
                    }
                }
            }
            movedTo = new Point(0,0);
            return false;
        }

        private bool CheckAllMatch(Point newImageStart, Point baselineStart)
        {
            bool matched = true;
            int rightmostMatch;
            int leftmostMatch;
            double matchThreshold = 0.9;
            int matchedCount = 0;
            int checkedCount = 0;
            for (int scanRow = 0; scanRow < Settings.BlockHeight; scanRow++)
            {
                if (scanRow + newImageStart.X >= comparisonPixels.GetLength(1) || scanRow + baselineStart.X >= baselinePixels.GetLength(1))
                {
                    break;
                }
                //Now loop over smaller image to see if it matches here
                for (int scanCol = 0; scanCol < Settings.BlockWidth; scanCol++)
                {
                    //record pixel counds for background pixel determination later
                    try
                    {
                        int rowMatchCount = 0;
                        if (scanCol + newImageStart.Y >= comparisonPixels.GetLength(0) || scanCol + baselineStart.Y >= baselinePixels.GetLength(0))
                        {
                            break;
                        }
                        bool matchedROw = true;
                        checkedCount++;
                        var currentPixel = comparisonPixels[newImageStart.Y + scanCol, scanRow + newImageStart.X];
                        if (currentPixel.IsBackgroundPixel || currentPixel.IsImagePixel)
                        {
                            break;
                        }
                        if (comparisonPixels[newImageStart.Y + scanCol, scanRow +newImageStart.X].IsMatch(baselinePixels[scanCol + baselineStart.Y, scanRow + baselineStart.X]))
                        {
                            rowMatchCount++;
                            matchedCount++;
                            //LogEmiter.LogMessage($"location: {j + scanCol},{i + scanRow} matched");
                            //matched = true; 
                        }
                        else
                        {
                            return false;
                            //LogEmiter.LogMessage($"location: {j + scanCol},{i + scanRow} mismatched");
                            if (scanCol - rowMatchCount > 2)
                            {
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var exx = "";
                    }
                }
            }
            double matchedPercentage = (double)matchedCount / checkedCount;
            if (matchedPercentage >= matchThreshold)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="largerImage">This should be </param>
        /// <param name="LargeStart"></param>
        /// <param name="SmallerImage"></param>
        /// <param name="smallStart"></param>
        /// <exception cref="NotImplementedException"></exception>
        private Rectangle PerformMovedComparison(Point baselineStart,  Point newImageStart, bool hasBackgroundColor, Colour backgroundColor)
        {
            Dictionary<string, int> pixelCounts = new Dictionary<string, int>();

            var width = comparisonPixels.GetLength(1);
            var height = comparisonPixels.GetLength(0);
            var baselineWidth = baselinePixels.GetLength(1);
            var baselineHeight = baselinePixels.GetLength(0);
            int newImageWidthOffset = newImageStart.X;
            int newImageHeightOffset = newImageStart.Y;
            int baselineWidthOffset = baselineStart.X;
            int baselineHeightOffset = baselineStart.Y;
            int maxRight = 0;
            int maxLeft = 0;//shouldnt be needed
            int heightScanned = 0;//shouldnt be needed
            int MaxBottom = 0;
            bool mismatch1 = false;
            bool mismatch2 = false;
            var newStartPixel = comparisonPixels[newImageStart.Y, newImageStart.X];
            var oldStartPixel = baselinePixels[baselineStart.Y, baselineStart.X];
            if (!comparisonPixels[newImageStart.Y, newImageStart.X].IsMatch(baselinePixels[baselineStart.Y, baselineStart.X]))
            {
                //this should never be hit
                var d = "Fail";
            }

            //extent of move is unknown. Perform a scan to figure out the bounds and mark all moved pixels as processed to prevent later scanning
            //scan left to right, top to bottom
            for (int i = 0; i < (height-newImageHeightOffset); i++)//for each row
            {
                //heightScanned = i;
                //if (i%3 == 0 && i>0)
                //{
                //    backgroundColor = GetUpdatedBackgroundColor(pixelCounts, width * 3);
                //    pixelCounts = new Dictionary<string, int>();
                //}
                if (mismatch1 && mismatch2)
                {
                    break;
                }
                
                if (i + baselineHeightOffset >= baselineHeight)
                {
                    break;
                }
                for (int j = 0; j < (width- newImageWidthOffset); j++)//for each column
                {
                    //check bounds are not exceeded
                    if (mismatch1 && j>= maxRight)
                    {
                        break;
                    }
                    if (j + baselineWidthOffset >= baselineWidth)
                    {
                        break;
                    }
                    WindowsPixel newImagePixel = new WindowsPixel(255,0,0, 0, 0); 
                    WindowsPixel baselinePixel = new WindowsPixel(255,0,0,0,0);
                    //check pixel
                    try
                    {
                        newImagePixel = comparisonPixels[i + newImageHeightOffset, j + newImageWidthOffset];
                        baselinePixel = baselinePixels[i + baselineHeightOffset,j + baselineWidthOffset];
                        var key = $"{baselinePixel.Colour.R}_{baselinePixel.Colour.G}_{baselinePixel.Colour.B}";
                        if (pixelCounts.ContainsKey(key))
                        {
                            pixelCounts[key]++;
                        }
                        else
                        {
                            pixelCounts.Add(key, 1);
                        }
                    }
                    catch(Exception ex)
                    {
                        var ff = "";
                    }
                    if (newImagePixel.IsMatch(baselinePixel))
                    {
                        //newImagePixel = newImage[i + newImageHeightOffset, j + newImageWidthOffset];
                        //baselinePixel = baseline[i + baselineHeightOffset, j + baselineWidthOffset];
                        var x = i + newImageHeightOffset;
                        var y= j + newImageWidthOffset;
                        outputImage[i + newImageHeightOffset, j + newImageWidthOffset].processed = true;
                        outputImage[i + newImageHeightOffset, j + newImageWidthOffset].IsMoved = true;
                        comparisonPixels[i + newImageHeightOffset, j + newImageWidthOffset].processed = true;
                        comparisonPixels[i + newImageHeightOffset, j + newImageWidthOffset].IsMoved = true;
                        baselinePixels[i + baselineHeightOffset, j + baselineWidthOffset].IsMoved = true;
                        baselinePixels[i + baselineHeightOffset, j + baselineWidthOffset].processed = true;
                        //if its a background pixel we dont want to highlight
                        if (hasBackgroundColor && baselinePixels[i + baselineHeightOffset, j + baselineWidthOffset].IsMatch( backgroundColor))
                        {
                            baselinePixels[i+ baselineHeightOffset, j + baselineWidthOffset].needsHighlight = false;
                            outputImage[i + newImageHeightOffset, j + newImageWidthOffset].needsHighlight = false;
                        }
                        else
                        {
                            baselinePixels[i + baselineHeightOffset, j + baselineWidthOffset].needsHighlight = true;
                            outputImage[i + newImageHeightOffset, j + newImageWidthOffset].needsHighlight = true;
                        }
                    }
                    else
                    {
                        if (!mismatch1)
                        {
                            LogEmiter.LogMessage($"Found First Mismatch at {i + newImageHeightOffset},{j + newImageWidthOffset}");
                            //first mismatch should be on right
                            maxRight = j;
                            mismatch1 = true;
                        }
                        else
                        {
                            if (i < 10)
                            {
                                if (j < maxRight)
                                {
                                    maxRight = j;
                                }
                                continue;
                            }
                            LogEmiter.LogMessage($"Found Second Mismatch at {i + newImageHeightOffset},{j + newImageWidthOffset}. Moved size was {i}x{j}");
                            //second mismatch    should be bottom we are now done                       
                            MaxBottom = i;
                            if (MovedSectionCount % 20 == 0)
                            {
                                var progressionImage2 = GetImageFromPixals(outputImage);
                                progressionImage2.Save($"C:\\tmp\\Progression_{DateTime.Now.Ticks}.jpg");
                            }
                            MovedSectionCount++;
                            if (maxRight == 0) maxRight = j;
                            return new Rectangle(newImageStart, new Size(maxRight, i)); 
                        }
                        //if not matched try determine if is a right or bottom boundary
                        //continue looking in direction boundary not yet reached
                        //if both boundries reached then return the rectangle bounds
                    }
                }
            }
            if (maxRight == 0) maxRight = width;
            return new Rectangle(newImageStart, new Size(maxRight, heightScanned));
           // var progressionImage = GetImageFromPixals(outputImage);
            //progressionImage.Save($"C:\\tmp\\Progression_{DateTime.Now.Ticks}.jpg");
        }


        private Color GetUpdatedBackgroundColor(Dictionary<string,int> colorList, int totalCount)
        {
            Color backgroundColor;
            var threshold = totalCount * 0.44;
            if (colorList.Any(x => x.Value >= threshold))
            {
                var key = colorList.First(x => x.Value >= threshold);
                var vals = key.Key.Split("_");
                return Color.FromArgb(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));               
            }
            return Color.White;
        }

        private WindowsPixel[,] ProcessPixels(BitmapData image)
        {
            int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;
            int stride = image.Stride;

            byte[] buffer = new byte[image.Height * stride];

            Marshal.Copy(image.Scan0, buffer, 0, buffer.Length);
            var pixels = new WindowsPixel[image.Height, image.Width];
            for (int i = 0; i < image.Height; i++)
            {
                for (int widthIndex = 0; widthIndex < image.Width; widthIndex++)
                {
                    int rowOffset = i * stride;
                    var byteIndex = (bytesPerPixel * widthIndex) + rowOffset;
                    var pixelColor = Colour.FromArgb(buffer[byteIndex + 2], buffer[byteIndex + 1], buffer[byteIndex]);
                    pixels[i, widthIndex] = new WindowsPixel(pixelColor, i, widthIndex);
                }
            }
            return pixels;
        }

        private Bitmap GetImageFromPixals(WindowsPixel[,] pixels)
        {
            int width = pixels.GetLength(1);
            int height = pixels.GetLength(0);
            //rebuild image for testing
            Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if(pixels[i, j].needsHighlight && pixels[i, j].IsMoved)
                    {
                        saveImage.SetPixel(j, i, Color.Green);
                    }
                    else if (pixels[i, j].IsImagePixel)
                    {
                        saveImage.SetPixel(j, i, Color.Yellow);
                    }
                    else if (pixels[i, j].IsBackgroundPixel)
                    {
                        saveImage.SetPixel(j, i, Color.Black);
                    }
                    else if (pixels[i, j].needsHighlight)
                    {
                        saveImage.SetPixel(j, i, Color.Magenta);
                    }
                    else if (pixels[i, j].processed)
                    {
                        saveImage.SetPixel(j, i, Color.Black);
                    }
                    else
                    {
                        saveImage.SetPixel(j, i, pixels[i, j].Colour.ToColor());
                    }
                }
            }
            return saveImage;
        }

        //static Bitmap HighlightDifferences(Bitmap image1, Bitmap image2)
        //{
        //    int width = Math.Min(image1.Width, image2.Width);
        //    int height = Math.Min(image1.Height, image2.Height);
        //    int maxWidth = Math.Max(image1.Width, image2.Width);
        //    int maxHeight = Math.Max(image1.Height, image2.Height);

        //    Bitmap diffImage = new Bitmap(maxWidth, maxHeight, PixelFormat.Format24bppRgb);

        //    BitmapData data1 = image1.LockBits(new Rectangle(0, 0, image1.Width, image1.Height), ImageLockMode.ReadOnly, image1.PixelFormat);
        //    BitmapData data2 = image2.LockBits(new Rectangle(0, 0, image2.Width, image2.Height), ImageLockMode.ReadOnly, image2.PixelFormat);
        //    BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, diffImage.Width, diffImage.Height), ImageLockMode.WriteOnly, diffImage.PixelFormat);

        //    int bytesPerPixel = Image.GetPixelFormatSize(image1.PixelFormat) / 8;
        //    int stride1 = data1.Stride;
        //    int stride2 = data2.Stride;
        //    int diffStride = diffData.Stride;

        //    byte[] buffer1 = new byte[data1.Height * stride1];
        //    byte[] buffer2 = new byte[data2.Height * stride2];
        //    byte[] bufferDiff = new byte[diffData.Height * diffStride];

        //    Marshal.Copy(data1.Scan0, buffer1, 0, buffer1.Length);
        //    Marshal.Copy(data2.Scan0, buffer2, 0, buffer2.Length);
        //    Marshal.Copy(diffData.Scan0, bufferDiff, 0, bufferDiff.Length);

        //    bool[,] visited = new bool[height / BlockSize, width / BlockSize];

        //    for (int y = 0; y < height; y += BlockSize)
        //    {
        //        for (int x = 0; x < width; x += BlockSize)
        //        {
        //            if (!IsVisited(visited, x, y))
        //            {
        //                Point match = FindBestMatch(buffer1, buffer2, x, y, stride1, stride2, bytesPerPixel);
        //                if (match.X != -1 && match.Y != -1)
        //                {
        //                    CopyBlock(buffer2, bufferDiff, match.X, match.Y, stride2, diffStride, bytesPerPixel);
        //                    SetVisited(visited, match.X, match.Y);
        //                }
        //                else
        //                {
        //                    HighlightBlock(bufferDiff, buffer1, buffer2, x, y, stride1, stride2, diffStride, bytesPerPixel);
        //                }
        //            }
        //        }
        //    }

        //    // Fill remaining areas from image1 and image2 as in the previous code
        //    FillRemainingArea(bufferDiff, buffer1, width, height, diffStride, stride1, bytesPerPixel);
        //    FillRemainingArea(bufferDiff, buffer2, width, height, diffStride, stride2, bytesPerPixel);

        //    Marshal.Copy(bufferDiff, 0, diffData.Scan0, bufferDiff.Length);

        //    image1.UnlockBits(data1);
        //    image2.UnlockBits(data2);
        //    diffImage.UnlockBits(diffData);

        //    return diffImage;
        //}

        //static bool IsVisited(bool[,] visited, int x, int y)
        //{
        //    int blockX = x / BlockSize;
        //    int blockY = y / BlockSize;
        //    if (blockX < 0 || blockX >= visited.GetLength(1) || blockY < 0 || blockY >= visited.GetLength(0))
        //    {
        //        return true; // Treat out of bounds as visited to avoid exceptions
        //    }
        //    return visited[blockY, blockX];
        //}

        //static void SetVisited(bool[,] visited, int x, int y)
        //{
        //    int blockX = x / BlockSize;
        //    int blockY = y / BlockSize;
        //    if (blockX >= 0 && blockX < visited.GetLength(1) && blockY >= 0 && blockY < visited.GetLength(0))
        //    {
        //        visited[blockY, blockX] = true;
        //    }
        //}

        //static Point FindBestMatch(byte[] buffer1, byte[] buffer2, int startX, int startY, int stride1, int stride2, int bytesPerPixel)
        //{
        //    int bestMatchX = -1;
        //    int bestMatchY = -1;
        //    double bestDifference = double.MaxValue;

        //    for (int offsetY = -SearchWindow; offsetY <= SearchWindow; offsetY++)
        //    {
        //        for (int offsetX = -SearchWindow; offsetX <= SearchWindow; offsetX++)
        //        {
        //            int x2 = startX + offsetX;
        //            int y2 = startY + offsetY;
        //            if (x2 >= 0 && y2 >= 0 && x2 + BlockSize <= buffer2.Length / stride2 * bytesPerPixel && y2 + BlockSize <= buffer2.Length / stride2 * bytesPerPixel)
        //            {
        //                double difference = CalculateBlockDifference(buffer1, buffer2, startX, startY, x2, y2, stride1, stride2, bytesPerPixel);
        //                if (difference < bestDifference)
        //                {
        //                    bestDifference = difference;
        //                    bestMatchX = x2;
        //                    bestMatchY = y2;
        //                }
        //            }
        //        }
        //    }

        //    if (bestDifference <= Threshold)
        //    {
        //        return new Point(bestMatchX, bestMatchY);
        //    }
        //    return new Point(-1, -1);
        //}

        //static double CalculateBlockDifference(byte[] buffer1, byte[] buffer2, int x1, int y1, int x2, int y2, int stride1, int stride2, int bytesPerPixel)
        //{
        //    double difference = 0;
        //    for (int y = 0; y < BlockSize; y++)
        //    {
        //        for (int x = 0; x < BlockSize; x++)
        //        {
        //            int index1 = ((y1 + y) * stride1) + ((x1 + x) * bytesPerPixel);
        //            int index2 = ((y2 + y) * stride2) + ((x2 + x) * bytesPerPixel);

        //            if (index1 < buffer1.Length && index2 < buffer2.Length)
        //            {
        //                for (int i = 0; i < bytesPerPixel; i++)
        //                {
        //                    difference += Math.Abs(buffer1[index1 + i] - buffer2[index2 + i]);
        //                }
        //            }
        //        }
        //    }
        //    return difference / (BlockSize * BlockSize * bytesPerPixel);
        //}

        //static bool IsBlockMoved(byte[] buffer1, byte[] buffer2, int startX, int startY, int stride1, int stride2, int bytesPerPixel)
        //{
        //    for (int offsetY = -SearchWindow; offsetY <= SearchWindow; offsetY++)
        //    {
        //        for (int offsetX = -SearchWindow; offsetX <= SearchWindow; offsetX++)
        //        {
        //            if (BlocksMatch(buffer1, buffer2, startX, startY, startX + offsetX, startY + offsetY, stride1, stride2, bytesPerPixel))
        //            {
        //                return true;
        //            }
        //        }
        //    }
        //    return false;
        //}

        //static bool BlocksMatch(byte[] buffer1, byte[] buffer2, int x1, int y1, int x2, int y2, int stride1, int stride2, int bytesPerPixel)
        //{
        //    for (int y = 0; y < BlockSize; y++)
        //    {
        //        for (int x = 0; x < BlockSize; x++)
        //        {
        //            int index1 = ((y1 + y) * stride1) + ((x1 + x) * bytesPerPixel);
        //            int index2 = ((y2 + y) * stride2) + ((x2 + x) * bytesPerPixel);

        //            if (index1 < 0 || index2 < 0 || index1 >= buffer1.Length || index2 >= buffer2.Length)
        //            {
        //                return false;
        //            }

        //            for (int i = 0; i < bytesPerPixel; i++)
        //            {
        //                if (Math.Abs(buffer1[index1 + i] - buffer2[index2 + i]) > Threshold)
        //                {
        //                    return false;
        //                }
        //            }
        //        }
        //    }
        //    return true;
        //}

        //static void CopyBlock(byte[] source, byte[] destination, int startX, int startY, int sourceStride, int destStride, int bytesPerPixel)
        //{
        //    for (int y = 0; y < BlockSize; y++)
        //    {
        //        for (int x = 0; x < BlockSize; x++)
        //        {
        //            int srcIndex = ((startY + y) * sourceStride) + ((startX + x) * bytesPerPixel);
        //            int destIndex = ((startY + y) * destStride) + ((startX + x) * bytesPerPixel);

        //            if (srcIndex < source.Length && destIndex < destination.Length)
        //            {
        //                Array.Copy(source, srcIndex, destination, destIndex, bytesPerPixel);
        //            }
        //        }
        //    }
        //}

        //static void HighlightBlock(byte[] buffer, byte[] buffer1, byte[] buffer2, int startX, int startY, int stride1, int stride2, int diffStride, int bytesPerPixel)
        //{
        //    for (int y = 0; y < BlockSize; y++)
        //    {
        //        for (int x = 0; x < BlockSize; x++)
        //        {
        //            int index1 = ((startY + y) * stride1) + ((startX + x) * bytesPerPixel);
        //            int index2 = ((startY + y) * stride2) + ((startX + x) * bytesPerPixel);
        //            int diffIndex = ((startY + y) * diffStride) + ((startX + x) * bytesPerPixel);

        //            if (index1 < buffer1.Length && index2 < buffer2.Length && diffIndex < buffer.Length)
        //            {
        //                bool isForegroundPixel = false;

        //                for (int i = 0; i < bytesPerPixel; i++)
        //                {
        //                    if (Math.Abs(buffer1[index1 + i] - buffer2[index2 + i]) > Threshold)
        //                    {
        //                        isForegroundPixel = true;
        //                        break;
        //                    }
        //                }

        //                if (isForegroundPixel)
        //                {
                            
        //                    buffer[diffIndex] = Color.Magenta.R; // Red
        //                    buffer[diffIndex + 1] = Color.Magenta.G; // Green
        //                    buffer[diffIndex + 2] = Color.Magenta.B; // Blue
        //                    if (bytesPerPixel == 4)
        //                    {
        //                        buffer[diffIndex + 3] = 255; // Alpha
        //                    }
        //                }
        //                else
        //                {
        //                    Array.Copy(buffer1, index1, buffer, diffIndex, bytesPerPixel);
        //                }
        //            }
        //        }
        //    }
        //}

        //static void FillRemainingArea(byte[] diffBuffer, byte[] sourceBuffer, int width, int height, int diffStride, int sourceStride, int bytesPerPixel)
        //{
        //    for (int y = height; y < sourceBuffer.Length / sourceStride; y++)
        //    {
        //        for (int x = 0; x < sourceStride / bytesPerPixel; x++)
        //        {
        //            int srcIndex = (y * sourceStride) + (x * bytesPerPixel);
        //            int destIndex = (y * diffStride) + (x * bytesPerPixel);

        //            if (srcIndex < sourceBuffer.Length && destIndex < diffBuffer.Length)
        //            {
        //                Array.Copy(sourceBuffer, srcIndex, diffBuffer, destIndex, bytesPerPixel);
        //            }
        //        }
        //    }

        //    for (int y = 0; y < sourceBuffer.Length / sourceStride; y++)
        //    {
        //        for (int x = width; x < sourceStride / bytesPerPixel; x++)
        //        {
        //            int srcIndex = (y * sourceStride) + (x * bytesPerPixel);
        //            int destIndex = (y * diffStride) + (x * bytesPerPixel);

        //            if (srcIndex < sourceBuffer.Length && destIndex < diffBuffer.Length)
        //            {
        //                Array.Copy(sourceBuffer, srcIndex, diffBuffer, destIndex, bytesPerPixel);
        //            }
        //        }
        //    }
        //}


        ///// <summary>
        ///// Checks the areaa around a mismatched pixel to find mismatches within a boundary length (2 pixels) away
        ///// </summary>
        ///// <param name="rowStart"></param>
        ///// <param name="columnStart"></param>
        ///// <returns></returns>
        //public ImagePixels IdentifyMismatchBounds2(int columnStart, int rowStart)
        //{
        //    List<WindowsPixel> mismatched = new List<WindowsPixel>() { comparisonPixels[columnStart, rowStart] };
        //    List<WindowsPixel> ToReview = new List<WindowsPixel>();
        //    List<WindowsPixel> borderPixels = new List<WindowsPixel>();
        //    for (int row = rowStart - traversalBorder; row < rowStart + traversalBorder + 1; row++)
        //    {
        //        if (row < 0)
        //        {
        //            continue;
        //        }
        //        var rowMax = comparisonPixels.GetLength(1);
        //        var colMax = comparisonPixels.GetLength(0);
        //        if (row >= rowMax)
        //        {
        //            break;
        //        }
        //        for (int column = columnStart - traversalBorder; column < columnStart + traversalBorder + 1; column++)
        //        {
        //            if (column < 0)
        //            {
        //                continue;
        //            }
        //            if (column >= colMax)
        //            {
        //                break;
        //            }
        //            if (column >= colMax)
        //            {
        //                break;
        //            }
        //            try
        //            {
        //                if (!ComparePixels(comparisonPixels[column, row], baselinePixels[column, row]))
        //                {
        //                    ToReview.Add(comparisonPixels[column, row]);
        //                }
        //            }catch(Exception ex)
        //            {
        //                var s = "";
        //            }
        //        }
        //    }
        //    if(ToReview.Count == 0)
        //    {
        //        var ww = "";
        //    }
        //    if (mismatched.Count == 0)
        //    {
        //        var ww = "";
        //    }
        //    mismatched = NavigateMismatched(mismatched, ToReview, out borderPixels);
        //    if(borderPixels.Count == 0)
        //    {
        //        borderPixels.Add(mismatched[0]);
        //    }
        //    else
        //    {
        //        var dd = "";
        //    }
        //    //check to see if all border pixels are the same
        //    WindowsPixel borderPix = borderPixels[0];
        //    bool allBorderPixelsTheSame = false;
        //    Dictionary<string, int> counts = new Dictionary<string, int>();
        //    // find the counts of each pixel colour
        //    foreach (var pixel in borderPixels)
        //    {
        //        var key = $"{pixel.Colour.R}_{pixel.Colour.G}_{pixel.Colour.B}";
        //        if (counts.ContainsKey(key))
        //        {
        //            counts[key]++;
        //        }
        //        else
        //        {
        //            counts.Add(key, 1);
        //        }
        //    }
        //    //if more than 80% of pixels are the same consider them the colour of the border
        //    var threshold = borderPixels.Count * 0.8;
        //    if (counts.Any(x => x.Value >= threshold))
        //    {
        //        var key = counts.First(x => x.Value >= threshold);
        //        var vals = key.Key.Split("_");
        //        borderPix.pixel = Color.FromArgb(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));
        //        //borderPix.pixel.R = byte.Parse(vals[0]);
        //        //borderPix.pixel.G = byte.Parse(vals[1]);
        //        //borderPix.pixel.B = byte.Parse(vals[2]);
        //        allBorderPixelsTheSame = true;
        //    }
        //    var startRow = mismatched[0].Row;
        //    var endRow = mismatched[0].Row;
        //    var startColumn = mismatched[0].Column;
        //    var endColumn = mismatched[0].Column;
        //    var shouldNotHightlight = mismatched.Any(x => x.pixel.B == borderPix.pixel.B && x.pixel.R == borderPix.pixel.R && x.pixel.G == borderPix.pixel.G);
        //    if (shouldNotHightlight && allBorderPixelsTheSame)
        //    {
        //        var d = "";
        //    }
        //    foreach (var pixel in mismatched)
        //    {
        //        if (pixel.Row < startRow) startRow = pixel.Row;
        //        if (pixel.Row > endRow) endRow = pixel.Row;
        //        if (pixel.Column < startColumn) startColumn = pixel.Column;
        //        if (pixel.Column > endColumn) endColumn = pixel.Column;
        //    }
        //    var mismatchRegion = new ImagePixels();
        //    for (int row = startRow; row < endRow + 1; row++)
        //    {
        //        var baseRow = new Row();
        //        for (int col = startColumn; col < endColumn + +1; col++)
        //        {
        //            if (allBorderPixelsTheSame)
        //            {
        //                if (comparisonPixels[row,col].IsMatch(borderPix, 20))
        //                {
        //                    comparisonPixels[row, col].isBorderPixel = true;
        //                }
        //                else
        //                {
        //                    var dd = "";
        //                }
        //            }
        //            else
        //            {
        //                comparisonPixels[row, col].isBorderPixel = false;
        //            }
        //            baseRow.Add(comparisonPixels[row, col]);//swapped row/col
        //        }
        //        mismatchRegion.AddRow(baseRow);
        //    }
        //    if (mismatchRegion.Rows.Count == 0 || mismatchRegion.Rows[0].Count == 0)
        //    {
        //        var s = "";
        //    }
        //    //LogScanner();
        //    //logger.SaveImageAsPng(mismatchRegion, $"MismatychAt{rowStart}-{columnStart}");
        //    return mismatchRegion;
        //}

        ///// <summary>
        ///// Navigates through surrounding pixels to find the extent of a missmatch
        ///// </summary>
        ///// <param name="mismatched"></param>
        ///// <param name="toReview"></param>
        ///// <param name="borderPixels"></param>
        ///// <returns></returns>
        //public List<WindowsPixel> NavigateMismatched(List<WindowsPixel> mismatched, List<WindowsPixel> toReview, out List<WindowsPixel> borderPixels)
        //{
        //    borderPixels = new List<WindowsPixel>();
        //    List<WindowsPixel> newForReview = new List<WindowsPixel>();
        //    var rowMax = comparisonPixels.GetLength(1);
        //    var colMax = comparisonPixels.GetLength(0);
        //    var baseRowMax = comparisonPixels.GetLength(1);
        //    var baseColMax = comparisonPixels.GetLength(0);
        //    foreach (var underReview in toReview)
        //    {
        //        for (int row = underReview.Row - traversalBorder; row < underReview.Row + traversalBorder + 1; row++)
        //        {
        //            if (row < 0)
        //            {
        //                continue;
        //            }
        //            if (row >= rowMax)
        //            {
        //                break;
        //            }
        //            for (int column = underReview.Column - traversalBorder; column < underReview.Column + traversalBorder + 1; column++)
        //            {
        //                if (column < 0)
        //                {
        //                    continue;
        //                }
        //                if (column >= colMax)
        //                {
        //                    break;
        //                }
        //                if(row>=baseRowMax || column >= baseColMax)
        //                {
        //                    break;
        //                }
        //                //if (!mismatched.Any(X => X.Column == column && X.Row == row)) {
        //                if (!comparisonPixels[row,column].processed)
        //                {
        //                    comparisonPixels[row,column].processed = true;
        //                    if (!ComparePixels(comparisonPixels[row,column], baselinePixels[row,column]))
        //                    {
        //                        outputImage[row,column].pixel = Color.Magenta;
        //                        //LogScanner();
        //                        mismatched.Add(comparisonPixels[row,column]);
        //                        newForReview.Add(comparisonPixels[row,column]);
        //                    }
        //                    else
        //                    {
        //                        borderPixels.Add(outputImage[row,column]);
        //                        //scanning.Rows[row].Pixels[column].pixel = Color.Navy;
        //                        //LogScanner();
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    if (newForReview.Count == 0)
        //    {
        //        return mismatched;
        //    }
        //    else
        //    {
        //        var moreBorderPixels = new List<WindowsPixel>();
        //        var mismatch = NavigateMismatched(mismatched, newForReview, out moreBorderPixels);
        //        borderPixels.AddRange(moreBorderPixels);
        //        return mismatch;
        //    }
        //}
        ////public ImagePixels IdentifyMismatchBounds(int rowStart, int columnStart)
        ////{
        ////    //todo: need to be able to scan left also
        ////    int width = 0;
        ////    int height = 0;
        ////    int heightMisMatches = 0;
        ////    int sideMatches = 0;
        ////    for (int row = rowStart; row < comparisonPixels.Count; row++)
        ////    {
        ////        bool doneHeight = false;
        ////        if (row >= baselinePixels.Count)
        ////        {
        ////            //break;
        ////            // comparisonPixels[row][column].pixel = Color.Magenta;                        
        ////        }
        ////        for (int column = columnStart; column < comparisonPixels[0].Count; column++)
        ////        {
        ////            if (baselinePixels[row].Count <= column)
        ////            {
        ////                //break;
        ////            }
        ////            scanning.Rows[row].Pixels[column].pixel = Color.Magenta;
        ////            LogScanner();
        ////            //var scanningProgress = System.Drawing.Image.FromFile($"{Application.StartupPath}\\Images\\Scanning.png");
        ////            //pictureBox1.Image = scanningProgress;
        ////            if (ComparePixels(comparisonPixels[row][column], baselinePixels[row][column]))
        ////            {
        ////                sideMatches++;                        
        ////            }
        ////            else
        ////            {
        ////                if (!doneHeight)
        ////                {
        ////                    heightMisMatches++;
        ////                    doneHeight = true;
        ////                }
        ////                sideMatches = 0;
        ////            }
        ////            comparisonPixels[row][column].processed = true;
        ////            if (column > width)
        ////            {
        ////                width = column;
        ////            }
        ////            if (sideMatches >= rowBuffer)
        ////            {
        ////                break;
        ////            }
        ////        }
        ////        if ((row- rowStart)- heightMisMatches >= 10)
        ////        {
        ////            break;
        ////        }
        ////    }
        ////    width = width - columnStart;
        ////    height = heightMisMatches;
        ////    //for (int column = columnStart; column < comparisonPixels[0].Count; column++)
        ////    //{
        ////    //    if (baselinePixels[0].Count <= column)
        ////    //    {
        ////    //        break;
        ////    //    }
        ////    //    int heightMatches = 0;
        ////    //    for (int row = rowStart; row < comparisonPixels.Count; row++)
        ////    //    {
        ////    //        if (row >= baselinePixels.Count)
        ////    //        {
        ////    //            break;
        ////    //        }


        ////    //        if (ComparePixels(comparisonPixels[row][column], baselinePixels[row][column]))
        ////    //        {
        ////    //            heightMatches++;
        ////    //        }
        ////    //        else
        ////    //        {
        ////    //            heightMatches = 0;
        ////    //        }
        ////    //        comparisonPixels[row][column].processed = true;
        ////    //        if (row > height)
        ////    //        {
        ////    //            height = row;
        ////    //        }
        ////    //        if (heightMatches == columnBuffer)
        ////    //        {
        ////    //            break;
        ////    //        }
        ////    //    }
        ////    //}
        ////    int count = 0;
        ////    foreach(var row in comparisonPixels)
        ////    {
        ////        foreach (var pixel in row)
        ////        {
        ////            if (pixel.processed)
        ////            {
        ////                count++;
        ////            }
        ////        }
        ////    }


        ////    var mismatchRegion = new ImagePixels();
        ////    for (int row = rowStart; row < height + rowStart; row++)
        ////    {
        ////        var baseRow = new Row();
        ////        for (int col = columnStart; col < width + columnStart; col++)
        ////        {
        ////            baseRow.Add(baselinePixels[row][col]);//swapped row/col
        ////        }
        ////        mismatchRegion.AddRow(baseRow);
        ////    }
        ////    if (mismatchRegion.Rows.Count == 0 || mismatchRegion.Rows[0].Count == 0)
        ////    {
        ////        var s = "";
        ////    }
        ////    //logger.SaveImageAsPng(mismatchRegion, $"MismatychAt{rowStart}-{columnStart}");
        ////    return mismatchRegion;
        ////}

        ////private void LogScanner(bool overrideLogging = false)
        ////{
        ////    if (!LoggingOn && !overrideLogging)
        ////    {
        ////        return;
        ////    }
        ////    scanningCHangedCount++;
        ////    //if (scanningCHangedCount % 1 == 0)
        ////    //{
        ////    if (currentScannerImage == "Scanning")
        ////    {
        ////        pictureBox1.Image = null;
        ////        logger.SaveImageAsBitmap(scanning, "Scanning2");
        ////        pictureBox1.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes($"{Application.StartupPath}\\Images\\Scanning2.bmp")));
        ////        //pictureBox1.Image = System.Drawing.Image.FromFile($"{Application.StartupPath}\\Images\\Scanning2.bmp");
        ////        currentScannerImage = "Scanning2";
        ////        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        ////        pictureBox1.Refresh();
        ////    }
        ////    else
        ////    {
        ////        pictureBox1.Image = null;
        ////        logger.SaveImageAsBitmap(scanning, "Scanning");
        ////        pictureBox1.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes($"{Application.StartupPath}\\Images\\Scanning.bmp")));
        ////        currentScannerImage = "Scanning";
        ////        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        ////        pictureBox1.Refresh();
        ////    }
        ////    //}
        ////}

        //public void ValidateMismatch(ImagePixels mismatch, int rowStart, int columnStart)
        //{
        //    var rowMax = comparisonPixels.GetLength(1);
        //    var colMax = comparisonPixels.GetLength(0);
        //    int baseStartColumn = columnStart - searchWidth;
        //    int baseEndColumn = columnStart + searchWidth + mismatch.Rows[0].Count;
        //    if (baseStartColumn < 0) baseStartColumn = 0;
        //    if (baseEndColumn >= baselinePixels.GetLength(0)) baseEndColumn = baselinePixels.GetLength(1) - 1;
        //    int baseRowStart = rowStart - searchHeight;
        //    int baseRowEnd = rowStart + searchHeight + mismatch.Rows.Count;
        //    if (baseRowStart < 0) baseRowStart = 0;
        //    if (baseRowEnd >= baselinePixels.GetLength(1)) baseRowEnd = baselinePixels.GetLength(1) - 1;
        //    //int width = mismatch.Rows[0].Count + searchWidth * 2;
        //    //int height = mismatch.Rows.Count + searchHeight;
        //    var baseRows = new ImagePixels();
        //    for (int row = baseRowStart; row < baseRowEnd; row++)
        //    {
        //        var baseRow = new Row();
        //        for (int col = baseStartColumn; col < baseEndColumn; col++)
        //        {
        //            if (baselinePixels.GetLength(0) <= col)
        //            {
        //                break;
        //            }
        //            baseRow.Add(baselinePixels[col, row]);
        //        }
        //        baseRows.AddRow(baseRow);
        //    }

        //    // var basePath = logger.SaveImageAsPng(baseRows, "BaseForCompare");
        //    // var mismatchPath = logger.SaveImageAsPng(mismatch, "MismatchForCompare");
        //    //for (int row = baseRowStart; row < baseRowEnd; row++)
        //    //{
        //    //    //try
        //    //    //{
        //    //        if (scanning.Rows.Count > row && scanning.Rows[row].Pixels.Count > baseEndColumn)
        //    //        {
        //    //            scanning.Rows[row].Pixels[baseStartColumn].pixel = Color.Red;
        //    //            scanning.Rows[row].Pixels[baseEndColumn].pixel = Color.Red;
        //    //        }

        //    //    //}
        //    //    //catch {
        //    //    //    var s = "";
        //    //    //}
        //    //}
        //    //for (int column = baseStartColumn; column < baseEndColumn; column++)
        //    //{
        //    //    //try
        //    //    //{
        //    //        if (scanning.Rows.Count > baseRowEnd && scanning.Rows[0].Pixels.Count > column)
        //    //        {
        //    //            scanning.Rows[baseRowStart].Pixels[column].pixel = Color.Red;
        //    //            scanning.Rows[baseRowEnd].Pixels[column].pixel = Color.Red;
        //    //        }

        //    //    //}
        //    //    //catch {
        //    //    //    var s = "";
        //    //    //}
        //    //}

        //    //LogScanner();
        //    bool hasMoved = baseRows.ContainsImage(mismatch);
        //    string mismatchText = "";
        //    string baseText = "";
        //    //if (hasMoved == true)
        //    //{
        //    //    var s = "";
        //    //}
        //    //else
        //    //{
        //    //    ImageTextReader reader = new ImageTextReader();
        //    //    var text = reader.ReadText(mismatchPath).Trim();
        //    //    if (!string.IsNullOrEmpty(text))
        //    //    {
        //    //        if (text.Contains("chief", StringComparison.CurrentCultureIgnoreCase))
        //    //        {
        //    //            var s = "";
        //    //        }
        //    //        baseText = reader.ReadText(basePath).Trim();

        //    //        if (baseText.Contains(text))
        //    //        {
        //    //            hasMoved = true;
        //    //        }
        //    //    }
        //    //}
        //    //using (var api = OcrApi.Create())
        //    //{
        //    //    api.Init(Languages.English);

        //    //    mismatchText = api.GetTextFromImage(mismatchPath);
        //    //    if (!string.IsNullOrEmpty(mismatchText))
        //    //    {
        //    //        baseText= api.GetTextFromImage(basePath);

        //    //        if (baseText.Contains(mismatchText)){
        //    //            hasMoved = true;
        //    //        }
        //    //    }
        //    //}


        //    var matchColour = mismatch.Rows[mismatch.Rows.Count - 1].Pixels[0];
        //    foreach (var row in mismatch.Rows)
        //    {
        //        foreach (var pixel in row.Pixels)
        //        {
        //            //if (comparisonPixels[pixel.Row,pixel.Column].isDifferent)
        //            //{
        //            //    comparisonPixels[pixel.Row,pixel.Column].needsHighlight = !hasMoved;
        //            //}

        //            if (comparisonPixels[pixel.Column, pixel.Row].isDifferent)
        //            {
        //                comparisonPixels[pixel.Column, pixel.Row].needsHighlight = !hasMoved;
        //            }
        //        }
        //    }
        //}

        public static Bitmap ScaleImageToWidth(Bitmap image, int desiredWidth)
        {
            var ratio = (double)desiredWidth / image.Width;

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
           

        //private bool ComparePixels(WindowsPixel basepix, WindowsPixel other)
        //{
        //    try
        //    {
        //        if (basepix.Colour == other.Colour)
        //        {
        //            outputImage[basepix.Row, basepix.Column].isDifferent = false;
        //            return true;
        //        }
        //        outputImage[basepix.Row, basepix.Column].isDifferent = true;
        //    }catch(Exception ex)
        //    {
        //        var d = "";
        //    }
        //    return false;
        //}

        private bool CompareBorderPixels(WindowsPixel basepix, Color other)
        {
            //if (basepix.pixel.Rgb == other.Rgb)
            //{
            //    return true;
            //}
            return false;
        }

       
    }
}