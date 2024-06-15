using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static System.Net.Mime.MediaTypeNames;

namespace ImageDiff
{
    public class ImageDiff
    {
        //public string baselinePath;
        //public string comparisonPath;
        //public string resultPath;
        public bool displayingBaseline;
        public List<List<Pixel>> comparisonPixels = new List<List<Pixel>>();
        public List<List<Pixel>> baselinePixels = new List<List<Pixel>>();
        public int searchHeight = 50;
        public int searchWidth = 50;
        public int rowBuffer = 10;
        public int columnBuffer = 10;
        public int traversalBorder = 2;
        public Logger logger;
        public ImagePixels scanning;
        bool LoggingOn = false;
        string currentScannerImage = "";
        ComparisonLevel ComparisonLevel;

        
        public ImagePixels IdentifyMismatchBounds2(int rowStart, int columnStart)
        {
            List<Pixel> mismatched = new List<Pixel>() { comparisonPixels[rowStart][columnStart] };
            List<Pixel> ToReview = new List<Pixel>();
            List<Pixel> borderPixels = new List<Pixel>();
            for (int row = rowStart - traversalBorder; row < rowStart + traversalBorder + 1; row++)
            {
                if (row < 0)
                {
                    continue;
                }
                if (row >= comparisonPixels.Count)
                {
                    break;
                }
                for (int column = columnStart - traversalBorder; column < columnStart + traversalBorder + 1; column++)
                {
                    if (column < 0)
                    {
                        continue;
                    }
                    if (column >= comparisonPixels[row].Count)
                    {
                        break;
                    }
                    if (!ComparePixels(comparisonPixels[row][column], baselinePixels[row][column]))
                    {
                        ToReview.Add(comparisonPixels[row][column]);
                    }
                }
            }
            mismatched = NavigateMismatched(mismatched, ToReview, out borderPixels);
            Pixel borderPix = borderPixels[0];
            bool allBorderPixelsTheSame = false;
            Dictionary<string, int> counts = new Dictionary<string, int>();
            foreach (var pixel in borderPixels)
            {
                var key = $"{pixel.pixel.R}_{pixel.pixel.G}_{pixel.pixel.B}";
                if (counts.ContainsKey(key))
                {
                    counts[key]++;
                }
                else{
                    counts.Add(key, 1);
                }
            }
            var threshold = borderPixels.Count * 0.8;
            if (counts.Any(x => x.Value >= threshold))
            {
                var key = counts.First(x => x.Value >= threshold);
                var vals= key.Key.Split("_");
                borderPix.pixel.R = byte.Parse(vals[0]);
                borderPix.pixel.G = byte.Parse(vals[1]);
                borderPix.pixel.B = byte.Parse(vals[2]);
                allBorderPixelsTheSame = true;
            }
            var startRow = mismatched[0].Row;
            var endRow = mismatched[0].Row;
            var startColumn = mismatched[0].Column;
            var endColumn = mismatched[0].Column;
            var shouldNotHightlight = mismatched.Any(x => x.pixel.B == borderPix.pixel.B && x.pixel.R == borderPix.pixel.R && x.pixel.G == borderPix.pixel.G);
            if (shouldNotHightlight && allBorderPixelsTheSame)
            {
                var d = "";
            }
            foreach (var pixel in mismatched)
            {
                if (pixel.Row < startRow) startRow = pixel.Row;
                if (pixel.Row > endRow) endRow = pixel.Row;
                if (pixel.Column < startColumn) startColumn = pixel.Column;
                if (pixel.Column > endColumn) endColumn = pixel.Column;
            }
            var mismatchRegion = new ImagePixels();
            for (int row = startRow; row < endRow + 1; row++)
            {
                var baseRow = new Row();
                for (int col = startColumn; col < endColumn + +1; col++)
                {
                    if (allBorderPixelsTheSame)
                    {
                        if (comparisonPixels[row][col].IsMatch(borderPix, 20))
                        {
                            comparisonPixels[row][col].isBorderPixel = true;
                        }
                        else
                        {
                            var dd = "";
                        }
                    }
                    else { 
                        comparisonPixels[row][col].isBorderPixel = false; 
                    }
                    baseRow.Add(comparisonPixels[row][col]);//swapped row/col
                }
                mismatchRegion.AddRow(baseRow);
            }
            if (mismatchRegion.Rows.Count == 0 || mismatchRegion.Rows[0].Count == 0)
            {
                var s = "";
            }
            //LogScanner();
            //logger.SaveImageAsPng(mismatchRegion, $"MismatychAt{rowStart}-{columnStart}");
            return mismatchRegion;
        }
        public List<Pixel> NavigateMismatched(List<Pixel> mismatched, List<Pixel> toReview, out List<Pixel> borderPixels)
        {
            borderPixels = new List<Pixel>();
            List<Pixel> newForReview = new List<Pixel>();
            foreach (var underReview in toReview)
            {
                for (int row = underReview.Row - traversalBorder; row < underReview.Row + traversalBorder + 1; row++)
                {
                    if (row < 0)
                    {
                        continue;
                    }
                    if (row >= comparisonPixels.Count)
                    {
                        break;
                    }
                    for (int column = underReview.Column - traversalBorder; column < underReview.Column + traversalBorder + 1; column++)
                    {
                        if (column < 0)
                        {
                            continue;
                        }
                        if (column >= comparisonPixels[row].Count)
                        {
                            break;
                        }

                        //if (!mismatched.Any(X => X.Column == column && X.Row == row)) {
                        if (!comparisonPixels[row][column].processed)
                        {
                            comparisonPixels[row][column].processed = true;
                            if (!ComparePixels(comparisonPixels[row][column], baselinePixels[row][column]))
                            {
                                scanning.Rows[row].Pixels[column].pixel = Color.Magenta;
                                //LogScanner();
                                mismatched.Add(comparisonPixels[row][column]);
                                newForReview.Add(comparisonPixels[row][column]);
                            }
                            else
                            {
                                borderPixels.Add(scanning.Rows[row].Pixels[column]);
                                //scanning.Rows[row].Pixels[column].pixel = Color.Navy;
                                //LogScanner();
                            }
                        }
                    }
                }
            }
            if (newForReview.Count == 0)
            {
                return mismatched;
            }
            else
            {
                var moreBorderPixels = new List<Pixel>();
                var mismatch = NavigateMismatched(mismatched, newForReview, out moreBorderPixels);
                borderPixels.AddRange(moreBorderPixels);
                return mismatch;
            }
        }
        //public ImagePixels IdentifyMismatchBounds(int rowStart, int columnStart)
        //{
        //    //todo: need to be able to scan left also
        //    int width = 0;
        //    int height = 0;
        //    int heightMisMatches = 0;
        //    int sideMatches = 0;
        //    for (int row = rowStart; row < comparisonPixels.Count; row++)
        //    {
        //        bool doneHeight = false;
        //        if (row >= baselinePixels.Count)
        //        {
        //            //break;
        //            // comparisonPixels[row][column].pixel = Color.Magenta;                        
        //        }
        //        for (int column = columnStart; column < comparisonPixels[0].Count; column++)
        //        {
        //            if (baselinePixels[row].Count <= column)
        //            {
        //                //break;
        //            }
        //            scanning.Rows[row].Pixels[column].pixel = Color.Magenta;
        //            LogScanner();
        //            //var scanningProgress = System.Drawing.Image.FromFile($"{Application.StartupPath}\\Images\\Scanning.png");
        //            //pictureBox1.Image = scanningProgress;
        //            if (ComparePixels(comparisonPixels[row][column], baselinePixels[row][column]))
        //            {
        //                sideMatches++;                        
        //            }
        //            else
        //            {
        //                if (!doneHeight)
        //                {
        //                    heightMisMatches++;
        //                    doneHeight = true;
        //                }
        //                sideMatches = 0;
        //            }
        //            comparisonPixels[row][column].processed = true;
        //            if (column > width)
        //            {
        //                width = column;
        //            }
        //            if (sideMatches >= rowBuffer)
        //            {
        //                break;
        //            }
        //        }
        //        if ((row- rowStart)- heightMisMatches >= 10)
        //        {
        //            break;
        //        }
        //    }
        //    width = width - columnStart;
        //    height = heightMisMatches;
        //    //for (int column = columnStart; column < comparisonPixels[0].Count; column++)
        //    //{
        //    //    if (baselinePixels[0].Count <= column)
        //    //    {
        //    //        break;
        //    //    }
        //    //    int heightMatches = 0;
        //    //    for (int row = rowStart; row < comparisonPixels.Count; row++)
        //    //    {
        //    //        if (row >= baselinePixels.Count)
        //    //        {
        //    //            break;
        //    //        }


