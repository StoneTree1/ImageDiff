using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

public class ImageDifferenceHighlighter4
{
    const int BlockSize = 10;  // Size of the blocks
    const int SearchWindow = 20;  // Size of the search window
    const int Threshold = 30;  // Threshold for pixel intensity difference

    public static void DoProcess(string[] args)
    {
        //if (args.Length < 3)
        //{
        //    Console.WriteLine("Usage: ImageDifferenceHighlighter <image1_path> <image2_path> <output_path>");
        //    return;
        //}

        string image1Path = "C:\\Users\\trist\\Downloads\\WordpressDriverImage_20240620024020.jpg"; // args[0];
        string image2Path = "C:\\Users\\trist\\Downloads\\WordpressImage_20240620024020.jpg"; // args[1];
        string outputPath = "C:\\Users\\trist\\Downloads\\CompareResultV3.bmp"; // args[2];


        Bitmap image1 = new Bitmap(image1Path);
        Bitmap image2 = new Bitmap(image2Path);

        Bitmap diffImage = HighlightDifferences(image1, image2);

        diffImage.Save(outputPath);
        Console.WriteLine("Difference image saved to " + outputPath);
    }

    static Bitmap HighlightDifferences(Bitmap image1, Bitmap image2)
    {
        int width = Math.Min(image1.Width, image2.Width);
        int height = Math.Min(image1.Height, image2.Height);
        int maxWidth = Math.Max(image1.Width, image2.Width);
        int maxHeight = Math.Max(image1.Height, image2.Height);

        Bitmap diffImage = new Bitmap(maxWidth, maxHeight, PixelFormat.Format24bppRgb);

        BitmapData data1 = image1.LockBits(new Rectangle(0, 0, image1.Width, image1.Height), ImageLockMode.ReadOnly, image1.PixelFormat);
        BitmapData data2 = image2.LockBits(new Rectangle(0, 0, image2.Width, image2.Height), ImageLockMode.ReadOnly, image2.PixelFormat);
        BitmapData diffData = diffImage.LockBits(new Rectangle(0, 0, diffImage.Width, diffImage.Height), ImageLockMode.WriteOnly, diffImage.PixelFormat);

        int bytesPerPixel = Image.GetPixelFormatSize(image1.PixelFormat) / 8;
        int stride1 = data1.Stride;
        int stride2 = data2.Stride;
        int diffStride = diffData.Stride;

        byte[] buffer1 = new byte[data1.Height * stride1];
        byte[] buffer2 = new byte[data2.Height * stride2];
        byte[] bufferDiff = new byte[diffData.Height * diffStride];

        Marshal.Copy(data1.Scan0, buffer1, 0, buffer1.Length);
        Marshal.Copy(data2.Scan0, buffer2, 0, buffer2.Length);
        Marshal.Copy(diffData.Scan0, bufferDiff, 0, bufferDiff.Length);

        bool[,] visited = new bool[height / BlockSize, width / BlockSize];

        for (int y = 0; y < height; y += BlockSize)
        {
            for (int x = 0; x < width; x += BlockSize)
            {
                if (!IsVisited(visited, x, y))
                {
                    Point match = FindBestMatch(buffer1, buffer2, x, y, stride1, stride2, bytesPerPixel);
                    if (match.X != -1 && match.Y != -1)
                    {
                        CopyBlock(buffer2, bufferDiff, match.X, match.Y, stride2, diffStride, bytesPerPixel);
                        SetVisited(visited, match.X, match.Y);
                    }
                    else
                    {
                        HighlightBlock(bufferDiff, buffer1, buffer2, x, y, stride1, stride2, diffStride, bytesPerPixel);
                    }
                }
            }
        }

        // Fill remaining areas from image1 and image2 as in the previous code
        FillRemainingArea(bufferDiff, buffer1, width, height, diffStride, stride1, bytesPerPixel);
        FillRemainingArea(bufferDiff, buffer2, width, height, diffStride, stride2, bytesPerPixel);

        Marshal.Copy(bufferDiff, 0, diffData.Scan0, bufferDiff.Length);

        image1.UnlockBits(data1);
        image2.UnlockBits(data2);
        diffImage.UnlockBits(diffData);

        return diffImage;
    }

    static bool IsVisited(bool[,] visited, int x, int y)
    {
        int blockX = x / BlockSize;
        int blockY = y / BlockSize;
        if (blockX < 0 || blockX >= visited.GetLength(1) || blockY < 0 || blockY >= visited.GetLength(0))
        {
            return true; // Treat out of bounds as visited to avoid exceptions
        }
        return visited[blockY, blockX];
    }

    static void SetVisited(bool[,] visited, int x, int y)
    {
        int blockX = x / BlockSize;
        int blockY = y / BlockSize;
        if (blockX >= 0 && blockX < visited.GetLength(1) && blockY >= 0 && blockY < visited.GetLength(0))
        {
            visited[blockY, blockX] = true;
        }
    }

