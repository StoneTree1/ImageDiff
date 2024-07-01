using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageDiff;
using Tesseract;

namespace ImageComparer
{
    public partial class ImageProcessingForm : Form
    {
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

                var img = image.GetImageFromDiffPixals();
                //pictureBox1.Height = img.Height;
                pictureBox1.Image = img;
                // pictureBox1.Location = new Point(5, 5);

                // pictureBox2.Parent = pictureBox1;
                // pictureBox2.BackColor = Color.Transparent;
                var duration = DateTime.Now - start;
                lblDuration.Text = $"Load Image took {duration.TotalMilliseconds} milliseconds";
            }
        }

        private void btnSelectBaseline_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var start = DateTime.Now;
                image2 = openFileDialog1.FileName;
                Baseline = new ImageDiff.DiffImage(settings, image2, engine);
                var img = Baseline.GetImageFromDiffPixals();
                // pictureBox2.Height = img.Height/2;
                pictureBox2.Image = img;
                // pictureBox2.Location = new Point(800, 5);
                //pictureBox2.Parent = pictureBox1;
                //pictureBox2.BackColor = Color.Transparent;
                var duration = DateTime.Now - start;
                lblDuration.Text = $"Load Image took {duration.TotalMilliseconds} milliseconds";
            }
        }

        private void btnShowAsGreyScale_Click(object sender, EventArgs e)
        {
            var img = image.GetGreyScale(false);
            pictureBox1.Image = img;
            img.Save("C:\\tmp\\GreySCale.png");
        }

        private void btnTestTransparency_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                lblImage1.Text = openFileDialog1.FileName;
                image1 = openFileDialog1.FileName;
                image = new ImageDiff.DiffImage(settings, image1);
                var img = image.GetGreyScale(true);
                img.MakeTransparent();
                pictureBox1.Image = img;
                pictureBox2.Image = img;
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

        private void btnDoCompare_Click(object sender, EventArgs e)
        {
            //this.SuspendLayout();
            //CompareTo(image, Baseline);

            //this.ResumeLayout();

            var start = DateTime.Now;
            var resultImage = image.CompareTo(Baseline);
            var duration = DateTime.Now - start;
            lblDuration.Text = $"Compare took {duration.TotalMilliseconds} milliseconds";
            resultImage.Save("C:\\tmp\\CompareResult_AreasOfInterest.jpg");
            pictureBox1.Image = resultImage;
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
           .Select(group => new { Point = group.Key, Count = group.Count() })
           .OrderByDescending(x => x.Count);

            // Get the most common Point
            var mostCommonPoint = query.First().Point;

            foreach (var layout in newImage.AreasOfInterest)
            {
                var otherLayout = layout.FindClosestMatch(baselineImage.AreasOfInterest);

                var matched = newImage.CompareBounds(layout, otherLayout, baselineImage);
                var newimg = newImage.FastGetImageFromDiffPixals();
                var basel = baselineImage.FastGetImageFromDiffPixals();

                using (var graphics = Graphics.FromImage(newimg))
                {
                    using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                    {
                        Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                        graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(layout.Bounds.X1, layout.Bounds.Y1, layout.Bounds.Width, layout.Bounds.Height));

                    }
                }
                pictureBox1.Image = newimg;
                using (var graphics = Graphics.FromImage(basel))
                {
                    using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                    {
                        Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                        graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(otherLayout.Bounds.X1, otherLayout.Bounds.Y1, otherLayout.Bounds.Width, otherLayout.Bounds.Height));

                    }
                }
                pictureBox2.Image = basel;
                this.Refresh();
                //determine if want to compare from a different offset
                bool checkDifferentOffset = true;
                if (checkDifferentOffset)
                {

                }
                else
                {
                    //bool matchedWithOffset = false;
                    if ((layout.BlockType == PolyBlockType.CaptionText || layout.BlockType == PolyBlockType.FlowingText) && !matched)
                    {
                        List<double> matchWeights = new List<double>();
                        //+/-?
                        int count = 1;
                        while (count < 50 && !matched)
                        {
                            for (int i = 0 - count; i < count + 1; i++)
                            {
                                for (int j = count - 1; j < count + 1; j++)
                                {

                                    if (j == 0 && i == 0) continue;
                                    var matchWeight = 0.0;
                                    //using (var graphics = Graphics.FromImage(basel))
                                    //{
                                    //    using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
                                    //    {
                                    //        Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                                    //        graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(otherLayout.Bounds.X1, otherLayout.Bounds.Y1, otherLayout.Bounds.Width, otherLayout.Bounds.Height));

                                    //    }
                                    //}
                                    //pictureBox2.Image = basel;
                                    matched = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(i, j), out matchWeight);

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
                            //var newmatched = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(0, count));
                            //var newmatchedcount = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(count, 0));
                            //var newmatched2 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(count, count));
                            //var newmatched3 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(0, -count));
                            //var newmatched4 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(-count, 0));
                            //var newmatched5 = newImage.CompareBoundsWithOffset(layout, otherLayout, baselineImage, new Point(-count, -count));
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
                    }
                }

                newimg = newImage.FastGetImageFromDiffPixals();
                basel = baselineImage.FastGetImageFromDiffPixals();
                pictureBox1.Image = newimg;
                pictureBox2.Image = basel;
                this.Refresh();
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
            foreach (var region in regions)
            {
                var col = new Random().Next(255);
                foreach (var pixel in region.region)
                {
                    output.SetPixel(pixel.Column, pixel.Row, Color.FromArgb(255, col, col, col));

                }
            }

            pictureBox1.Image = output.Bitmap;

        }
    }
}