        //    //        if (ComparePixels(comparisonPixels[row][column], baselinePixels[row][column]))
        //    //        {
        //    //            heightMatches++;
        //    //        }
        //    //        else
        //    //        {
        //    //            heightMatches = 0;
        //    //        }
        //    //        comparisonPixels[row][column].processed = true;
        //    //        if (row > height)
        //    //        {
        //    //            height = row;
        //    //        }
        //    //        if (heightMatches == columnBuffer)
        //    //        {
        //    //            break;
        //    //        }
        //    //    }
        //    //}
        //    int count = 0;
        //    foreach(var row in comparisonPixels)
        //    {
        //        foreach (var pixel in row)
        //        {
        //            if (pixel.processed)
        //            {
        //                count++;
        //            }
        //        }
        //    }


        //    var mismatchRegion = new ImagePixels();
        //    for (int row = rowStart; row < height + rowStart; row++)
        //    {
        //        var baseRow = new Row();
        //        for (int col = columnStart; col < width + columnStart; col++)
        //        {
        //            baseRow.Add(baselinePixels[row][col]);//swapped row/col
        //        }
        //        mismatchRegion.AddRow(baseRow);
        //    }
        //    if (mismatchRegion.Rows.Count == 0 || mismatchRegion.Rows[0].Count == 0)
        //    {
        //        var s = "";
        //    }
        //    //logger.SaveImageAsPng(mismatchRegion, $"MismatychAt{rowStart}-{columnStart}");
        //    return mismatchRegion;
        //}

