using ImageDiff;

using SixLabors.ImageSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace ImageComparer;

public partial class Form1 : Form
{
    TesseractEngine engine;
    string baselineFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\BaselineImages";
    string comparisonFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\ComparisonImages";
    string resultsFolder = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\CompareResults";
    string imageA = "";
    string imageB = "";
    string imageC = "";
    bool displayImageA;
    public Form1()
    {
        engine = new TesseractEngine("C:\\tmp\\tessdata", "eng", EngineMode.Default);
        InitializeComponent();
        folderBrowserDialog1.InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        lblBaselineFoilder.Text = baselineFolder;
        lblComparisonFolder.Text = comparisonFolder;
        var imgFoem = new ImageProcessingForm();
        imgFoem.ShowDialog();
    }

    private void btnSelectBaselineFolder_Click(object sender, EventArgs e)
    {

        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
        {
            baselineFolder = folderBrowserDialog1.SelectedPath;
            lblBaselineFoilder.Text = baselineFolder;
        }
    }

    private void btnSelectComparisonFolder_Click(object sender, EventArgs e)
    {
        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
        {
            comparisonFolder = folderBrowserDialog1.SelectedPath;
            lblComparisonFolder.Text = comparisonFolder;
        }
    }

    private void btnPerformDiffs_Click(object sender, EventArgs e)
    {
        this.SuspendLayout();
        //DoCompares();
        DoCompareWindows();
        this.ResumeLayout();
    }

    private void DoCompareWindows()
    {
        CompareSettings settings = new CompareSettings();
        settings.BlockWidth = 30;
        settings.BlockHeight = 15;
        settings.SearchWidth = 60;
        settings.SearchHeight = 60;
        settings.PixelThreshold = 20;
        settings.ImageDetectionThreshold = 0.08;
        settings.CompareType = CompareType.DetectMovement;
        var Differ = new ImageDiffWindows(settings);

        var comparisonImage = File.ReadAllBytes($"C:\\tmp\\WordpressImaGerey.png");
        var baselineImage = File.ReadAllBytes($"C:\\tmp\\BaselineImageForCompareGreyScale.png");
        bool isDifferent = false;

        var datetime = DateTime.Now;
        Bitmap result = Differ.DoCompare(comparisonImage, baselineImage, out isDifferent);
        result.Save($"C:\\tmp\\CompareResult.jpg");

        var endTime = DateTime.Now;
        FormLogger.Instance.Log($"Image NewImageForCompare took {(endTime - datetime).TotalSeconds} seconds to process\n");

        // Refresh();
        cmbImagesForReview.SelectedIndex = 0;
    }

    //Todo: add threading support to speed things up
    private void DoCompares()
    {
        var comparer = new ImageDiff.ImageDiff();
        comparer.searchWidth = 75;
        comparer.searchHeight = 75;
        comparer.traversalBorder = 1;
        var comparisonImages = Directory.GetFiles(comparisonFolder);
        var baselineImages = Directory.GetFiles(baselineFolder);
        foreach (var file in baselineImages)
        {
            var datetime = DateTime.Now;
            var fileName = Path.GetFileName(file);
            if (comparisonImages.Any(x => x.Contains(fileName)))
            {
                var comparisonImage = File.ReadAllBytes($"{comparisonFolder}\\{Path.GetFileName(fileName)}");
                var baselineImage = File.ReadAllBytes($"{baselineFolder}\\{Path.GetFileName(fileName)}");
                bool isDifferent;
                var result = comparer.DoCompare(comparisonImage, baselineImage, out isDifferent);
                result.SaveAsPng($"{resultsFolder}\\{Path.GetFileNameWithoutExtension(file)}.png");
            }
            var endTime = DateTime.Now;
            FormLogger.Instance.Log($"Image {fileName} took {(endTime - datetime).TotalSeconds} seconds to process\n");
        }
        Refresh();
        cmbImagesForReview.SelectedIndex = 0;
    }

