using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class ClaudeImageComparer
{
    public static void CompareImages(string oldImagePath, string newImagePath)
    {
        using (Bitmap originalOldImage = new Bitmap(oldImagePath))
        using (Bitmap originalNewImage = new Bitmap(newImagePath))
        {
            int width = Math.Min(originalOldImage.Width, originalNewImage.Width);
            int height = Math.Min(originalOldImage.Height, originalNewImage.Height);

            using (Bitmap oldImage = ResizeImage(originalOldImage, width, height))
            using (Bitmap newImage = ResizeImage(originalNewImage, width, height))
            {
                int changedPixels = 0;
                int movedPixels = 0;

                Bitmap diffImage = new Bitmap(width, height);

                BitmapData oldData = oldImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData newData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                int bytesPerPixel = 4;
                int stride = width * bytesPerPixel;
                byte[] oldPixels = new byte[stride * height];
                byte[] newPixels = new byte[stride * height];
                byte[] diffPixels = new byte[stride * height];

                Marshal.Copy(oldData.Scan0, oldPixels, 0, oldPixels.Length);
                Marshal.Copy(newData.Scan0, newPixels, 0, newPixels.Length);

                List<Rectangle> rois = DetermineRegionsOfInterest(oldPixels, newPixels, width, height);

                foreach (Rectangle roi in rois)
                {
                    for (int y = roi.Top; y < roi.Bottom; y++)
                    {
                        int rowOffset = y * stride;
                        for (int x = roi.Left; x < roi.Right; x++)
                        {
                            int i = rowOffset + x * bytesPerPixel;

                            if (oldPixels[i] != newPixels[i] || oldPixels[i + 1] != newPixels[i + 1] || oldPixels[i + 2] != newPixels[i + 2])
                            {
                                changedPixels++;

                                if (FindMovedPixel(oldPixels, newPixels, oldPixels.AsSpan(i, bytesPerPixel), x, y, width, height))
                                {
                                    movedPixels++;
                                    // Green for moved pixels
                                    diffPixels[i] = 0;       // Red
                                    diffPixels[i + 1] = 255; // Green
                                    diffPixels[i + 2] = 0;   // Blue
                                }
                                else
                                {
                                    // Magenta for changed pixels
                                    diffPixels[i] = 255;     // Red
                                    diffPixels[i + 1] = 0;   // Green
                                    diffPixels[i + 2] = 255; // Blue
                                }
                                diffPixels[i + 3] = 255; // Alpha
                            }
                            else
                            {
                                // Keep original color for unchanged pixels
                                diffPixels[i] = oldPixels[i];
                                diffPixels[i + 1] = oldPixels[i + 1];
                                diffPixels[i + 2] = oldPixels[i + 2];
                                diffPixels[i + 3] = oldPixels[i + 3];
                            }
                        }
                    }
                }

                Marshal.Copy(diffPixels, 0, diffData.Scan0, diffPixels.Length);

                oldImage.UnlockBits(oldData);
                newImage.UnlockBits(newData);
                diffImage.UnlockBits(diffData);

                diffImage.Save("C:\\tmp\\diff_image.png", ImageFormat.Png);

                Console.WriteLine($"Original old image size: {originalOldImage.Width}x{originalOldImage.Height}");
                Console.WriteLine($"Original new image size: {originalNewImage.Width}x{originalNewImage.Height}");
                Console.WriteLine($"Comparison size: {width}x{height}");
                Console.WriteLine($"Total pixels: {width * height}");
                Console.WriteLine($"Changed pixels: {changedPixels}");
                Console.WriteLine($"Moved pixels: {movedPixels}");
                Console.WriteLine($"Percentage changed: {(double)changedPixels / (width * height) * 100:F2}%");
            }
        }
    }
    private static Bitmap ResizeImage(Bitmap image, int width, int height)
    {
        if (image.Width == width && image.Height == height)
        {
            return new Bitmap(image);
        }

        Bitmap resized = new Bitmap(width, height);
        using (Graphics g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(image, 0, 0, width, height);
        }
        return resized;
    }

    private static List<Rectangle> DetermineRegionsOfInterest(byte[] oldPixels, byte[] newPixels, int width, int height)
    {
        const int blockSize = 32; // Size of each block to check
        List<Rectangle> rois = new List<Rectangle>();
        int bytesPerPixel = 4;
        int stride = width * bytesPerPixel;

        for (int y = 0; y < height; y += blockSize)
        {
            for (int x = 0; x < width; x += blockSize)
            {
                bool different = false;
                int blockHeight = Math.Min(blockSize, height - y);
                int blockWidth = Math.Min(blockSize, width - x);

                for (int by = 0; by < blockHeight && !different; by++)
                {
                    int rowOffset = (y + by) * stride;
                    for (int bx = 0; bx < blockWidth && !different; bx++)
                    {
                        int i = rowOffset + (x + bx) * bytesPerPixel;
                        if (oldPixels[i] != newPixels[i] || oldPixels[i + 1] != newPixels[i + 1] || oldPixels[i + 2] != newPixels[i + 2])
                        {
                            different = true;
                        }
                    }
                }

                if (different)
                {
                    rois.Add(new Rectangle(x, y, blockWidth, blockHeight));
                }
            }
        }

        return rois;
    }

    private static bool FindMovedPixel(byte[] oldPixels, byte[] newPixels, ReadOnlySpan<byte> pixelColor, int originalX, int originalY, int width, int height)
    {
        int searchRadius = 10; // Adjust this value to change the search area
        int bytesPerPixel = 4;
        int stride = width * bytesPerPixel;

        for (int y = Math.Max(0, originalY - searchRadius); y < Math.Min(height, originalY + searchRadius); y++)
        {
            for (int x = Math.Max(0, originalX - searchRadius); x < Math.Min(width, originalX + searchRadius); x++)
            {
                if (x == originalX && y == originalY) continue;

                int i = (y * width + x) * bytesPerPixel;
                if (newPixels[i] == pixelColor[0] && newPixels[i + 1] == pixelColor[1] && newPixels[i + 2] == pixelColor[2])
                {
                    return true;
                }
            }
        }

        return false;
    }
}