        //private void LogScanner(bool overrideLogging = false)
        //{
        //    if (!LoggingOn && !overrideLogging)
        //    {
        //        return;
        //    }
        //    scanningCHangedCount++;
        //    //if (scanningCHangedCount % 1 == 0)
        //    //{
        //    if (currentScannerImage == "Scanning")
        //    {
        //        pictureBox1.Image = null;
        //        logger.SaveImageAsBitmap(scanning, "Scanning2");
        //        pictureBox1.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes($"{Application.StartupPath}\\Images\\Scanning2.bmp")));
        //        //pictureBox1.Image = System.Drawing.Image.FromFile($"{Application.StartupPath}\\Images\\Scanning2.bmp");
        //        currentScannerImage = "Scanning2";
        //        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        //        pictureBox1.Refresh();
        //    }
        //    else
        //    {
        //        pictureBox1.Image = null;
        //        logger.SaveImageAsBitmap(scanning, "Scanning");
        //        pictureBox1.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes($"{Application.StartupPath}\\Images\\Scanning.bmp")));
        //        currentScannerImage = "Scanning";
        //        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        //        pictureBox1.Refresh();
        //    }
        //    //}
        //}

        public void ValidateMismatch(ImagePixels mismatch, int rowStart, int columnStart)
        {
            int baseStartColumn = columnStart - searchWidth;
            int baseEndColumn = columnStart + searchWidth + mismatch.Rows[0].Count;
            if (baseStartColumn < 0) baseStartColumn = 0;
            if (baseEndColumn >= baselinePixels[0].Count) baseEndColumn = baselinePixels[0].Count - 1;
            int baseRowStart = rowStart - searchHeight;
            int baseRowEnd = rowStart + searchHeight + mismatch.Rows.Count;
            if (baseRowStart < 0) baseRowStart = 0;
            if (baseRowEnd >= baselinePixels.Count) baseRowEnd = baselinePixels.Count - 1;
            //int width = mismatch.Rows[0].Count + searchWidth * 2;
            //int height = mismatch.Rows.Count + searchHeight;
            var baseRows = new ImagePixels();
            for (int row = baseRowStart; row < baseRowEnd; row++)
            {
                var baseRow = new Row();
                for (int col = baseStartColumn; col < baseEndColumn; col++)
                {
                    if (baselinePixels[row].Count <= col)
                    {
                        break;
                    }
                    baseRow.Add(baselinePixels[row][col]);
                }
                baseRows.AddRow(baseRow);
            }

            // var basePath = logger.SaveImageAsPng(baseRows, "BaseForCompare");
            // var mismatchPath = logger.SaveImageAsPng(mismatch, "MismatchForCompare");
            //for (int row = baseRowStart; row < baseRowEnd; row++)
            //{
            //    //try
            //    //{
            //        if (scanning.Rows.Count > row && scanning.Rows[row].Pixels.Count > baseEndColumn)
            //        {
            //            scanning.Rows[row].Pixels[baseStartColumn].pixel = Color.Red;
            //            scanning.Rows[row].Pixels[baseEndColumn].pixel = Color.Red;
            //        }

            //    //}
            //    //catch {
            //    //    var s = "";
            //    //}
            //}
            //for (int column = baseStartColumn; column < baseEndColumn; column++)
            //{
            //    //try
            //    //{
            //        if (scanning.Rows.Count > baseRowEnd && scanning.Rows[0].Pixels.Count > column)
            //        {
            //            scanning.Rows[baseRowStart].Pixels[column].pixel = Color.Red;
            //            scanning.Rows[baseRowEnd].Pixels[column].pixel = Color.Red;
            //        }

            //    //}
            //    //catch {
            //    //    var s = "";
            //    //}
            //}

            //LogScanner();
            bool hasMoved = baseRows.ContainsImage(mismatch);
            string mismatchText = "";
            string baseText = "";
            //if (hasMoved == true)
            //{
            //    var s = "";
            //}
            //else
            //{
            //    ImageTextReader reader = new ImageTextReader();
            //    var text = reader.ReadText(mismatchPath).Trim();
            //    if (!string.IsNullOrEmpty(text))
            //    {
            //        if (text.Contains("chief", StringComparison.CurrentCultureIgnoreCase))
            //        {
            //            var s = "";
            //        }
            //        baseText = reader.ReadText(basePath).Trim();

            //        if (baseText.Contains(text))
            //        {
            //            hasMoved = true;
            //        }
            //    }
            //}
            //using (var api = OcrApi.Create())
            //{
            //    api.Init(Languages.English);

            //    mismatchText = api.GetTextFromImage(mismatchPath);
            //    if (!string.IsNullOrEmpty(mismatchText))
            //    {
            //        baseText= api.GetTextFromImage(basePath);

            //        if (baseText.Contains(mismatchText)){
            //            hasMoved = true;
            //        }
            //    }
            //}


            var matchColour = mismatch.Rows[mismatch.Rows.Count - 1].Pixels[0];
            foreach (var row in mismatch.Rows)
            {
                foreach (var pixel in row.Pixels)
                {
                    if (comparisonPixels[pixel.Row][pixel.Column].isDifferent)
                    {
                        comparisonPixels[pixel.Row][pixel.Column].needsHighlight = !hasMoved;
                    }
                }
            }
        }

