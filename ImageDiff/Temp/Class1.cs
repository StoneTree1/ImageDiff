using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

class ImageDifferenceHighlighter
{
    const int BlockSize = 10;  // Size of the blocks
    const int SearchWindow = 20;  // Size of the search window

    public static void DoProcess(string[] args)
    {
        //if (args.Length < 3)
        //{
        //    Console.WriteLine("Usage: ImageDifferenceHighlighter <image1_path> <image2_path> <output_path>");
        //    return;
        //}

        string image1Path = "C:\\Users\\trist\\Downloads\\WordpressDriverImage_20240620024020.jpg"; // args[0];
        string image2Path = "C:\\Users\\trist\\Downloads\\WordpressImage_20240620024020.jpg"; // args[1];
        string outputPath = "C:\\Users\\trist\\Downloads\\CompareResult.bmp"; // args[2];

        Bitmap image1 = new Bitmap(image1Path);
        Bitmap image2 = new Bitmap(image2Path);

        Bitmap diffImage = HighlightDifferences2(image1, image2);

        diffImage.Save(outputPath);
        Console.WriteLine("Difference image saved to " + outputPath);
    }

    //static Bitmap HighlightDifferences(Bitmap image1, Bitmap image2)
    //{
    //    int width = Math.Min(image1.Width, image2.Width);
    //    int height = Math.Min(image1.Height, image2.Height);
    //    int maxWidth = Math.Max(image1.Width, image2.Width);
    //    int maxHeight = Math.Max(image1.Height, image2.Height);

    //    Bitmap diffImage = new Bitmap(maxWidth, maxHeight);

    //    for (int y = 0; y < height; y += BlockSize)
    //    {
    //        for (int x = 0; x < width; x += BlockSize)
    //        {
    //            if (!IsBlockMoved(image1, image2, x, y))
    //            {
    //                CopyBlock(image1, diffImage, x, y);
    //            }
    //            else
    //            {
    //                HighlightBlock(diffImage, x, y);
    //            }
    //        }
    //    }

    //    // Fill remaining areas from image1 and image2 as in the previous code
    //    FillRemainingArea(diffImage, image1, width, height);
    //    FillRemainingArea(diffImage, image2, width, height);

    //    return diffImage;
    //}

    //static bool IsBlockMoved(Bitmap image1, Bitmap image2, int startX, int startY)
    //{
    //    for (int offsetY = -SearchWindow; offsetY <= SearchWindow; offsetY++)
    //    {
    //        for (int offsetX = -SearchWindow; offsetX <= SearchWindow; offsetX++)
    //        {
    //            if (BlocksMatch(image1, image2, startX, startY, startX + offsetX, startY + offsetY))
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    //static bool BlocksMatch(Bitmap image1, Bitmap image2, int x1, int y1, int x2, int y2)
    //{
    //    for (int y = 0; y < BlockSize; y++)
    //    {
    //        for (int x = 0; x < BlockSize; x++)
    //        {
    //            if (x1 + x < 0 || y1 + y < 0 || x2 + x < 0 || y2 + y < 0 ||
    //                x1 + x >= image1.Width || y1 + y >= image1.Height ||
    //                x2 + x >= image2.Width || y2 + y >= image2.Height)
    //            {
    //                return false;
    //            }

    //            if (image1.GetPixel(x1 + x, y1 + y) != image2.GetPixel(x2 + x, y2 + y))
    //            {
    //                return false;
    //            }
    //        }
    //    }
    //    return true;
    //}

    //static void CopyBlock(Bitmap source, Bitmap destination, int startX, int startY)
    //{
    //    for (int y = 0; y < BlockSize; y++)
    //    {
    //        for (int x = 0; x < BlockSize; x++)
    //        {
    //            if (startX + x < destination.Width && startY + y < destination.Height &&
    //                startX + x < source.Width && startY + y < source.Height)
    //            {
    //                destination.SetPixel(startX + x, startY + y, source.GetPixel(startX + x, startY + y));
    //            }
    //        }
    //    }
    //}

    //static void HighlightBlock(Bitmap image, int startX, int startY)
    //{
    //    for (int y = 0; y < BlockSize; y++)
    //    {
    //        for (int x = 0; x < BlockSize; x++)
    //        {
    //            if (startX + x < image.Width && startY + y < image.Height)
    //            {
    //                image.SetPixel(startX + x, startY + y, Color.Magenta);
    //            }
    //        }
    //    }
    //}

