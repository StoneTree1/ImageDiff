using System;
using System.Collections.Generic;
//using System.Drawing.Imaging;
//using System.Drawing;
using System.Runtime.InteropServices;
using Tesseract;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using SixLabors.ImageSharp;

namespace ImageDiff.MultiPlatform
{
    public class ComparableImageV2
    {
        public List<LayoutBlock> AreasOfInterest;
        public byte[,] RawImageGreyScale;
        public Pixel[,] RawImage;
        private readonly CompareSettings Settings;

        public ComparableImageV2(CompareSettings settings, string imagePath)
        {
            Settings = settings;
            LoadFromFile(imagePath);
            AreasOfInterest = new List<LayoutBlock>();
        }

        public ComparableImageV2(CompareSettings settings, string imagePath, TesseractEngine engine)
        {
            Settings = settings;
            AreasOfInterest = new List<LayoutBlock>();
            LoadFromFile(imagePath, engine);
        }
        public ComparableImageV2(CompareSettings settings, byte[] imagePath, TesseractEngine engine)
        {
            Settings = settings;
            AreasOfInterest = new List<LayoutBlock>();
            LoadFromBytes(imagePath, engine);
        }
        public ComparableImageV2(CompareSettings settings, byte[] imagePath)
        {
            Settings = settings;
            AreasOfInterest = new List<LayoutBlock>();
            LoadFromBytes(imagePath);
        }
        //public ComparableImage(CompareSettings settings, Bitmap image, TesseractEngine engine)
        //{
        //    byte[] imageData;
        //    using (var stream = new MemoryStream())
        //    {
        //        image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //        imageData = stream.ToArray();
        //    }
        //    Settings = settings;
        //    AreasOfInterest = new List<LayoutBlock>();
        //    LoadFromBytes(imageData, engine);
        //    image.Dispose();
        //}
        public ComparableImageV2(CompareSettings settings)
        {
            AreasOfInterest = new List<LayoutBlock>();
            Settings = settings;
        }

        //public void LoadFromBitmap(Bitmap image, TesseractEngine engine)
        //{
        //    RawImage = ProcessPixels(image);
        //    DetectAreasOfInterestUsingTesseract(engine, image);
        //    PreProcessImageUsingAreasOfInterest();
        //}
        public void LoadFromBytes(byte[] fileName)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(new MemoryStream(fileName));
            RawImage = ProcessPixels(image);
            image.Dispose();
        }

