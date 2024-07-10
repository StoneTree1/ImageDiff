
//using FFMediaToolkit;
//using FFMediaToolkit.Encoding;
//using FFMediaToolkit.Graphics;
using System;
using System.IO;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using SixLabors.Fonts;
using System.Drawing;

namespace ImageDiff
{
    public class FileLogger
    {
       // public static MediaOutput videoBuilder;
        //public static string videoFilePath;
        //public static AnimatedGifCreator videoGif;
       // private static readonly object videoBuilderLock = new object();

        public void InitialiseDependancies()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //FFmpegLoader.FFmpegPath = path + "\\Dependancies";
                File.Delete($"{path}\\log.txt");
            }
        }

        public static void Log(string text)
        {
            //File.AppendAllText("C:\\tmp\\ImageLogging.txt", $"\n{text}");
           // var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            //File.AppendAllText($"{path}\\CompareResults\\log.txt", text);
        }

        //public void InitializeVideoLogger(string fileName)
        //{
        //    if (videoBuilder == null)
        //    {
        //        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        //        var settings = new VideoEncoderSettings(width: 1920, height: 1080, framerate: 60, codec: VideoCodec.H264);
        //        settings.EncoderPreset = EncoderPreset.VerySlow;
        //        settings.CRF = 17;
        //        videoBuilder = MediaBuilder.CreateContainer($"{path}\\{fileName}.mp4").WithVideo(settings).Create();
        //    }
        //}


        //public void LogImage(ImagePixels image)
        //{
        //    byte[] data = image.ToArray();
        //    //var bmd = ImageData.FromArray(imageBytes.ToArray(), ImagePixelFormat.Bgr24, new System.Drawing.Size(img.Size().Width, img.Size().Height));
        //    var estimate = EstimateStride(image.Rows[0].Count, ImagePixelFormat.Bgr24) * image.Rows.Count;
        //    var bmd = ImageData.FromArray(data, ImagePixelFormat.Bgr24, new System.Drawing.Size(image.Rows[0].Count, image.Rows.Count));
        //    videoBuilder.Video.AddFrame(bmd);
        //}
        //public int EstimateStride(int width, ImagePixelFormat format) => 4 * (((24 * width) + 31) / 32);
        //public void LogScreenshot(string title, byte[] image, bool force = false)
        //{
        //    try
        //    {
        //        var ii = SixLabors.ImageSharp.Image.Load(image);
        //        SixLabors.ImageSharp.Image<Rgba32> img = (SixLabors.ImageSharp.Image<Rgba32>)SixLabors.ImageSharp.Image<Rgba32>.Load(image);

        //        // PixelAccessor<Rgba32> access = new SixLabors.ImageSharp.PixelAccessor<Rgba32>();

        //        Image<Rgba32> targetImage = new Image<Rgba32>(img.Size().Width, img.Size().Height);
        //        int height = img.Size().Height;
        //        List<byte> imageBytes = new List<byte>();
        //        img.ProcessPixelRows(targetImage, (sourceAccessor, targetAccessor) =>
        //        {
        //            for (int i = 0; i < height; i++)
        //            {
        //                var span = sourceAccessor.GetRowSpan(i);
        //                foreach (var pixel in span)
        //                {
        //                    imageBytes.Add(pixel.B);
        //                    imageBytes.Add(pixel.G);
        //                    imageBytes.Add(pixel.R);
        //                }
        //            }
        //        });
        //        var bmd = ImageData.FromArray(imageBytes.ToArray(), ImagePixelFormat.Bgr24, new System.Drawing.Size(img.Size().Width, img.Size().Height));
        //        videoBuilder.Video.AddFrame(bmd);

        //    }
        //    catch (Exception ex)
        //    {
        //        var s = ":";
        //    }
        //}

        //public void LogMessage(string message)
        //{
        //    try
        //    {
        //        //Logger.Debug("Logging message as screenshot");
        //        //if (TestRunSettings.ImageLoggingStyle != ImageLoggingStyle.Images)
        //        //{
        //        //    Bitmap bitmap = new Bitmap(width: 1920, height: 1080);
        //        //    using (Graphics graphics = Graphics.FromImage(bitmap))
        //        //    {

        //        //        Font font = new Font(FontFamily.GenericSerif, 20);
        //        //        graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, bitmap.Width, bitmap.Height);
        //        //        graphics.DrawString(message, font, new SolidBrush(Color.Black), 50, 50);
        //        //        graphics.Flush();
        //        //        font.Dispose();
        //        //        graphics.Dispose();
        //        //    }
        //        //    //ImageConverter converter = new ImageConverter();
        //        //    //var asBytes = (byte[])converter.ConvertTo(bitmap, typeof(byte[]));
        //        //    //LogScreenshot("User Merssage", asBytes, true);

        //        //    var rect = new Rectangle(Point.Empty, bitmap.Size);
        //        //    var bitLock = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        //        //    var bitmapData = ImageData.FromPointer(bitLock.Scan0, ImagePixelFormat.Bgr24, bitmap.Size);
        //        //    lock (videoBuilderLock)
        //        //    {
        //        //        videoBuilder.Video.AddFrame(bitmapData);
        //        //    }
        //        //}
        //        //Logger.Debug("End Logging message as screenshot");

        //        FontCollection coll = new FontCollection();
        //        coll.AddSystemFonts();
        //        var names = "";
        //        foreach (SixLabors.Fonts.FontFamily font in coll.Families)
        //        {
        //            names += font.Name + ",";
        //        }
        //        //Logger.Debug("Available fonts: " + names);

        //        var img = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(800, 600, new Rgba32(255, 255, 255));
        //        //img.Mutate(x => x.BackgroundColor(SixLabors.ImageSharp.Color.White));
        //        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //        {
        //            SixLabors.Fonts.FontFamily family = coll.Get("Arial");
        //            img.Mutate(x => x.DrawText(message, family.CreateFont(20, SixLabors.Fonts.FontStyle.Regular), SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(10, 10)));
        //        }
        //        else
        //        {
        //            SixLabors.Fonts.FontFamily family = coll.Get("DejaVu Sans");
        //            img.Mutate(x => x.DrawText(message, family.CreateFont(20, SixLabors.Fonts.FontStyle.Regular), SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(10, 10)));
        //        }
        //        //img.Mutate(x => x.DrawText(message, family.CreateFont(20, SixLabors.Fonts.FontStyle.Regular), SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.PointF(10, 10)));
        //        Image<Rgba32> targetImage = new Image<Rgba32>(img.Size().Width, img.Size().Height);
        //        List<byte> imageBytes = new List<byte>();
        //        img.ProcessPixelRows(targetImage, (sourceAccessor, targetAccessor) =>
        //        {
        //            for (int i = 0; i < 600; i++)
        //            {
        //                var span = sourceAccessor.GetRowSpan(i);
        //                foreach (var pixel in span)
        //                {
        //                    imageBytes.Add(pixel.B);
        //                    imageBytes.Add(pixel.G);
        //                    imageBytes.Add(pixel.R);
        //                }
        //            }
        //        });
        //        var bmd = ImageData.FromArray(imageBytes.ToArray(), ImagePixelFormat.Bgr24, new System.Drawing.Size(img.Size().Width, img.Size().Height));
        //        videoBuilder.Video.AddFrame(bmd);
        //    }
        //    catch (Exception ex)
        //    {
        //       // Logger.Debug("Error doing custom message" + ex.Message);
        //    }
        //}

        //public void FinalizeVideo()
        //{
        //    try
        //    {
        //                videoBuilder.Dispose();
        //                videoBuilder = null;
        //                //var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\TestRun.mp4";
        //                //var video = File.ReadAllBytes(path);                       
                   
                
        //    }
        //    catch (Exception ex)
        //    {
        //        //Logger.Warn($"Error trying to upload video:\n{ex.Message}\n{ex.StackTrace}");
        //    }

        //}
    }
}