    static Point FindBestMatch(byte[] buffer1, byte[] buffer2, int startX, int startY, int stride1, int stride2, int bytesPerPixel)
    {
        int bestMatchX = -1;
        int bestMatchY = -1;
        double bestDifference = double.MaxValue;

        for (int offsetY = -SearchWindow; offsetY <= SearchWindow; offsetY++)
        {
            for (int offsetX = -SearchWindow; offsetX <= SearchWindow; offsetX++)
            {
                int x2 = startX + offsetX;
                int y2 = startY + offsetY;
                if (x2 >= 0 && y2 >= 0 && x2 + BlockSize <= buffer2.Length / stride2 * bytesPerPixel && y2 + BlockSize <= buffer2.Length / stride2 * bytesPerPixel)
                {
                    double difference = CalculateBlockDifference(buffer1, buffer2, startX, startY, x2, y2, stride1, stride2, bytesPerPixel);
                    if (difference < bestDifference)
                    {
                        bestDifference = difference;
                        bestMatchX = x2;
                        bestMatchY = y2;
                    }
                }
            }
        }

        if (bestDifference <= Threshold)
        {
            return new Point(bestMatchX, bestMatchY);
        }
        return new Point(-1, -1);
    }

    static double CalculateBlockDifference(byte[] buffer1, byte[] buffer2, int x1, int y1, int x2, int y2, int stride1, int stride2, int bytesPerPixel)
    {
        double difference = 0;
        for (int y = 0; y < BlockSize; y++)
        {
            for (int x = 0; x < BlockSize; x++)
            {
                int index1 = ((y1 + y) * stride1) + ((x1 + x) * bytesPerPixel);
                int index2 = ((y2 + y) * stride2) + ((x2 + x) * bytesPerPixel);

                if (index1 < buffer1.Length && index2 < buffer2.Length)
                {
                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        difference += Math.Abs(buffer1[index1 + i] - buffer2[index2 + i]);
                    }
                }
            }
        }
        return difference / (BlockSize * BlockSize * bytesPerPixel);
    }

    static void CopyBlock(byte[] source, byte[] destination, int startX, int startY, int sourceStride, int destStride, int bytesPerPixel)
    {
        for (int y = 0; y < BlockSize; y++)
        {
            for (int x = 0; x < BlockSize; x++)
            {
                int srcIndex = ((startY + y) * sourceStride) + ((startX + x) * bytesPerPixel);
                int destIndex = ((startY + y) * destStride) + ((startX + x) * bytesPerPixel);

                if (srcIndex < source.Length && destIndex < destination.Length)
                {
                    Array.Copy(source, srcIndex, destination, destIndex, bytesPerPixel);
                }
            }
        }
    }

    static void HighlightBlock(byte[] buffer, byte[] buffer1, byte[] buffer2, int startX, int startY, int stride1, int stride2, int diffStride, int bytesPerPixel)
    {
        for (int y = 0; y < BlockSize; y++)
        {
            for (int x = 0; x < BlockSize; x++)
            {
                int index1 = ((startY + y) * stride1) + ((startX + x) * bytesPerPixel);
                int index2 = ((startY + y) * stride2) + ((startX + x) * bytesPerPixel);
                int diffIndex = ((startY + y) * diffStride) + ((startX + x) * bytesPerPixel);

                if (index1 < buffer1.Length && index2 < buffer2.Length && diffIndex < buffer.Length)
                {
                    bool isForegroundPixel = false;

                    for (int i = 0; i < bytesPerPixel; i++)
                    {
                        if (Math.Abs(buffer1[index1 + i] - buffer2[index2 + i]) > Threshold)
                        {
                            isForegroundPixel = true;
                            break;
                        }
                    }

                    if (isForegroundPixel)
                    {
                        buffer[diffIndex] = 255; // Red
                        buffer[diffIndex + 1] = 0; // Green
                        buffer[diffIndex + 2] = 0; // Blue
                        if (bytesPerPixel == 4)
                        {
                            buffer[diffIndex + 3] = 255; // Alpha
                        }
                    }
                    else
                    {
                        Array.Copy(buffer1, index1, buffer, diffIndex, bytesPerPixel);
                    }
                }
            }
        }
    }

    static void FillRemainingArea(byte[] diffBuffer, byte[] sourceBuffer, int width, int height, int diffStride, int sourceStride, int bytesPerPixel)
    {
        for (int y = height; y < sourceBuffer.Length / sourceStride; y++)
        {
            for (int x = 0; x < sourceStride / bytesPerPixel; x++)
            {
                int srcIndex = (y * sourceStride) + (x * bytesPerPixel);
                int destIndex = (y * diffStride) + (x * bytesPerPixel);

                if (srcIndex < sourceBuffer.Length && destIndex < diffBuffer.Length)
                {
                    Array.Copy(sourceBuffer, srcIndex, diffBuffer, destIndex, bytesPerPixel);
                }
            }
        }

        for (int y = 0; y < sourceBuffer.Length / sourceStride; y++)
        {
            for (int x = width; x < sourceStride / bytesPerPixel; x++)
            {
                int srcIndex = (y * sourceStride) + (x * bytesPerPixel);
                int destIndex = (y * diffStride) + (x * bytesPerPixel);

                if (srcIndex < sourceBuffer.Length && destIndex < diffBuffer.Length)
                {
                    Array.Copy(sourceBuffer, srcIndex, diffBuffer, destIndex, bytesPerPixel);
                }
            }
        }
    }
}