        public void LoadFromBytes(byte[] fileName, TesseractEngine engine)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(new MemoryStream(fileName));
            RawImage = ProcessPixels(image);
            DetectAreasOfInterestUsingTesseract(engine, fileName);
            PreProcessImageUsingAreasOfInterest();
            image.Dispose();
        }

        public void LoadFromFile(string fileName)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(fileName);
            RawImage = ProcessPixels(image);
            PreProcessImage();
            image.Dispose();
        }
        public void LoadFromFile(string fileName, TesseractEngine engine)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(fileName);
            RawImage = ProcessPixels(image);
            //PreProcessImage();
            DetectAreasOfInterestUsingTesseract(engine, fileName);
            PreProcessImageUsingAreasOfInterest();
            image.Dispose();
        }
        public Image<Rgba32> FastCompareTo(ComparableImageV2 baselineImage, out bool hasDifferences)
        {
            WindowsPixel[,] outputImage;
            hasDifferences = false;
            var baselineWidth = baselineImage.RawImage.GetLength(1);
            var baselineHeight = baselineImage.RawImage.GetLength(0);
            var width = RawImage.GetLength(1);
            var height = RawImage.GetLength(0);
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
                                var pixel = RawImage[maxHeight + i, maxWidth + j];
                                var key = $"{pixel.pixel.R}_{pixel.pixel.G}_{pixel.pixel.B}";
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
                        Pixel backgroundColor = new Pixel(new Rgba32(255, 255, 255), -1, -1);

                        var threshold = Settings.BlockWidth * Settings.BlockHeight * 0.45;
                        if (pixelBlock.Any(x => x.Value >= threshold))
                        {
                            var key = pixelBlock.First(x => x.Value >= threshold);
                            var vals = key.Key.Split("_");
                            blockHasBackgroundColor = true;
                            backgroundColor = Pixel.FromArgb(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));
                        }

                        for (int i = 0; i < Settings.BlockHeight; i++)
                        {
                            for (int j = 0; j < Settings.BlockWidth; j++)
                            {
                                if (maxWidth + j >= width || maxHeight + i >= height)
                                {
                                    continue;
                                }

                                if (maxWidth + j >= baselineWidth || maxHeight + i >= baselineHeight || !RawImage[maxHeight + i, maxWidth + j].IsMatch(baselineImage.RawImage[maxHeight + i, maxWidth + j], 10))
                                {
                                    hasDifferences = true;
                                    RawImage[maxHeight + i, maxWidth + j].processed = true;
                                    RawImage[maxHeight + i, maxWidth + j].IsMoved = false;
                                    if (blockHasBackgroundColor && RawImage[maxHeight + i, maxWidth + j].IsMatch(backgroundColor, 10))
                                    {
                                        RawImage[maxHeight + i, maxWidth + j].needsHighlight = false;
                                    }
                                    else
                                    {
                                        RawImage[maxHeight + i, maxWidth + j].needsHighlight = true;
                                    }
                                }
                                else
                                {
                                    RawImage[maxHeight + i, maxWidth + j].processed = true;
                                    RawImage[maxHeight + i, maxWidth + j].IsMoved = false;
                                    RawImage[maxHeight + i, maxWidth + j].needsHighlight = false;
                                }
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var es = "";
                return null;
            }
            return GetImageFromDiff();
        }

        public Image<Rgba32> GetImageFromDiff()
        {
            int width = RawImage.GetLength(1);
            int height = RawImage.GetLength(0);
            Image<Rgba32> image;
            using (image = new(width, height))
            {

                // Do your drawing in here...
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        var currentPixel = RawImage[i, j];
                        if (RawImage[i, j].needsHighlight && RawImage[i, j].IsMoved)
                        {
                            //var col = Color.FromArgb(255, 0, RawImage[i, j].AsGrey(), 0);
                            image[j, i] = new Rgba32(0, RawImage[i, j].AsGrey(), 0);
                        }
                        else if (RawImage[i, j].IsMoved)
                        {
                            image[j, i] = new Rgba32(0, RawImage[i, j].AsGrey(), 0);
                        }
                        else if (RawImage[i, j].needsHighlight)
                        {
                            //var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), 0, 0);
                            image[j, i] = new Rgba32(RawImage[i, j].AsGrey(), 0, 0);
                        }
                        else if (RawImage[i, j].IsImagePixel)
                        {
                            //yellw shift?
                            //var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), RawImage[i, j].AsGrey(), 0);
                            image[j, i] = new Rgba32(RawImage[i, j].AsGrey(), RawImage[i, j].AsGrey(), 0);
                        }
                        else if (RawImage[i, j].IsBackgroundPixel)
                        {
                            image[j, i] = new Rgba32(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B);
                            //output.SetPixel(j, i, Color.FromArgb(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B));
                            //saveImage.SetPixel(j, i, Color.Black);
                        }
                        else if (RawImage[i, j].processed)
                        {
                            image[j, i] = new Rgba32(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B);
                            //output.SetPixel(j, i, Color.FromArgb(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B));
                            // saveImage.SetPixel(j, i, Color.Black);
                        }
                        else
                        {
                            image[j, i] = new Rgba32(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B);
                            //var col = Color.FromArgb(255, 0, 0, RawImage[i, j].Colour.AsGrey());
                            //output.SetPixel(j, i, Color.FromArgb(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B));
                        }
                        //output.SetPixel(j, i, Color.FromArgb(128, RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j]));
                    }
                }
            }

            return image;
        }

        //public DirectBitmap GetImageFromDiffPixals()
        //{
        //    //rebuild image for testing
        //    DirectBitmap output = new DirectBitmap(width, height);
        //    for (int i = 0; i < height; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //            var currentPixel = RawImage[i, j];
        //            if (RawImage[i, j].needsHighlight && RawImage[i, j].IsMoved)
        //            {
        //                var col = Color.FromArgb(255, 0, RawImage[i, j].AsGrey(), 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsMoved)
        //            {
        //                var col = Color.FromArgb(255, 0, RawImage[i, j].AsGrey(), 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].needsHighlight)
        //            {
        //                var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), 0, 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsImagePixel)
        //            {
        //                //yellw shift?
        //                var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), RawImage[i, j].AsGrey(), 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsBackgroundPixel)
        //            {
        //                output.SetPixel(j, i, Color.FromArgb(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B));
        //                //saveImage.SetPixel(j, i, Color.Black);
        //            }
        //            else if (RawImage[i, j].processed)
        //            {
        //                output.SetPixel(j, i, Color.FromArgb(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B));
        //                // saveImage.SetPixel(j, i, Color.Black);
        //            }
        //            else
        //            {
        //                //var col = Color.FromArgb(255, 0, 0, RawImage[i, j].Colour.AsGrey());
        //                output.SetPixel(j, i, Color.FromArgb(RawImage[i, j].pixel.R, RawImage[i, j].pixel.G, RawImage[i, j].pixel.B));
        //            }
        //            //output.SetPixel(j, i, Color.FromArgb(128, RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j]));
        //        }
        //    }
        //    return output;
        //}

        private Pixel[,] ProcessPixels(Image<Rgba32> image)
        {
            var pixels = new Pixel[image.Height, image.Width];
            RawImageGreyScale = new byte[image.Height, image.Width];

            image.ProcessPixelRows(accessor =>
            {
                // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                Rgba32 transparent = Color.Transparent;

                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        // Get a reference to the pixel at position x
                        ref Rgba32 pixel = ref pixelRow[x];
                        if (pixel.A == 0)
                        {
                            // Overwrite the pixel referenced by 'ref Rgba32 pixel':
                            pixel = transparent;
                            pixels[y, x] = new Pixel(pixel, y, x);
                        }
                    }
                }
            });
            return pixels;
        }

        private byte AsGrey(Rgba32 pixelColor)
        {
            return Convert.ToByte((pixelColor.R + pixelColor.G + pixelColor.B) / 3);
        }

        public void PreProcessImageUsingAreasOfInterest()
        {
            //Can be more specific with the areas later figure out if any should be ignored
            var maxWidth = RawImage.GetLength(1);
            var maxHeight = RawImage.GetLength(0);
            var imageAreas = AreasOfInterest.Where(x => x.BlockType == PolyBlockType.FlowingImage || x.BlockType == PolyBlockType.HeadingImage || x.BlockType == PolyBlockType.PullOutImage).ToList();
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
            foreach (var imageLayout in imageAreas)
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
            for (int width = layout.Bounds.X1; width < layout.Bounds.Width + layout.Bounds.X1; width++)
            {
                for (int height = layout.Bounds.Y1; height < layout.Bounds.Height + layout.Bounds.Y1; height++)
                {
                    if (layout.BlockType == PolyBlockType.FlowingImage
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
                            var key = $"{pixel.pixel.R}_{pixel.pixel.G}_{pixel.pixel.B}";
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
                    Pixel backgroundColor = Pixel.White();
                    var threshold = Settings.BlockWidth * Settings.BlockHeight * 0.45;
                    if (pixelBlock.Any(x => x.Value >= threshold))
                    {
                        var key = pixelBlock.First(x => x.Value >= threshold);
                        var vals = key.Key.Split("_");
                        blockHasBackgroundColor = true;
                        backgroundColor = Pixel.FromArgb(byte.Parse(vals[0]), byte.Parse(vals[1]), byte.Parse(vals[2]));
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

        //public DirectBitmap GetImage()
        //{
        //    int width = RawImage.GetLength(1);
        //    int height = RawImage.GetLength(0);
        //    //rebuild image for testing
        //    Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        //    DirectBitmap output = new DirectBitmap(width, height);
        //    for (int i = 0; i < height; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //            output.SetPixel(j, i, RawImage[i, j].GetColor());
        //        }
        //    }
        //    return output;
        //}


        //public DirectBitmap FastGetImageFromDiffPixals()
        //{
        //    int width = RawImage.GetLength(1);
        //    int height = RawImage.GetLength(0);
        //    //rebuild image for testing
        //    Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
        //    DirectBitmap output = new DirectBitmap(width, height);
        //    for (int i = 0; i < height; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //            var currentPixel = RawImage[i, j];
        //            if (RawImage[i, j].needsHighlight && RawImage[i, j].IsMoved)
        //            {
        //                var col = Color.FromArgb(255, 0, RawImage[i, j].AsGrey(), 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsMoved)
        //            {
        //                var col = Color.FromArgb(255, 0, RawImage[i, j].AsGrey(), 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].needsHighlight)
        //            {
        //                var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), 0, 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsImagePixel)
        //            {
        //                //yellw shift?
        //                var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), RawImage[i, j].AsGrey(), 0);
        //                output.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsBackgroundPixel)
        //            {
        //                output.SetPixel(j, i, RawImage[i, j].GetColor());
        //                //saveImage.SetPixel(j, i, Color.Black);
        //            }
        //            else if (RawImage[i, j].processed)
        //            {
        //                output.SetPixel(j, i, RawImage[i, j].GetColor());
        //                // saveImage.SetPixel(j, i, Color.Black);
        //            }
        //            else
        //            {
        //                //var col = Color.FromArgb(255, 0, 0, RawImage[i, j].AsGrey());
        //                output.SetPixel(j, i, RawImage[i, j].GetColor());
        //            }
        //            //output.SetPixel(j, i, Color.FromArgb(128, RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j]));
        //        }
        //    }
        //    return output;


        //}

        //public Bitmap GetImageFromDiffPixals()
        //{
        //    int width = RawImage.GetLength(1);
        //    int height = RawImage.GetLength(0);
        //    //rebuild image for testing
        //    Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        //    for (int i = 0; i < height; i++)
        //    {
        //        for (int j = 0; j < width; j++)
        //        {
        //            if (RawImage[i, j].needsHighlight && RawImage[i, j].IsMoved)
        //            {
        //                var col = Color.FromArgb(255, 0, RawImage[i, j].AsGrey(), 0);
        //                saveImage.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsMoved)
        //            {
        //                var col = Color.FromArgb(255, 0, RawImage[i, j].AsGrey(), 0);
        //                saveImage.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].needsHighlight)
        //            {
        //                var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), 0, 0);
        //                saveImage.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsImagePixel)
        //            {
        //                //yellw shift?
        //                var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), RawImage[i, j].AsGrey(), 0);
        //                saveImage.SetPixel(j, i, col);
        //            }
        //            else if (RawImage[i, j].IsBackgroundPixel)
        //            {
        //                var col = Color.FromArgb(255, RawImage[i, j].AsGrey(), RawImage[i, j].AsGrey(), RawImage[i, j].AsGrey());
        //                saveImage.SetPixel(j, i, col);
        //                //saveImage.SetPixel(j, i, Color.Black);
        //            }
        //            else if (RawImage[i, j].processed)
        //            {
        //                // saveImage.SetPixel(j, i, Color.Black);
        //            }
        //            else
        //            {
        //                var col = Color.FromArgb(255, 0, 0, RawImage[i, j].AsGrey());
        //                saveImage.SetPixel(j, i, col);
        //            }
        //        }
        //    }
        //    return saveImage;
        //}

        public Image<Rgba32> GetGreyScale(bool setTransperancy)
        {
            if (setTransperancy)
            {
                int width = RawImage.GetLength(1);
                int height = RawImage.GetLength(0);
                //rebuild image for testing
                Image<Rgba32> image;
                using (image = new(width, height))
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            image[j, i] = new Rgba32(RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j], 128);
                        }
                    }
                }
                return image;

            }
            else
            {
                int width = RawImage.GetLength(1);
                int height = RawImage.GetLength(0);
                //rebuild image for testing
                Image<Rgba32> image;
                using (image = new(width, height))
                {
                    for (int i = 0; i < height; i++)
                    {
                        for (int j = 0; j < width; j++)
                        {
                            image[j, i] = new Rgba32(RawImageGreyScale[i, j], RawImageGreyScale[i, j], RawImageGreyScale[i, j]);
                        }
                    }
                }
                return image;
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
                            var bounds = new Rect();
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
                using (var page = engine.Process(img, PageSegMode.SparseText))
                {
                    using (var iter = page.GetIterator())
                    {
                        iter.Begin();
                        do
                        {
                            var bounds = new Rect();
                            iter.TryGetBoundingBox(PageIteratorLevel.Block, out bounds);
                            var text = iter.GetText(PageIteratorLevel.Block);
                            if (string.IsNullOrEmpty(text))
                            {
                                text = "nontextarea";
                            }
                            AreasOfInterest.Add(new LayoutBlock() { Bounds = bounds, Text = text, BlockType = iter.BlockType });

                            //File.AppendAllText($"C:\\tmp\\{fileName.Replace(".png", ".txt").Replace(".jpeg", ".txt").Replace(".webp", ".txt")}", $"\n{text}");
                        } while (iter.Next(PageIteratorLevel.Block));
                    }
                }
            }
        }

        //Upgrade this to change isDifferent to a difference level or List of differences for better analysis processing??
        public Image<Rgba32> CompareTo(ComparableImageV2 baselineImage, out List<Difference> differences)
        {
            differences = new List<Difference>();
            //isDifferent = false;
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
                    var diffType = DifferenceType.Changed;
                    if (layout.BlockType == PolyBlockType.FlowingImage)
                    {
                        diffType = DifferenceType.ImageChanged;
                    }
                    differences.Add(new Difference() { Type = diffType, Area = new Rectangle(layout.Bounds.X1, layout.Bounds.Y1, layout.Bounds.Width, layout.Bounds.Height) });
                    //isDifferent = true;
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

                        differences.Add(new Difference() { Type = DifferenceType.Moved, Area = new Rectangle(layout.Bounds.X1, layout.Bounds.Y1, layout.Bounds.Width, layout.Bounds.Height) });

                    }
                    else
                    {
                        //matched at same coordinates
                    }
                }
            }
            return GetImageFromDiff();
        }

        public bool CompareBoundsWithOffset(LayoutBlock thisLocation, LayoutBlock otherLocation, ComparableImageV2 otherImage, Point offest, out double matchWeight)
        {
            int otherWidth = 0;
            int otherHeight = 0;
            try
            {
                //var offset = new Point(thisLocation.Bounds.X1 - otherLocation.Bounds.X1, thisLocation.Bounds.Y1 - otherLocation.Bounds.Y1);
                if (otherImage.RawImage == null)
                {
                    throw new Exception("OtherImage.RawImage was null");
                }

                otherWidth = otherImage.RawImage.GetLength(1);
                otherHeight = otherImage.RawImage.GetLength(0);
            }
            catch (Exception ex)
            {
                matchWeight = 0;
                otherWidth = otherImage.RawImage.GetLength(1);
                otherHeight = otherImage.RawImage.GetLength(0);
            }
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
                    if (i + offest.X + thisLocation.Bounds.X1 >= otherWidth) { break; }
                    if (i + offest.X + thisLocation.Bounds.X1 < 0) { continue; }
                    if (i + offest.X + otherLocation.Bounds.X1 >= otherWidth) { break; }
                    if (i + offest.X + otherLocation.Bounds.X1 < 0) { continue; }
                    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                    {
                        RawImage[j, i].processed = true;
                        var thisPixel = RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1];
                        if (j + otherLocation.Bounds.Y1 + offest.Y >= otherHeight) { break; }//break;??
                        if (j + thisLocation.Bounds.Y1 + offest.Y < 0) { continue; }//break;??
                        var checkHeight = j + otherLocation.Bounds.Y1 + offest.Y;
                        var checkWidth = i + otherLocation.Bounds.X1 + offest.X;
                        if (checkWidth < 0 || checkHeight < 0) continue;
                        checkedCount++;
                        //try
                        //{
                        var otherPixel = otherImage.RawImage[j + otherLocation.Bounds.Y1 + offest.Y, i + otherLocation.Bounds.X1 + offest.X];
                        if (thisPixel.IsMatch(otherPixel))
                        {
                            matchedCount++;
                        }
                        //}
                        //catch (Exception ecx)
                        //{
                        //    var s = "caught";
                        //}
                    }
                }
                matchWeight = (double)matchedCount / checkedCount;
                if (matchWeight > threshold)
                {
                    return true;
                }
                return false;
            }
            else
            {
                int matchedCount = 0;
                int greymatchedCount = 0;
                double threshold = 0.99;
                int checkedCount = 0;
                List<int> differences = new List<int>();
                for (int i = 0; i < thisLocation.Bounds.Width; i++)
                {
                    if (i + offest.X + thisLocation.Bounds.X1 >= otherWidth) { break; }
                    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                    {
                        if (RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1].IsBackgroundPixel) { continue; }
                        if (j + offest.Y + otherLocation.Bounds.Y1 >= otherHeight) { break; }//break;??
                        if (i + offest.X + otherLocation.Bounds.X1 >= otherWidth) { break; }//break;??
                        if (j + offest.Y + thisLocation.Bounds.Y1 >= RawImage.GetLength(0)) { break; }//break;??
                        var checkHeight = j + otherLocation.Bounds.Y1 + offest.Y;
                        var checkWidth = i + otherLocation.Bounds.X1 + offest.X;
                        if (checkWidth < 0 || checkHeight < 0) continue;
                        checkedCount++;
                        var thisPixel = RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1];
                        var otherPixel = otherImage.RawImage[j + otherLocation.Bounds.Y1 + offest.Y, i + otherLocation.Bounds.X1 + offest.X];
                        var thisGrey = thisPixel.AsGrey();
                        var otherGrey = otherPixel.AsGrey();
                        var diff = Math.Abs(thisGrey - otherGrey);
                        differences.Add(diff);
                        if (diff < 50)
                        {
                            greymatchedCount++;
                        }
                        if (thisPixel.IsMatch(otherPixel))
                        {
                            matchedCount++;
                        }
                        else
                        {
                            var m = "";
                        }
                    }
                }
                var matchWeightG = (double)greymatchedCount / checkedCount;
                if (offest.X == 0 && offest.Y == 0 || matchWeightG > 0.9)
                {
                    var d = "";
                }
                var max = 0;
                var min = 0;
                var average = 0.0;
                if (differences.Count > 0)
                {
                    max = differences.Max();
                    min = differences.Min();
                    average = differences.Average();
                }

                matchWeight = (double)matchedCount / checkedCount;
                if (matchWeight >= threshold)
                {
                    return true;
                }
                if (false)
                {
                    //DirectBitmap output = new DirectBitmap(thisLocation.Bounds.Width, thisLocation.Bounds.Height);
                    //DirectBitmap obaseleineOutput = new DirectBitmap(thisLocation.Bounds.Width, thisLocation.Bounds.Height);

                    //for (int i = 0; i < thisLocation.Bounds.Width; i++)
                    //{
                    //    if (i + offest.X + thisLocation.Bounds.X1 >= otherWidth) { break; }
                    //    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                    //    {
                    //        if (RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1].IsBackgroundPixel) { continue; }
                    //        if (j + offest.Y + thisLocation.Bounds.Y1 >= otherHeight) { break; }//break;??
                    //        checkedCount++;
                    //        var thisPixel = RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1];
                    //        var otherPixel = otherImage.RawImage[j + otherLocation.Bounds.Y1 + offest.Y, i + otherLocation.Bounds.X1 + offest.X];

                    //        output.SetPixel(i, j, thisPixel.GetColor());
                    //        obaseleineOutput.SetPixel(i, j, otherPixel.GetColor());
                    //    }
                    //}

                    //var text = thisLocation.Text.Replace("\n", "").Replace("\r", "");
                    //int length = 15;
                    //if (text.Length < 15)
                    //{
                    //    length = text.Length;
                    //}
                    //output.Bitmap.Save($"C:\\tmp\\layouts\\{offest.X}_{offest.Y}_nw_{text.Substring(0, length)}.png");
                    //obaseleineOutput.Bitmap.Save($"C:\\tmp\\layouts\\{offest.X}_{offest.Y}_bl_{text.Substring(0, length)}.png");
                    //obaseleineOutput.Dispose();
                    //output.Dispose();
                }
                return false;
            }
        }
        public bool CompareBounds(LayoutBlock thisLocation, LayoutBlock otherLocation, ComparableImageV2 otherImage)
        {
            //var offset = new Point(thisLocation.Bounds.X1 - otherLocation.Bounds.X1, thisLocation.Bounds.Y1 - otherLocation.Bounds.Y1);
            try
            {
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
                            if (i + otherLocation.Bounds.X1 >= otherWidth) { break; }//break;??
                            var checkHeight = j + otherLocation.Bounds.Y1;
                            var checkWidth = i + otherLocation.Bounds.X1;
                            if (checkWidth < 0 || checkHeight < 0) continue;
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
                        if (i + thisLocation.Bounds.X1 >= otherWidth)
                        {
                            break;
                        }
                        for (int j = 0; j < thisLocation.Bounds.Height; j++)
                        {
                            try
                            {
                                if (RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1].IsBackgroundPixel)
                                {
                                    continue;
                                }
                                if (j + thisLocation.Bounds.Y1 >= otherHeight)
                                {
                                    break;
                                }
                                if (i + otherLocation.Bounds.X1 >= otherWidth)
                                {
                                    break;
                                }
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
                            catch (Exception ex)
                            {
                                var indexError = "";
                            }
                        }
                    }
                    var result = (double)matchedCount / checkedCount;
                    if ((double)matchedCount / checkedCount >= threshold)
                    {
                        return true;
                    }
                    if (Debugger.IsAttached)
                    {
                        if (false)
                        {
                            //DirectBitmap output = new DirectBitmap(thisLocation.Bounds.Width, thisLocation.Bounds.Height);
                            //DirectBitmap obaseleineOutput = new DirectBitmap(thisLocation.Bounds.Width, thisLocation.Bounds.Height);

                            //for (int i = 0; i < thisLocation.Bounds.Width; i++)
                            //{
                            //    if (i + thisLocation.Bounds.X1 >= otherWidth)
                            //    {
                            //        break;
                            //    }
                            //    for (int j = 0; j < thisLocation.Bounds.Height; j++)
                            //    {
                            //        if (RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1].IsBackgroundPixel)
                            //        {
                            //            continue;
                            //        }
                            //        if (j + thisLocation.Bounds.Y1 >= otherHeight)
                            //        {
                            //            break;
                            //        }
                            //        if (i + otherLocation.Bounds.X1 >= otherWidth)
                            //        {
                            //            break;
                            //        }
                            //        checkedCount++;
                            //        var thisPixel = RawImage[j + thisLocation.Bounds.Y1, i + thisLocation.Bounds.X1];
                            //        var otherPixel = otherImage.RawImage[j + otherLocation.Bounds.Y1, i + otherLocation.Bounds.X1];

                            //        output.SetPixel(i, j, thisPixel.GetColor());
                            //        obaseleineOutput.SetPixel(i, j, otherPixel.GetColor());
                            //    }
                            //}
                            //var text = thisLocation.Text.Replace("\n", "").Replace("\r", "");
                            //text = ReplaceInvalidChars(text);
                            //int length = 15;
                            //if (text.Length < 15)
                            //{
                            //    length = text.Length;
                            //}
                            //output.Bitmap.Save($"C:\\tmp\\layouts\\nw_{text.Substring(0, length)}.png");
                            //obaseleineOutput.Bitmap.Save($"C:\\tmp\\layouts\\bl_{text.Substring(0, length)}.png");
                            //obaseleineOutput.Dispose();
                            //output.Dispose();
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                var ss = "Error with image";
                return false;
            }
        }

        public string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public List<BackgroundRegion> DetectBackgroundRegions()
        {
            int width = RawImage.GetLength(1);
            int height = RawImage.GetLength(0);
            //create object Region to store pixels and some metadata (width height)
            List<BackgroundRegion> regions = new List<BackgroundRegion>();
            //List<Pixel> region = new List<Pixel>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (RawImage[j, i].IsBackgroundPixel && !RawImage[j, i].processed)
                    {
                        var region = new BackgroundRegion();
                        region.ScanFrom(RawImage[j, i].Location, RawImage);
                        regions.Add(region);
                    }
                }
            }
            return regions;
            //for()
            //foreach row
            //foreach column
            //scan sourrunding pixels
            //foreach sourrunding pixel that is background and is acolour match
            //add to set
            //Set pixel as processed
            //foreach pixel found sourrounding,
            ////recurrsively scan their borders for unprocessed border pixels
        }


    }
}
