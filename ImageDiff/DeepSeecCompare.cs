using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenCvSharp;
namespace DeepSeekCompare
{
    public class ChangeDetectionResult
    {
        public float DifferenceScore { get; set; }
        public List<ChangeRegion> Changes { get; set; } = new List<ChangeRegion>();
    }

    public class ChangeRegion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Type { get; set; } // "Movement" or "ContentChange"
        public int DisplacementX { get; set; }
        public int DisplacementY { get; set; }
    }

    public class ImageComparer
    {
        private Size GetCommonSize(Mat a, Mat b)
        {
            return new Size(
                Math.Min(a.Width, b.Width),
                Math.Min(a.Height, b.Height)
            );
        }
        public (byte[] ResultImage, string JsonResult) Compare(byte[] todayBytes, byte[] previousBytes)
        {
            using Mat todayOriginal = Mat.FromImageData(todayBytes, ImreadModes.Color);
            using Mat previousOriginal = Mat.FromImageData(previousBytes, ImreadModes.Color);

            // Ensure consistent dimensions
            Size targetSize = GetCommonSize(todayOriginal, previousOriginal);
            using Mat todayMat = todayOriginal.Resize(targetSize);
            using Mat previousMat = previousOriginal.Resize(targetSize);

            // Rest of processing remains the same
            using Mat todayGray = new Mat();
            using Mat previousGray = new Mat();
            Cv2.CvtColor(todayMat, todayGray, ColorConversionCodes.BGR2GRAY);
            Cv2.CvtColor(previousMat, previousGray, ColorConversionCodes.BGR2GRAY);
            Cv2.GaussianBlur(todayGray, todayGray, new Size(5, 5), 0);
            Cv2.GaussianBlur(previousGray, previousGray, new Size(5, 5), 0);
                      
            // Compute absolute difference
            using Mat diff = new Mat();
            Cv2.Absdiff(todayGray, previousGray, diff);

            // Thresholding
            using Mat thresh = new Mat();
            Cv2.Threshold(diff, thresh, 30, 255, ThresholdTypes.Binary);

            // Find contours
            Cv2.FindContours(thresh, out var contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            List<ChangeRegion> changes = new List<ChangeRegion>();
            using Mat overlay = todayMat.Clone();

            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);
                if (rect.Width < 10 || rect.Height < 10) continue;

                // Extract region from today's image
                using Mat template = new Mat(todayGray, rect);
                int searchMargin = 15;
                Rect searchRect = new Rect(
                    Math.Max(rect.X - searchMargin, 0),
                    Math.Max(rect.Y - searchMargin, 0),
                    Math.Min(rect.Width + 2 * searchMargin, previousGray.Width - Math.Max(rect.X - searchMargin, 0)),
                    Math.Min(rect.Height + 2 * searchMargin, previousGray.Height - Math.Max(rect.Y - searchMargin, 0))
                );

                if (searchRect.Width <= template.Width || searchRect.Height <= template.Height)
                    continue;

                using Mat searchArea = new Mat(previousGray, searchRect);
                using Mat result = new Mat();
                Cv2.MatchTemplate(searchArea, template, result, TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                if (maxVal > 0.75)
                {
                    Point matchLoc = new Point(maxLoc.X + searchRect.X, maxLoc.Y + searchRect.Y);
                    int dx = matchLoc.X - rect.X;
                    int dy = matchLoc.Y - rect.Y;

                    if (Math.Abs(dx) <= 5 && Math.Abs(dy) <= 5)
                        continue; // Ignore small movements

                    changes.Add(new ChangeRegion
                    {
                        X = rect.X,
                        Y = rect.Y,
                        Width = rect.Width,
                        Height = rect.Height,
                        Type = "Movement",
                        DisplacementX = dx,
                        DisplacementY = dy
                    });

                    Cv2.Rectangle(overlay, rect, new Scalar(255, 0, 0), 2); // Blue for movement
                }
                else
                {
                    changes.Add(new ChangeRegion
                    {
                        X = rect.X,
                        Y = rect.Y,
                        Width = rect.Width,
                        Height = rect.Height,
                        Type = "ContentChange",
                        DisplacementX = 0,
                        DisplacementY = 0
                    });

                    Cv2.Rectangle(overlay, rect, new Scalar(0, 0, 255), 2); // Red for content change
                }
            }

            // Calculate difference score
            float totalArea = todayMat.Width * todayMat.Height;
            float changedArea = changes.Sum(c => c.Width * c.Height);
            float differenceScore = changedArea / totalArea;

            // Encode overlay image
            Cv2.ImEncode(".png", overlay, out byte[] overlayBytes);

            // Serialize result
            var resultObj = new ChangeDetectionResult
            {
                DifferenceScore = differenceScore,
                Changes = changes
            };

            string json = JsonSerializer.Serialize(resultObj, new JsonSerializerOptions { WriteIndented = true });

            return (overlayBytes, json);
        }
    }
}