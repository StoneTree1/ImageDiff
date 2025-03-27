using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using ImageDiff;
using ImageDiffConsole;
//using ImageMagick;
using Newtonsoft.Json;
using Tesseract;
using Windows.Devices.PointOfService.Provider;
using Windows.Foundation.Diagnostics;
using Windows.System.UserProfile;
using CompareSettings = ImageDiff.CompareSettings;
using ImageDiff.MultiPlatform;
using WebpageScreenshot;
using DeepSeekCompare;

namespace ImageComparer
{
    public partial class ImageProcessingForm : Form
    {
        DirectBitmap bmpNewPixel;
        DirectBitmap bmpBaselinePixel;
        //DirectBitmap output;
        DiffImage image;
        DiffImage Baseline;
        string image1 = "";
        string image2 = "";
        TesseractEngine engine;
        CompareSettings settings;
        public ImageProcessingForm()
        {
            settings = new CompareSettings();
            settings.BlockWidth = 30;
            settings.BlockHeight = 15;
            settings.SearchWidth = 60;
            settings.SearchHeight = 60;
            settings.PixelThreshold = 80;
            settings.ImageDetectionThreshold = 0.08;
            settings.CompareType = CompareType.DetectMovement;

            InitializeComponent();
            pictureBox2.BackColor = Color.Transparent;
            engine = new TesseractEngine("C:\\tmp\\tessdata", "eng", EngineMode.Default);

            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var start = DateTime.Now;
                lblImage1.Text = openFileDialog1.FileName;
                image1 = openFileDialog1.FileName;
                image = new ImageDiff.DiffImage(settings, image1, engine);
                var duration = DateTime.Now - start;
                lblDuration.Text = $"Load Image took {duration.TotalMilliseconds} milliseconds";
                var img = image.GetImageFromDiffPixals();
                pictureBox1.Height = img.Height;
                pictureBox1.Width = img.Width;
                pictureBox1.Image = img.Bitmap;
                // pictureBox1.Location = new Point(5, 5);

                // pictureBox2.Parent = pictureBox1;
                // pictureBox2.BackColor = Color.Transparent;

                //img.Dispose();
            }
        }

        private void btnSelectBaseline_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var start = DateTime.Now;
                image2 = openFileDialog1.FileName;
                Baseline = new ImageDiff.DiffImage(settings, image2, engine);
                var duration = DateTime.Now - start;
                lblDuration.Text = $"Load Image took {duration.TotalMilliseconds} milliseconds";
                var img = Baseline.GetImageFromDiffPixals();
                pictureBox2.Height = img.Height;
                pictureBox2.Width = img.Width;
                pictureBox2.Image = img.Bitmap;