    //static void FillRemainingArea(Bitmap diffImage, Bitmap sourceImage, int width, int height)
    //{
    //    for (int y = height; y < sourceImage.Height; y++)
    //    {
    //        for (int x = 0; x < sourceImage.Width; x++)
    //        {
    //            if (x < diffImage.Width && y < diffImage.Height)
    //            {
    //                diffImage.SetPixel(x, y, sourceImage.GetPixel(x, y));
    //            }
    //        }
    //    }

    //    for (int y = 0; y < sourceImage.Height; y++)
    //    {
    //        for (int x = width; x < sourceImage.Width; x++)
    //        {
    //            if (x < diffImage.Width && y < diffImage.Height)
    //            {
    //                diffImage.SetPixel(x, y, sourceImage.GetPixel(x, y));
    //            }
    //        }
    //    }
    //}



    static Bitmap HighlightDifferences2(Bitmap image1, Bitmap image2)
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
        int diffImagebytesPerPixel = Image.GetPixelFormatSize(diffImage.PixelFormat) / 8;
        int stride1 = data1.Stride;
        int stride2 = data2.Stride;
        int diffStride = diffData.Stride;

        byte[] buffer1 = new byte[data1.Height * stride1];
        byte[] buffer2 = new byte[data2.Height * stride2];
        byte[] bufferDiff = new byte[diffData.Height * diffStride];

        Marshal.Copy(data1.Scan0, buffer1, 0, buffer1.Length);
        Marshal.Copy(data2.Scan0, buffer2, 0, buffer2.Length);
        Marshal.Copy(diffData.Scan0, bufferDiff, 0, bufferDiff.Length);

        for (int y = 0; y < height; y += BlockSize)
        {
            for (int x = 0; x < width; x += BlockSize)
            {
                if (!IsBlockMoved(buffer1, buffer2, x, y, stride1, stride2, bytesPerPixel))
                {
                    CopyBlock(buffer1, bufferDiff, x, y, stride1, diffStride, bytesPerPixel);
                }
                else
                {
                    HighlightBlock(bufferDiff, x, y, diffStride, bytesPerPixel);
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

    static bool IsBlockMoved(byte[] buffer1, byte[] buffer2, int startX, int startY, int stride1, int stride2, int bytesPerPixel)
    {
        for (int offsetY = -SearchWindow; offsetY <= SearchWindow; offsetY++)
        {
            for (int offsetX = -SearchWindow; offsetX <= SearchWindow; offsetX++)
            {
                if (BlocksMatch(buffer1, buffer2, startX, startY, startX + offsetX, startY + offsetY, stride1, stride2, bytesPerPixel))
                {
                    return true;
                }
            }
        }
        return false;
    }

    static bool BlocksMatch(byte[] buffer1, byte[] buffer2, int x1, int y1, int x2, int y2, int stride1, int stride2, int bytesPerPixel)
    {
        for (int y = 0; y < BlockSize; y++)
        {
            for (int x = 0; x < BlockSize; x++)
            {
                int index1 = ((y1 + y) * stride1) + ((x1 + x) * bytesPerPixel);
                int index2 = ((y2 + y) * stride2) + ((x2 + x) * bytesPerPixel);

                if (index1 < 0 || index2 < 0 || index1 >= buffer1.Length || index2 >= buffer2.Length)
                {
                    return false;
                }

                for (int i = 0; i < bytesPerPixel; i++)
                {
                    if (buffer1[index1 + i] != buffer2[index2 + i])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
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

    static void HighlightBlock(byte[] buffer, int startX, int startY, int stride, int bytesPerPixel)
    {
        for (int y = 0; y < BlockSize; y++)
        {
            for (int x = 0; x < BlockSize; x++)
            {
                int index = ((startY + y) * stride) + ((startX + x) * bytesPerPixel);

                if (index < buffer.Length)
                {
                    buffer[index] = 255; // Red
                    buffer[index + 1] = 0; // Green
                    buffer[index + 2] = 0; // Blue
                    if (bytesPerPixel == 4)
                    {
                        buffer[index + 3] = 255; // Alpha
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