    private void btnRefresh_Click(object sender, EventArgs e)
    {
        Refresh();
    }

    private void Refresh()
    {
        cmbImagesForReview.Items.Clear();
        foreach (var file in Directory.GetFiles(resultsFolder))
        {
            cmbImagesForReview.Items.Add(Path.GetFileName(file));
        }
    }

    private void cmbImagesForReview_SelectedIndexChanged(object sender, EventArgs e)
    {
        var comparisonImages = Directory.GetFiles(comparisonFolder);
        var baselineImages = Directory.GetFiles(baselineFolder);
        var resultImages = Directory.GetFiles(resultsFolder);
        var selectedFile = cmbImagesForReview.Text;
        var baseline = baselineImages.First(x => x.Contains(selectedFile));
        var comparison = comparisonImages.First(x => x.Contains(selectedFile));
        var result = resultImages.First(x => x.Contains(selectedFile));
        pbCompareResult.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes(result)));
        pbCompareResult.SizeMode = PictureBoxSizeMode.Zoom;
        imageA = baseline;
        imageB = comparison;
        imageC = result;
        timer1.Interval = 1000;
        displayImageA = true;
        timer1.Start();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        timer1.Interval = 500;
        if (displayImageA)
        {
            pbAlternaitingComparison.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes(imageA)));
            pbAlternaitingComparison.SizeMode = PictureBoxSizeMode.Zoom;
        }
        else
        {
            pbAlternaitingComparison.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes(imageB)));
            pbAlternaitingComparison.SizeMode = PictureBoxSizeMode.Zoom;
        }
        displayImageA = !displayImageA;
    }

    private int[,] GetBytes(BitmapData image)
    {
        int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(image.PixelFormat) / 8;
        int stride = image.Stride;

        byte[] buffer = new byte[image.Height * stride];
      

        Marshal.Copy(image.Scan0, buffer, 0, buffer.Length);
        var pixels = new int[image.Height, image.Width];
        for (int i = 0; i < image.Height; i++)
        {
            for (int widthIndex = 0; widthIndex < image.Width; widthIndex++)
            {
                int rowOffset = i * stride;
                var byteIndex = (bytesPerPixel * widthIndex) + rowOffset;
                var asGreyscale = (int)buffer[byteIndex + 2]+ (int) buffer[byteIndex + 1]+ (int)buffer[byteIndex];
                var bytepix = Convert.ToByte(asGreyscale);
                //var pixelColor = System.Drawing.Color.FromArgb(buffer[byteIndex + 2], buffer[byteIndex + 1], buffer[byteIndex]);
                pixels[i, widthIndex] = (int)asGreyscale/3;
            }
        }
        return pixels;
    }

    private void button3_Click(object sender, EventArgs e)
    {
        var  imgFoem = new ImageProcessingForm();
        imgFoem.ShowDialog();
        //var testImage = "C:\\tmp\\WordpressImage.png";
        //var baselineImage = "C:\\tmp\\BaselineImageForCompare.jpg";

        //Bitmap tempimage = new Bitmap(System.Drawing.Image.FromFile(testImage));
        //Bitmap tempBaseline = new Bitmap(System.Drawing.Image.FromFile(baselineImage));
        //BitmapData newImageByteData = tempimage.LockBits(new System.Drawing.Rectangle(0, 0, tempimage.Width, tempimage.Height), ImageLockMode.ReadOnly, tempimage.PixelFormat);
        //BitmapData baselineByteData = tempBaseline.LockBits(new System.Drawing.Rectangle(0, 0, tempBaseline.Width, tempBaseline.Height), ImageLockMode.ReadOnly, tempBaseline.PixelFormat);
        //var newImageBytes = GetBytes(newImageByteData);
        //var baselineImageBytes = GetBytes(baselineByteData);
        //CompareSequence seq = new CompareSequence();
        //var res = seq.CompareImages(newImageBytes, baselineImageBytes);

    }

    private void button2_Click(object sender, EventArgs e)
    {
        // ClaudeImageComparer comp = new ClaudeImageComparer();

        var testImage = "C:\\tmp\\WordpressImage.png";
        var baselineImage = "C:\\tmp\\BaselineImageForCompare.jpg";
        ClaudeImageComparer.CompareImages(baselineImage, testImage);
        Bitmap tempimage = new Bitmap(System.Drawing.Image.FromFile(testImage));
        Bitmap tempBaseline = new Bitmap(System.Drawing.Image.FromFile(baselineImage));
        BitmapData newImageBytes = tempimage.LockBits(new System.Drawing.Rectangle(0, 0, tempimage.Width, tempimage.Height), ImageLockMode.ReadOnly, tempimage.PixelFormat);

        var comparisonPixels = ProcessPixels(newImageBytes);

        int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(newImageBytes.PixelFormat) / 8;
        int stride = newImageBytes.Stride;
        byte[] buffer = new byte[newImageBytes.Height * stride];
        Marshal.Copy(newImageBytes.Scan0, buffer, 0, buffer.Length);
        tempimage.UnlockBits(newImageBytes);



        CompareSettings settings = new CompareSettings();
        settings.BlockWidth = 30;
        settings.BlockHeight = 15;
        settings.SearchWidth = 60;
        settings.SearchHeight = 60;
        settings.PixelThreshold = 80;
        settings.ImageDetectionThreshold = 0.08;
        settings.CompareType = CompareType.DetectMovement;
        var Differ = new ImageDiffWindows(settings);

        var outputImage = Differ.PreProcessImage(comparisonPixels, new System.Drawing.Size(settings.BlockHeight, settings.BlockHeight));
        var ppi = GetImageFromPixals(outputImage, true);
        var fileName = $"C:\\tmp\\NewImageForCompareHC.jpg";
        ppi.Save(fileName);
        pbAlternaitingComparison.Image = System.Drawing.Image.FromStream(new MemoryStream(File.ReadAllBytes(fileName)));
        pbAlternaitingComparison.SizeMode = PictureBoxSizeMode.Zoom;
    }

    public static Bitmap AdjustContrast(Bitmap Image, float Value)
    {
        Value = (100.0f + Value) / 100.0f;
        Value *= Value;
        Bitmap NewBitmap = (Bitmap)Image.Clone();
        BitmapData data = NewBitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, NewBitmap.Width, NewBitmap.Height),
            ImageLockMode.ReadWrite,
            NewBitmap.PixelFormat);
        int Height = NewBitmap.Height;
        int Width = NewBitmap.Width;

        unsafe
        {
            for (int y = 0; y < Height; ++y)
            {
                byte* row = (byte*)data.Scan0 + (y * data.Stride);
                int columnOffset = 0;
                for (int x = 0; x < Width; ++x)
                {
                    byte B = row[columnOffset];
                    byte G = row[columnOffset + 1];
                    byte R = row[columnOffset + 2];

                    float Red = R / 255.0f;
                    float Green = G / 255.0f;
                    float Blue = B / 255.0f;
                    Red = (((Red - 0.5f) * Value) + 0.5f) * 255.0f;
                    Green = (((Green - 0.5f) * Value) + 0.5f) * 255.0f;
                    Blue = (((Blue - 0.5f) * Value) + 0.5f) * 255.0f;

                    int iR = (int)Red;
                    iR = iR > 255 ? 255 : iR;
                    iR = iR < 0 ? 0 : iR;
                    int iG = (int)Green;
                    iG = iG > 255 ? 255 : iG;
                    iG = iG < 0 ? 0 : iG;
                    int iB = (int)Blue;
                    iB = iB > 255 ? 255 : iB;
                    iB = iB < 0 ? 0 : iB;

                    row[columnOffset] = (byte)iB;
                    row[columnOffset + 1] = (byte)iG;
                    row[columnOffset + 2] = (byte)iR;

                    columnOffset += 4;
                }
            }
        }

        NewBitmap.UnlockBits(data);

        return NewBitmap;
    }
    private void button1_Click(object sender, EventArgs e)
    {
        var testImage = "C:\\tmp\\NewImageForCompare.jpg";
        var testImageHC = "C:\\tmp\\NewImageForCompareHC.jpg";
        var baselineImage = "C:\\tmp\\BaselineImageForCompare.jpg";
        var baselineImageHC = "C:\\tmp\\BaselineImageForCompareHC.jpg";
        //Bitmap tempimage = new Bitmap(System.Drawing.Image.FromFile(baselineImage));
        //Bitmap tempBaseline = new Bitmap(System.Drawing.Image.FromFile(testImage));
        //tempimage = AdjustContrast(tempimage, 50);
        //tempimage.Save(testImageHC);
        //tempBaseline = AdjustContrast(tempBaseline, 50);
        //tempBaseline.Save(baselineImageHC);
        //Bitmap tempBaselineHC = new Bitmap(System.Drawing.Image.FromFile(testImageHC));
        //BitmapData newImageBytes = tempimage.LockBits(new System.Drawing.Rectangle(0, 0, tempimage.Width, tempimage.Height), ImageLockMode.ReadOnly, tempimage.PixelFormat);
        //BitmapData newImageBytesHC = tempBaselineHC.LockBits(new System.Drawing.Rectangle(0, 0, tempBaselineHC.Width, tempBaselineHC.Height), ImageLockMode.ReadOnly, tempBaselineHC.PixelFormat);
        //int bytesPerPixel = System.Drawing.Image.GetPixelFormatSize(newImageBytes.PixelFormat) / 8;
        //int stride = newImageBytes.Stride;
        //byte[] buffer = new byte[newImageBytes.Height * stride];
        //Marshal.Copy(newImageBytes.Scan0, buffer, 0, buffer.Length);
        //tempimage.UnlockBits(newImageBytes);

        //int bytesPerPixelHC = System.Drawing.Image.GetPixelFormatSize(newImageBytesHC.PixelFormat) / 8;
        //int stride2 = newImageBytesHC.Stride;
        //byte[] buffer2 = new byte[newImageBytes.Height * stride];
        //Marshal.Copy(newImageBytes.Scan0, buffer2, 0, buffer2.Length);
        //var comparisonPixels = ProcessPixels(newImageBytes);
        //tempBaselineHC.UnlockBits(newImageBytesHC);


        CompareSettings settings = new CompareSettings();
        settings.BlockWidth = 30;
        settings.BlockHeight = 15;
        settings.SearchWidth = 60;
        settings.SearchHeight = 60;
        settings.PixelThreshold = 20;
        settings.ImageDetectionThreshold = 0.08;
        settings.CompareType = CompareType.DetectMovement;
        //var Differ = new ImageDiffWindows(settings);

        //var outputImage = Differ.PreProcessImage(comparisonPixels, new System.Drawing.Size(settings.BlockHeight, settings.BlockHeight));
        //var ppi = GetImageFromPixals(outputImage, true);
        //var fileName = $"TestImage_JPEG_HighCompression.jpeg";
        //var fileName = $"TestImage_JPEG_LowCOmpression.jpeg";
        var fileName = $"GreySCale.png";
        // var fileName = $"TestImagePNGLowCompression.png";
        //ppi.Save(fileName);
        List<LayoutBlock> baselineBlocks = new List<LayoutBlock>();
        List<LayoutBlock> newImageBlocks = new List<LayoutBlock>();

        //Differ.AddBaseline();
        //Differ.AddImage();

        List<Rect> rects = new List<Rect>();
        List<Rect> ImageRects = new List<Rect>();

        var basel = new DiffImage(settings, baselineImage);
        basel.DetectAreasOfInterestUsingTesseract(engine, baselineImage);

        var img = new DiffImage(settings, testImage);
        img.DetectAreasOfInterestUsingTesseract(engine, testImage);

        img.CompareTo(basel);
        //using (var engine = new TesseractEngine("C:\\tmp\\tessdata", "eng", EngineMode.Default))
        //{
        

            //using (var img = Pix.LoadFromFile(baselineImageHC))
            //{
            //    using (var page = engine.Process(img))
            //    {
            //        using (var iter = page.GetIterator())
            //        {
            //            iter.Begin();
            //            do
            //            {
            //                var bounds = new Tesseract.Rect();
            //                iter.TryGetBoundingBox(PageIteratorLevel.Block, out bounds);
            //                var text = iter.GetText(PageIteratorLevel.Block);
            //                baselineBlocks.Add(new LayoutBlock() { Bounds = bounds, Text = text, BlockType = iter.BlockType });
            //                //Log(t);
            //            } while (iter.Next(PageIteratorLevel.Block));
            //        }
            //    }
            //}
        //
        //}
        var baselineCount = baselineBlocks.Count;
        var newImageCount = newImageBlocks.Count;

        foreach (var block in baselineBlocks)
        {
            if (!newImageBlocks.Exists(x => x.Text == block.Text))
            {
                var s = "size mismatch?";
            }
        }

        // var locations = Differ.ProcessForText(testImage);
        Bitmap textImage = new Bitmap(System.Drawing.Image.FromFile($"C:\\tmp\\{fileName}"));
        using (var graphics = Graphics.FromImage(textImage))
        {
            using (System.Drawing.Font arialFont = new System.Drawing.Font("Arial", 10))
            {
                Pen redPen = new Pen(System.Drawing.Color.Red, 2);
                int i = 0;
                foreach (var block in newImageBlocks)
                {
                    graphics.DrawString($"Instace: {i++}", arialFont, Brushes.Red, new System.Drawing.PointF(block.Bounds.X1, block.Bounds.Y1));
                    graphics.DrawRectangle(redPen, new System.Drawing.Rectangle(block.Bounds.X1, block.Bounds.Y1, block.Bounds.Width, block.Bounds.Height));
                }
            }
        }
        textImage.Save($"C:\\tmp\\TextDetection_{fileName}");

    }


    private void Log(string Message)
    {
        File.AppendAllText("C:\\tmp\\TextLogging.txt", $"\n{Message}");
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
                //var pixelColor = System.Drawing.Color.FromArgb(buffer[byteIndex + 2], buffer[byteIndex + 1], buffer[byteIndex]);
                pixels[i, widthIndex] = new WindowsPixel(buffer[byteIndex + 2], buffer[byteIndex + 1], buffer[byteIndex], i, widthIndex);
            }
        }
        return pixels;
    }

    private Bitmap GetImageFromPixals(WindowsPixel[,] pixels, bool forTextDetection)
    {
        int width = pixels.GetLength(1);
        int height = pixels.GetLength(0);
        //rebuild image for testing
        Bitmap saveImage = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (pixels[i, j].needsHighlight && pixels[i, j].IsMoved)
                {
                    saveImage.SetPixel(j, i, System.Drawing.Color.Green);
                }
                else if (pixels[i, j].IsImagePixel)
                {
                    saveImage.SetPixel(j, i, System.Drawing.Color.Yellow);
                }
                else if (pixels[i, j].IsBackgroundPixel)
                {
                    saveImage.SetPixel(j, i, System.Drawing.Color.Black);
                }
                else if (pixels[i, j].needsHighlight)
                {
                    saveImage.SetPixel(j, i, System.Drawing.Color.Magenta);
                }
                else if (pixels[i, j].processed)
                {
                    saveImage.SetPixel(j, i, System.Drawing.Color.Black);
                }
                else
                {
                    if (forTextDetection)
                    {
                        saveImage.SetPixel(j, i, System.Drawing.Color.White);
                    }
                    else
                    {
                        saveImage.SetPixel(j, i, pixels[i, j].Colour.ToColor());
                    }
                }
            }
        }
        return saveImage;
    }

}