        public Image<Rgba32> DoCompare(byte[] newImage, byte[] baselineImage, out bool isDifferent)
        {
            try
            {
                comparisonPixels = new List<List<Pixel>>();
                baselinePixels = new List<List<Pixel>>();
                bool LoggingOn = false;
                currentScannerImage = "";

                bool different = false;
                SixLabors.ImageSharp.Image<Rgba32> comparisonImage = (SixLabors.ImageSharp.Image<Rgba32>)SixLabors.ImageSharp.Image<Rgba32>.Load<Rgba32>(newImage);
                SixLabors.ImageSharp.Image<Rgba32> baseline = (SixLabors.ImageSharp.Image<Rgba32>)SixLabors.ImageSharp.Image<Rgba32>.Load<Rgba32>(baselineImage);
                scanning = new ImagePixels();
                comparisonImage.ProcessPixelRows(comparisonAccessor =>
                {
                    for (int rowIndex = 0; rowIndex < comparisonAccessor.Height; rowIndex++)
                    {
                        List<Pixel> row = new List<Pixel>();
                        Span<Rgba32> pixelRow = comparisonAccessor.GetRowSpan(rowIndex);
                        scanning.AddRow(pixelRow);
                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int column = 0; column < pixelRow.Length; column++)
                        {
                            row.Add(new Pixel(pixelRow[column], rowIndex, column));
                        }
                        comparisonPixels.Add(row);
                    }
                });
                baseline.ProcessPixelRows(baselineAccessor =>
                {
                    for (int rowIndex = 0; rowIndex < baselineAccessor.Height; rowIndex++)
                    {
                        List<Pixel> row = new List<Pixel>();
                        Span<Rgba32> pixelRow = baselineAccessor.GetRowSpan(rowIndex);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int column = 0; column < pixelRow.Length; column++)
                        {
                            row.Add(new Pixel(pixelRow[column], rowIndex, column));
                        }
                        baselinePixels.Add(row);
                    }
                });