                // img.Dispose();
                // pictureBox2.Location = new Point(800, 5);
                //pictureBox2.Parent = pictureBox1;
                //pictureBox2.BackColor = Color.Transparent;
            }
        }

        private void btnShowAsGreyScale_Click(object sender, EventArgs e)
        {
            var img = image.GetGreyScale(false);
            pictureBox1.Image = img.Bitmap;
            img.Bitmap.Save("C:\\tmp\\GreyScale.png");
            img.Dispose();
        }

        private void btnTestTransparency_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                lblImage1.Text = openFileDialog1.FileName;
                image1 = openFileDialog1.FileName;
                image = new ImageDiff.DiffImage(settings, image1);
                var img = image.GetGreyScale(true);
                img.Bitmap.MakeTransparent();
                pictureBox1.Image = img.Bitmap;
                pictureBox2.Image = img.Bitmap;
                img.Dispose();
                //pictureBox2.Parent = pictureBox1;
                //pictureBox2.BackColor = Color.Transparent;
                // pictureBox1.Location = new Point(12, 87);
                // pictureBox2.Location = new Point(0, 0);
            }
        }

        private void btnViewSubtraction_Click(object sender, EventArgs e)
        {
            var offset = txtOffset.Text.Split(',');

            var result = image.GetSubtractionImage((Bitmap)Bitmap.FromFile(image2), new Point(int.Parse(offset[0]), int.Parse(offset[1])));
            pictureBox1.Image = result;
            pictureBox1.BackColor = Color.White;
            //pictureBox2.Parent = pictureBox1;
            // pictureBox2.BackColor = Color.Transparent;
            //pictureBox2.Location = new Point(0, 0);
        }
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

        private void btnDoCompare_Click(object sender, EventArgs e)
        {

            var comparisonImage = File.ReadAllBytes(image1);
            var baselineImage = File.ReadAllBytes(image2);
            bool isDifferent = false;

            var compy = new DeepSeekCompare.ImageComparer();
            var (diffImage1, jsonResult1) = compy.Compare(comparisonImage, baselineImage);
            File.WriteAllText("differences1.json", JsonConvert.SerializeObject(jsonResult1, Formatting.Indented));
            File.WriteAllBytes("outputImage1.jpg", diffImage1);



            WebpageScreenshotComparer comp = new WebpageScreenshotComparer();
            var (diffImage, jsonResult) = WebpageScreenshotComparer.CompareScreenshots(comparisonImage, baselineImage);// comp.CompareScreenshots(comparisonImage, baselineImage);

            // Save highlighted differences as a JPEG

            // Save JSON output
            File.WriteAllText("differences.json", JsonConvert.SerializeObject(jsonResult, Formatting.Indented));
            File.WriteAllBytes("outputImage.json", diffImage);


            this.SuspendLayout();
            var newImage = new DiffImage(settings, image1, engine);
            var baseleinImage = new DiffImage(settings, image2, engine);
            List<Difference> differences = new List<Difference>();
            var resultImage = newImage.CompareTo(baseleinImage, out differences);
            resultImage.Save("C:\\tmp\\WindowsResult.png");
           

            //int width = image.RawImage.GetLength(1);
            //int height = image.RawImage.GetLength(0);

            //int blwidth = Baseline.RawImage.GetLength(1);
            //int blheight = Baseline.RawImage.GetLength(0);

            //if (blwidth != width)
            //{
            //    //var iuumg = ScaleImageToWidth(image.GetImage(), blwidth);
            //    //image = new ImageDiff.DiffImage(settings, iuumg, engine);
            //    //var img = image.GetImageFromDiffPixals();
            //    //pictureBox1.Image = img;
            //}

            //CompareTo(image, Baseline);

            this.ResumeLayout();

            //var start = DateTime.Now;

            //List<Difference> differences;
            //var resultImage = image.CompareTo(Baseline, out differences);
            //var duration = DateTime.Now - start;
            //lblDuration.Text = $"Compare took {duration.TotalMilliseconds} milliseconds";
            //resultImage.Save("C:\\tmp\\CompareResult_AreasOfInterest.jpg");
            //pictureBox1.Image = resultImage;
        }

        public void CompareTo(DiffImage newImage, DiffImage baselineImage)
        {
            var offsets = new List<Point>();
            foreach (var layout in newImage.AreasOfInterest)
            {
                var otherLayout = layout.FindClosestMatch(baselineImage.AreasOfInterest);
                offsets.Add(new Point(otherLayout.Bounds.X1 - layout.Bounds.X1, otherLayout.Bounds.Y1 - layout.Bounds.Y1));
            }
            var query = offsets
           .GroupBy(p => p)
           .Select(group => new { Point = group.Key, Count = group.Count() })//172 //227
           .OrderByDescending(x => x.Count);

            // Get the most common Point
            var mostCommonPoint = query.First().Point;

            foreach (var layout in newImage.AreasOfInterest)
            {
                var otherLayout = layout.FindClosestMatch(baselineImage.AreasOfInterest);

                //var newimg = newImage.GetImageFromDiffPixals();

                //using (var graphics = Graphics.FromImage(newimg.Bitmap))
                //{
                //    using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                //    {
                //        Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                //        graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(layout.Bounds.X1, layout.Bounds.Y1, layout.Bounds.Width, layout.Bounds.Height));

                //    }
                //}
                //pictureBox1.Image.Dispose();
                //pictureBox1.Image = newimg.Bitmap;

                //var basel = baselineImage.GetImageFromDiffPixals();

                //using (var graphics = Graphics.FromImage(basel.Bitmap))
                //{
                //    using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                //    {
                //        Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                //        graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(otherLayout.Bounds.X1, otherLayout.Bounds.Y1, otherLayout.Bounds.Width, otherLayout.Bounds.Height));

                //    }
                //}
                //pictureBox2.Image.Dispose();
                //pictureBox2.Image = basel.Bitmap;
                //this.Refresh();

                var matched = newImage.CompareBounds(layout, otherLayout, ref baselineImage);
                //determine if want to compare from a different offset
                //bool checkDifferentOffset = true;
                if (!matched)
                {

                }
                //else
                //{
                //bool matchedWithOffset = false;
                if ((layout.BlockType == PolyBlockType.CaptionText || layout.BlockType == PolyBlockType.FlowingText) && !matched)
                {
                    List<double> matchWeights = new List<double>();
                    //+/-?
                    int count = 1;
                    //TODO: Fix this to only use the outer pixels at each level

                    Queue<Point> startingLocations = new Queue<Point>();

                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            //TODO: add safety checks for oput of bounds
                            //if (j == 0 && i == 0) continue;
                            startingLocations.Enqueue(new Point(otherLayout.Bounds.X1 + i - 5, otherLayout.Bounds.Y1 + j - 5));
                            //startingLocations.Enqueue(new Point(otherLayout.Bounds.X1 + i, otherLayout.Bounds.Y1 - j));
                            //startingLocations.Enqueue(new Point(otherLayout.Bounds.X1 - i, otherLayout.Bounds.Y1 - j));
                            //startingLocations.Enqueue(new Point(otherLayout.Bounds.X1 - i, otherLayout.Bounds.Y1 + j));
                        }
                    }
                    while (startingLocations.Count > 0)
                    {
                        var locationToCheck = startingLocations.Dequeue();
                        //basel.Dispose();
                        //basel = baselineImage.GetImageFromDiffPixals();

                        //using (var graphics = Graphics.FromImage(basel.Bitmap))
                        //{
                        //    using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                        //    {
                        //        Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                        //        graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(locationToCheck.X, locationToCheck.Y, otherLayout.Bounds.Width, otherLayout.Bounds.Height));
                        //    }
                        //}
                        //pictureBox2.Image.Dispose();
                        //pictureBox2.Image = basel.Bitmap;

                        //this.Refresh();

                        var matchWeight = 0.0;
                        //var offset = new Point(locationToCheck.X - otherLayout.Bounds.X1, locationToCheck.Y - otherLayout.Bounds.Y1);
                        var offset = new Point(otherLayout.Bounds.X1- locationToCheck.X, otherLayout.Bounds.Y1- locationToCheck.Y);
                        matched = newImage.CompareBoundsWithOffset(layout, otherLayout, ref baselineImage, offset, out matchWeight);

                        matchWeights.Add(matchWeight);
                        if (matched)
                        {
                            break;
                        }
                    }
                    //while (count < 10 && !matched)
                    //{
                    //for (int i = 0 - count; i < count + 1; i++)
                    //{
                    //    for (int j = count - 1; j < count + 1; j++)
                    //    {
                    //        if (j == 0 && i == 0) continue;
                    //        var matchWeight = 0.0;
                    //        basel = baselineImage.GetImageFromDiffPixals();
                    //        using (var graphics = Graphics.FromImage(basel))
                    //        {
                    //            using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                    //            {
                    //                Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                    //                graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(otherLayout.Bounds.X1 + i, otherLayout.Bounds.Y1 + j, otherLayout.Bounds.Width, otherLayout.Bounds.Height));
                    //            }
                    //        }
                    //        pictureBox2.Image = basel;
                    //        this.Refresh();
                    //        matched = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(i, j), out matchWeight);

                    //        matchWeights.Add(matchWeight);
                    //        if (matched)
                    //        {
                    //            break;
                    //        }
                    //    }
                    //    if (matched)
                    //    {
                    //        break;
                    //    }
                    //}
                    //    count++;
                    //    //var newmatched = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(0, count));
                    //    //var newmatchedcount = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(count, 0));
                    //    //var newmatched2 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(count, count));
                    //    //var newmatched3 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(0, -count));
                    //    //var newmatched4 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(-count, 0));
                    //    //var newmatched5 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(-count, -count));
                    //}
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
                            newImage.RawImage[j + layout.Bounds.Y1, i + layout.Bounds.X1].needsHighlight = true;
                        }
                    }
                }
                else
                {
                    bool xmatcxhes = layout.Bounds.X1 == otherLayout.Bounds.X1;
                    bool ymatcxhes = layout.Bounds.Y1 == otherLayout.Bounds.Y1;
                    var offset = new Point(layout.Bounds.X1 - otherLayout.Bounds.X1, layout.Bounds.Y1 - otherLayout.Bounds.Y1);


                    if (offset == Point.Empty && xmatcxhes && ymatcxhes)
                    {
                        var d = "";
                    }
                    if (xmatcxhes && ymatcxhes)
                    {
                        var d = "";
                    }
                    else
                    {
                        var s = "";
                        //moved
                    }
                    if (offset != Point.Empty)
                    {
                        for (int i = 0; i < layout.Bounds.Width; i++)
                        {
                            for (int j = 0; j < layout.Bounds.Height; j++)
                            {
                                newImage.RawImage[j + layout.Bounds.Y1, i + layout.Bounds.X1].IsMoved = true;
                            }
                        }
                    }
                    else
                    {
                        //matched at same corodinates
                    }
                    // }
                }
                //newimg.Dispose();
                //newimg = newImage.GetImageFromDiffPixals();
                //basel.Dispose();
                //basel = baselineImage.GetImageFromDiffPixals();

                //pictureBox1.Image.Dispose();
                //pictureBox2.Image.Dispose();
                //pictureBox1.Image = newimg.Bitmap;
                //pictureBox2.Image = basel.Bitmap;


                //this.Refresh();
                //newimg.Dispose();
                //basel.Dispose();
            }
        }

        private void btnEdgeDetection_Click(object sender, EventArgs e)
        {
            int width = image.RawImage.GetLength(1);
            int height = image.RawImage.GetLength(0);
            //image = new ImageDiff.DiffImage(settings, image1);
            var regions = image.DetectBackgroundRegions();
            Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            DirectBitmap output = new DirectBitmap(width, height);
            int lastcolour = 0;
            foreach (var region in regions)
            {
                var col = new Random().Next(255);
                if (Math.Abs(col - lastcolour) < 50)
                {
                    col = 255 - col;
                    if (120 < col && col < 170)
                    {
                        col = col - 100;
                    }
                }
                foreach (var pixel in region.members)
                {
                    output.SetPixel(pixel.X, pixel.Y, Color.FromArgb(255, col, col, col));

                }
            }

            pictureBox1.Image = output.Bitmap;
            output.Dispose();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (image == null) return;
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            int width = 50;
            int height = 50;
            if (bmpNewPixel != null)
            {
                bmpNewPixel.Dispose();
                pbNewImagePreview.Image.Dispose();
            }
            bmpNewPixel = new DirectBitmap(width, height);
            var color = image.RawImage[coordinates.Y, coordinates.X].Colour.ToColor();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bmpNewPixel.SetPixel(j, i, color);
                }
            }
            pbNewImagePreview.Image = bmpNewPixel.Bitmap;
            lblNewImageB.Text = "B: " + color.B;
            lblNewImageR.Text = "R: " + color.R;
            lblNewImageG.Text = "G: " + color.G;
            txtNewImageCoordinate.Text = $"{coordinates.X},{coordinates.Y}";
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (Baseline == null) return;
            MouseEventArgs me = (MouseEventArgs)e;
            Point coordinates = me.Location;
            int width = 50;
            int height = 50;
            if (bmpBaselinePixel != null)
            {
                bmpBaselinePixel.Dispose();
                pbNewImagePreview.Image.Dispose();
            }
            bmpBaselinePixel = new DirectBitmap(width, height);
            var color = Baseline.RawImage[coordinates.Y, coordinates.X].Colour.ToColor();
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    bmpBaselinePixel.SetPixel(j, i, color);
                }
            }
            pbBaseleinePreview.Image = bmpBaselinePixel.Bitmap;
            lblBaseLineB.Text = "B: " + color.B;
            lblBaseLineR.Text = "R: " + color.R;
            lblBaseLineG.Text = "G: " + color.G;
            txtBaselineImageCoordinate.Text = $"{coordinates.X},{coordinates.Y}";
        }

        private void txtNewImageCoordinate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                int width = 50;
                int height = 50;

                var coords = txtNewImageCoordinate.Text.Split(',');

                var X = int.Parse(coords[0]);
                var Y = int.Parse(coords[1]);
                if (bmpNewPixel != null)
                {
                    bmpNewPixel.Dispose();
                    pbNewImagePreview.Image.Dispose();
                }
                bmpNewPixel = new DirectBitmap(width, height);
                var color = image.RawImage[Y, X].Colour.ToColor();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        bmpNewPixel.SetPixel(j, i, color);
                    }
                }
                pbNewImagePreview.Image = bmpNewPixel.Bitmap;
                lblNewImageB.Text = "B: " + color.B;
                lblNewImageR.Text = "R: " + color.R;
                lblNewImageG.Text = "G: " + color.G;
                e.Handled = true;
            }
        }

        private void txtBaselineImageCoordinate_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                int width = 50;
                int height = 50;

                var coords = txtBaselineImageCoordinate.Text.Split(',');

                var X = int.Parse(coords[0]);
                var Y = int.Parse(coords[1]);
                if (bmpBaselinePixel != null)
                {
                    bmpBaselinePixel.Dispose();
                    pbBaseleinePreview.Image.Dispose();
                }
                bmpBaselinePixel = new DirectBitmap(width, height);
                var color = Baseline.RawImage[Y, X].Colour.ToColor();
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        bmpBaselinePixel.SetPixel(j, i, color);
                    }
                }
                pbBaseleinePreview.Image = bmpBaselinePixel.Bitmap;
                lblBaseLineB.Text = "B: " + color.B;
                lblBaseLineR.Text = "R: " + color.R;
                lblBaseLineG.Text = "G: " + color.G;
                e.Handled = true;
            }
        }

        private void txtNewImageCoordinate_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            var startCommand = $"ImageDiffConsole.exe";

            Process p = new Process();
            var proc1 = new ProcessStartInfo();
            proc1.UseShellExecute = false;
            proc1.RedirectStandardOutput = true;
            //proc1.WorkingDirectory = @"C:\Windows\System32";
            //proc1.FileName = @"C:\Dev\repos\ImageDiff\ImageDiffConsole\bin\Debug\net8.0\ImageDiffConsole.exe ";

            proc1.WorkingDirectory = "C:\\Dev\\ImageProcessing\\9d1bb736-811b-4b42-b90f-85ce9e440e1b";
            proc1.FileName = $"C:\\Dev\\ImageProcessing\\9d1bb736-811b-4b42-b90f-85ce9e440e1b\\ImageDiffConsole.exe";

            // proc1.FileName = @"ImageDiffConsole.exe";
            proc1.Arguments = $"newImage=C:\\QATools\\ImgDiff\\6d3c07e4-cd2e-4840-ae46-41099fe60c1b.png " +
                $"baseline=C:\\QATools\\ImgDiff\\df4e972b-3b58-450f-8047-3db73ccdea1d.png " +
                $"tesseractPath=C:\\Dev\\ImageProcessing\\9d1bb736-811b-4b42-b90f-85ce9e440e1b\\tessdata " +
                $"OutputPath=C:\\Dev\\ImageProcessing\\9d1bb736-811b-4b42-b90f-85ce9e440e1b " +
                //$"tesseractPath=C:\\tmp\\tessdata " +
                $"SearchHeight=60 SearchWidth=60 PixelThreshold=50 BlockWidth=30 BlockHeight=15 ImageDetectionThreshold=80";
            // var envs = proc1.Environment;
            p.StartInfo = proc1;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            var lines = JsonConvert.DeserializeObject<CompareResult>(output);
        }

        private void btnCompareWithComparableImage_Click(object sender, EventArgs e)
        {
            //this.SuspendLayout();
            //var newImage = new ComparableImageV2(settings, image1, engine);
            //var baseleinImage = new ComparableImageV2(settings, image2, engine);
            //List<ImageDiff.MultiPlatform.Difference> differences = new List<ImageDiff.MultiPlatform.Difference>();
            //var resultImage = newImage.CompareTo(baseleinImage, out differences);
            //var stream = new MemoryStream();
            //resultImage.Save(stream, resultImage.Metadata.DecodedImageFormat);
            //File.WriteAllBytes("C:\\tmp\\MultiPlatformResult.png", stream.ToArray());
            //this.ResumeLayout();

        }
    }
}
