using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using Tesseract;
using ImageDiff;

namespace WebpageScreenshot
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using Newtonsoft.Json;
    using Emgu.CV;
    using Emgu.CV.CvEnum;
    using Emgu.CV.Structure;
    using Emgu.CV.Features2D;
    using Emgu.CV.Util;
    using Tesseract;

    public class WebpageScreenshotComparer
    {
        public static (byte[] diffImageBytes, string jsonResult) CompareScreenshots(byte[] image1Bytes, byte[] image2Bytes)
        {
            Mat img1 = ByteArrayToMat(image1Bytes);
            Mat img2 = ByteArrayToMat(image2Bytes);

            // Resize the smaller image to match the larger one
            ResizeImagesToMatch(ref img1, ref img2);

            // Convert to grayscale
            Mat gray1 = new Mat(), gray2 = new Mat();
            CvInvoke.CvtColor(img1, gray1, ColorConversion.Bgr2Gray);
            CvInvoke.CvtColor(img2, gray2, ColorConversion.Bgr2Gray);

            // Compute Structural Similarity Index (SSIM)
            Mat diffMap = new Mat();
            CvInvoke.AbsDiff(gray1, gray2, diffMap);
            CvInvoke.Threshold(diffMap, diffMap, 50, 255, ThresholdType.Binary);

            // Detect text changes using OCR
            List<TextChange> textDifferences = DetectTextChanges(image1Bytes, image2Bytes);

            // Detect layout/image movement using ORB feature matching
            List<MovementChange> movementDifferences = DetectFeatureMovements(img1, img2);

            // Highlight detected differences on the image
            Mat highlightedImage = HighlightDifferences(img1, diffMap);

            // Convert highlighted image to byte[]
            byte[] outputImageBytes = MatToByteArray(highlightedImage);

            // Calculate significance score
            double significanceScore = (textDifferences.Count + movementDifferences.Count) * 10;

            // Create result object
            ComparisonResult result = new ComparisonResult
            {
                SignificanceScore = significanceScore,
                TextChanges = textDifferences,
                MovementChanges = movementDifferences
            };

            // Convert result to JSON
            string jsonOutput = JsonConvert.SerializeObject(result, Formatting.Indented);

            return (outputImageBytes, jsonOutput);
        }

        private static void ResizeImagesToMatch(ref Mat img1, ref Mat img2)
        {
            if (img1.Size == img2.Size) return;

            int width = Math.Max(img1.Width, img2.Width);
            int height = Math.Max(img1.Height, img2.Height);

            Mat resizedImg1 = new Mat();
            Mat resizedImg2 = new Mat();

            CvInvoke.Resize(img1, resizedImg1, new Size(width, height), interpolation: Inter.Linear);
            CvInvoke.Resize(img2, resizedImg2, new Size(width, height), interpolation: Inter.Linear);

            img1 = resizedImg1;
            img2 = resizedImg2;
        }

        private static Mat HighlightDifferences(Mat originalImage, Mat diffMap)
        {
            Mat highlightedImage = originalImage.Clone();

            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                Mat hierarchy = new Mat();
                CvInvoke.FindContours(diffMap, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                for (int i = 0; i < contours.Size; i++)
                {
                    Rectangle boundingBox = CvInvoke.BoundingRectangle(contours[i]);
                    CvInvoke.Rectangle(highlightedImage, boundingBox, new MCvScalar(0, 0, 255), 2); // Red rectangle
                }
            }

            return highlightedImage;
        }

        private static List<TextChange> DetectTextChanges(byte[] imgBytes1, byte[] imgBytes2)
        {
            var textDiffs = new List<TextChange>();
            using var ocr = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var ocr2 = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);

            string text1 = ocr.Process(Pix.LoadFromMemory(imgBytes1)).GetText();
            string text2 = ocr2.Process(Pix.LoadFromMemory(imgBytes2)).GetText();

            string[] lines1 = text1.Split('\n');
            string[] lines2 = text2.Split('\n');

            int minLines = Math.Min(lines1.Length, lines2.Length);

            for (int i = 0; i < minLines; i++)
            {
                if (lines1[i] != lines2[i])
                {
                    textDiffs.Add(new TextChange
                    {
                        LineNumber = i,
                        OldText = lines1[i],
                        NewText = lines2[i]
                    });
                }
            }

            return textDiffs;
        }

        private static List<MovementChange> DetectFeatureMovements(Mat img1, Mat img2)
        {
            List<MovementChange> movements = new List<MovementChange>();

            var orb = new ORB(500); // 500 feature points
            VectorOfKeyPoint keyPoints1 = new VectorOfKeyPoint();
            VectorOfKeyPoint keyPoints2 = new VectorOfKeyPoint();
            Mat descriptors1 = new Mat(), descriptors2 = new Mat();

            orb.DetectAndCompute(img1, null, keyPoints1, descriptors1, false);
            orb.DetectAndCompute(img2, null, keyPoints2, descriptors2, false);

            BFMatcher matcher = new BFMatcher(DistanceType.Hamming);
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            matcher.KnnMatch(descriptors1, descriptors2, matches, 2);

            foreach (var match in matches.ToArrayOfArray())
            {
                if (match.Length == 2 && match[0].Distance < 0.75 * match[1].Distance)
                {
                    var kp1 = keyPoints1[match[0].QueryIdx].Point;
                    var kp2 = keyPoints2[match[0].TrainIdx].Point;

                    double distance = Math.Sqrt(Math.Pow(kp2.X - kp1.X, 2) + Math.Pow(kp2.Y - kp1.Y, 2));
                    if (distance > 5) // Consider as significant movement
                    {
                        movements.Add(new MovementChange
                        {
                            OldPosition = new Point((int)kp1.X, (int)kp1.Y),
                            NewPosition = new Point((int)kp2.X, (int)kp2.Y),
                            MovementDistance = distance
                        });
                    }
                }
            }

            return movements;
        }

        private static Mat ByteArrayToMat(byte[] imageBytes)
        {
            Mat mat = new Mat();
            CvInvoke.Imdecode(imageBytes, ImreadModes.Color, mat);
            return mat;
        }

        private static byte[] MatToByteArray(Mat mat)
        {
            using MemoryStream ms = new MemoryStream();
            CvInvoke.Imwrite(".jpg", mat, new KeyValuePair<ImwriteFlags, int>(ImwriteFlags.JpegQuality, 95));
            return File.ReadAllBytes(".jpg");
        }
    }

    // Output data models
    public class ComparisonResult
    {
        public double SignificanceScore { get; set; }
        public List<TextChange> TextChanges { get; set; }
        public List<MovementChange> MovementChanges { get; set; }
    }

    public class TextChange
    {
        public int LineNumber { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
    }

    public class MovementChange
    {
        public Point OldPosition { get; set; }
        public Point NewPosition { get; set; }
        public double MovementDistance { get; set; }
    }

}