                //List<Point> confirmedMismatches = new List<Point>();
                //List<Point> processedPixels = new List<Point>();
                //List<Point> potentialMismatches = new List<Point>();


                //int rowMatchCount = 0;
                //int columnMatchCount = 0;
                //bool foundMismatchRegion = false;
                //int mismatchStartX;
                //int mismatchStartY;
                //int mismatchWidth;
                //int mismatchHeight;
                var analysisProgression = new ImagePixels();
                ImagePixels mismatchRegion = new ImagePixels();
                for (int row = 0; row < comparisonPixels.Count; row++)
                {
                    analysisProgression.Rows.Add(new Row());
                    for (int column = 0; column < comparisonPixels[0].Count; column++)
                    {
                        analysisProgression.Rows[row].Add(new Pixel(new Rgba32(255, 255, 255), row, column));
                        //logger.SaveImage(analysisProgression, $"progression{row}-{column}");
                        if (row >= baselinePixels.Count || baselinePixels[row].Count <= column)
                        {
                            // comparisonPixels[row][column].pixel = Color.Magenta;                        
                        }
                        else
                        {
                            if (ComparePixels(comparisonPixels[row][column], baselinePixels[row][column]))
                            {
                                //var pix = comparisonPixels[row][column];
                                //pix.pixel.A = 100;
                                //comparisonPixels[row][column] = pix;
                                //if(foundMismatchRegion && rowMatchCount == rowBuffer && columnMatchCount == columnBuffer)
                                //{
                                //    //found end of mismatch
                                //    foundMismatchRegion = false;
                                //}
                                //else
                                //{

                                //    rowMatchCount++;
                                //}
                            }
                            else
                            {
                                different = true;
                                //if (!foundMismatchRegion)
                                //{
                                //    potentialMismatches = new List<Point>();
                                //}
                                //else
                                //{
                                //    potentialMismatches.Add(new Point(x,y));
                                //}
                                if (!comparisonPixels[row][column].processed)
                                {
                                    var ImagePixels = IdentifyMismatchBounds2(row, column);
                                    ValidateMismatch(ImagePixels, ImagePixels.Rows[0].Pixels[0].Row, ImagePixels.Rows[0].Pixels[0].Column);
                                }
                                else
                                {
                                    var s = "";
                                }
                                //comparisonPixels[x][y].pixel = Color.Magenta;
                            }
                        }
                    }
                }

                //Image<Rgba32> targetImage = new Image<Rgba32>(baseline.Size().Width, baseline.Size().Height);
                //baseline.ProcessPixelRows(accessor =>
                //{
                //    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                //    Rgba32 transparent = Color.Transparent;

                //    for (int y = 0; y < accessor.Height; y++)
                //    {
                //        Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                //        // pixelRow.Length has the same value as accessor.Width,
                //        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                //        for (int x = 0; x < pixelRow.Length; x++)
                //        {
                //            if (ComparePixels(pixelRow[x], comparisonPixels[y][x]))
                //            {
                //                ref Rgba32 pixel = ref pixelRow[x];
                //                //if (pixel.A == 0)
                //                //{
                //                // Overwrite the pixel referenced by 'ref Rgba32 pixel':
                //                pixel.A = 0;
                //                //}
                //                //pixel = transparent;
                //            }
                //            else
                //            {
                //                different = true;
                //                ref Rgba32 pixel = ref pixelRow[x];
                //                //if (pixel.A == 0)
                //                //{
                //                // Overwrite the pixel referenced by 'ref Rgba32 pixel':
                //                pixel.R = 255;
                //            }
                //        }
                //    }
                //});
                isDifferent = different;
                comparisonImage.ProcessPixelRows(comparisonAccessor =>
                {
                    for (int rowIndex = 0; rowIndex < comparisonAccessor.Height; rowIndex++)
                    {
                        Span<Rgba32> pixelRow = comparisonAccessor.GetRowSpan(rowIndex);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int column = 0; column < pixelRow.Length; column++)
                        {
                            if (comparisonPixels[rowIndex][column].isBorderPixel)
                            {
                                var s = "";
                            }
                            if (comparisonPixels[rowIndex][column].needsHighlight && !comparisonPixels[rowIndex][column].isBorderPixel)
                            {
                                ref Rgba32 pixel = ref pixelRow[column];
                                pixel = Color.Magenta;
                            }
                            else
                            {
                                ref Rgba32 pixel = ref pixelRow[column];
                                pixel.A = 100;
                            }
                        }
                    }
                });
                return comparisonImage;
                //baseline.ProcessPixelRows(targetImage, (sourceAccessor, targetAccessor) =>
                //{
                //    for (int i = 0; i < baseline.Size().Height; i++)
                //    {
                //        int columIndex = 0;
                //        var span = sourceAccessor.GetRowSpan(i);
                //        foreach (var pixel in span)
                //        {
                //            comparisonImage[200, 200] = Rgba32.White;
                //            imageBytes.Add(pixel.B);
                //            imageBytes.Add(pixel.G);
                //            imageBytes.Add(pixel.R);
                //            columIndex++;
                //        }
                //    }
                //});
            }catch(Exception ex)
            {
                var s = "";
            }
            isDifferent = true;
            return null;
        }


        private bool ComparePixels(Pixel basepix, Pixel other)
        {
            if (basepix.pixel.Rgb == other.pixel.Rgb)
            {
                comparisonPixels[basepix.Row][basepix.Column].isDifferent = false;
                return true;
            }
            comparisonPixels[basepix.Row][basepix.Column].isDifferent = true;
            return false;
        }

        private bool CompareBorderPixels(Pixel basepix, Rgba32 other)
        {
            if (basepix.pixel.Rgb == other.Rgb)
            {
                return true;
            }
            return false;
        }
    }
}