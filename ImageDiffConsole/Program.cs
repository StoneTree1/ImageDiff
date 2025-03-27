// See https://aka.ms/new-console-template for more information
using ImageDiff;
using ImageDiffConsole;
using Newtonsoft.Json;
using System.Security.AccessControl;
using Tesseract;
using System.IO;
using ImageDiff.MultiPlatform;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

string logging = "";
    string path = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
    string newImagePath = ""; //take path or base64 image?
    string baselineImagePath = "";
    string tesseractPath = "";
    string outputPath = "";
    int SearchHeight = 0;
    int SearchWidth = 0;
    int PixelThreshold = 0;
    int BlockWidth = 0;
    int BlockHeight = 0;
    double ImageDetectionThreshold = 0;
    bool useComparable = false;
CompareResult result = new CompareResult();
try
{
    ProcessArguments(args);
    
    var compareSettings = new CompareSettings()
    {
        BlockHeight = BlockHeight,
        BlockWidth = BlockWidth,
        ImageDetectionThreshold = ImageDetectionThreshold,
        PixelThreshold = PixelThreshold,
        SearchHeight = SearchHeight,
        SearchWidth = SearchWidth,
        CompareType = CompareType.DetectMovement
    };
    bool isDifferent = false;
    logging += $"Use comparableImage: {useComparable}\n";
    if (useComparable)
    {

        List<ImageDiff.MultiPlatform.Difference> differences;
        ComparableImageV2 newImage = null;
        ComparableImageV2 baseline = null;
        if (Path.Exists(baselineImagePath))
        {
            baseline = new ComparableImageV2(compareSettings, baselineImagePath);
        }
        else
        {
            logging += $"Couldnt find the baseline file!\n";
            baseline = new ComparableImageV2(compareSettings, Convert.FromBase64String(baselineImagePath));
        }
        if (Path.Exists(newImagePath))
        {
            newImage = new ComparableImageV2(compareSettings, newImagePath);
        }
        else
        {
            logging += $"Couldnt find the newImage file!\n";
            newImage = new ComparableImageV2(compareSettings, Convert.FromBase64String(newImagePath));
        }
        var resultBitmap = newImage.FastCompareTo(baseline, out isDifferent);
        logging += $"Fast compare complete, isDifferent: {isDifferent}\n";
        if (resultBitmap == null)
        {
            logging += "Result bitmap was null. Fast compare must have had error";
        }
        if (isDifferent)
        {
            //do a more in depth analysis
            result = DoFurtherProcessingWithComparable(newImage, baseline);
            //if(result == null)
            //{
            //    result = DoFurtherProcessing(newImage, baseline);
            //}
            if (result == null)
            {
                throw new Exception("Retry of deeper processing still failed after retry.\n" + logging);
            }
        }
    }
    else
    {

        List<ImageDiff.Difference> differences;
        DiffImage newImage = null;
        DiffImage baseline = null;
        if (Path.Exists(baselineImagePath))
        {
            baseline = new DiffImage(compareSettings, baselineImagePath);
        }
        else
        {
            logging += $"Couldnt find the baseline file!\n";
            baseline = new DiffImage(compareSettings, Convert.FromBase64String(baselineImagePath));
        }
        if (Path.Exists(newImagePath))
        {
            newImage = new DiffImage(compareSettings, newImagePath);
        }
        else
        {
            logging += $"Couldnt find the newImage file!\n";
            newImage = new DiffImage(compareSettings, Convert.FromBase64String(newImagePath));
        }
        var resultBitmap = newImage.FastCompareTo(baseline, out isDifferent);
        logging += $"Fast compare complete, isDifferent: {isDifferent}\n";
        if (resultBitmap == null)
        {
            logging += "Result bitmap was null. Fast compare must have had error";
        }
        if (isDifferent)
        {
            //do a more in depth analysis
            result = DoFurtherProcessing(newImage, baseline);
            //if(result == null)
            //{
            //    result = DoFurtherProcessing(newImage, baseline);
            //}
            if (result == null)
            {
                throw new Exception("Retry of deeper processing still failed after retry.\n" + logging);
            }
        }
    }

    Console.WriteLine($"{JsonConvert.SerializeObject(result)}");
}
catch (Exception ex)
{
    var exp = ex.InnerException;
    int count = 0;
    while (exp != null && count<5)
    {
        logging += $"Inner exception: \n{exp.Message}\n{exp.StackTrace}\n";
        exp = exp.InnerException;
        count++;
    }
    Console.WriteLine($"Exception caught.\n{ex.Message}\n{ex.StackTrace}");
    //temporarily log the errors for troubleshooting
    File.WriteAllText($"C:\\QATools\\ImgDiff\\{Guid.NewGuid().ToString()}-Error.txt", $"{logging}\nException caught.\n{ex.Message}\n{ex.StackTrace}");
    //todo update compare result to have an error component. And set it here
}
finally
{
}


void SetDirectoryPermissions(string path)
{
    DirectoryInfo directory = new DirectoryInfo(path);
    DirectorySecurity security = directory.GetAccessControl();
    security.AddAccessRule(new FileSystemAccessRule(@"EVERYONE", FileSystemRights.FullControl, AccessControlType.Allow));
    directory.SetAccessControl(security);
}
CompareResult DoFurtherProcessingWithComparable(ComparableImageV2 newImage, ComparableImageV2 baseline)
{
    var workingFolder = $"{tesseractPath}";
    var workingFolderB = $"{tesseractPath}";

    logging += $"tesseractPath: {tesseractPath}\n";
    CompareResult result = new CompareResult();
    bool isDifferent = false;
    List<ImageDiff.MultiPlatform.Difference> differences;
    try
    {
        logging += $"UsingTesseractPath: {workingFolder}\nImageSize:{newImage.RawImage.GetLength(0)},{newImage.RawImage.GetLength(1)}\n";
        logging += $"Checking Tesseract folder path exists for 'workingFolder': {Path.Exists(workingFolder)}\n";
        using (var engine = new TesseractEngine(tesseractPath, "eng", EngineMode.Default))
        {
            logging += $"Processing with tesseract for image: {newImagePath}, file exists: {System.IO.File.Exists(newImagePath)}\n";
            newImage.DetectAreasOfInterestUsingTesseract(engine, newImagePath);
            logging += $"Areas detected\n";
            newImage.PreProcessImageUsingAreasOfInterest();
            logging += $"new image processed\n";

            baseline.DetectAreasOfInterestUsingTesseract(engine, baselineImagePath);
            baseline.PreProcessImageUsingAreasOfInterest();
            logging += $"baseline image processed\n";
        }
        if(baseline.RawImage.GetLength(0) == 0)
        {
            logging += "Baseline had no length!";
        }
        SixLabors.ImageSharp.Image<Rgba32> differencesImage = newImage.CompareTo(baseline, out differences);
        isDifferent = differences.Exists(x => x.Type != DifferenceType.None);
        logging += $"IsDifferent after deeper comparison: {isDifferent}\n";
        if (isDifferent)
        {
            var resultPath = $"{outputPath}\\{Guid.NewGuid().ToString()}.png";
            logging += $"Saving result image to: {resultPath}\n";
            differencesImage.Save(resultPath);
            result.IsDifferent = true;
            result.ResultFile = resultPath;
        }
        differencesImage.Dispose();
        return result;

    }
    catch (Exception ex)
    {
        var exp = ex;
        int count = 0;
        while (exp != null && count < 5)
        {
            logging += $"Inner exception: \n{exp.Message}\n{exp.StackTrace}\n";
            exp = exp.InnerException;
            count++;
        }
        return null;
    }
    finally
    {
        //Directory.Delete(workingFolder, true);
        // Directory.Delete(workingFolderB, true);
    }

}
CompareResult DoFurtherProcessing(DiffImage newImage,DiffImage baseline)
{
    var workingFolder = $"{tesseractPath}";
    var workingFolderB = $"{tesseractPath}";

    logging += $"tesseractPath: {tesseractPath}\n";
    CompareResult result = new CompareResult();
    bool isDifferent = false;
    List<ImageDiff.Difference> differences;
    try
    {
        logging += $"UsingTesseractPath: {workingFolder}\nImageSize:{newImage.RawImage.GetLength(0)},{newImage.RawImage.GetLength(1)}\n";
        logging += $"Checking Tesseract folder path exists for 'workingFolder': {Path.Exists(workingFolder)}\n";
        using (var engine = new TesseractEngine(tesseractPath, "eng", EngineMode.Default))
        {
            logging += $"Processing with tesseract for image: {newImagePath}, file exists: {System.IO.File.Exists(newImagePath)}\n";
            newImage.DetectAreasOfInterestUsingTesseract(engine, newImagePath);
            logging += $"Areas detected\n";
            newImage.PreProcessImageUsingAreasOfInterest();
            logging += $"new image processed\n";
            logging += $"new image width: {newImage.RawImage.GetLength(1)}";
            logging += $"new image height: {newImage.RawImage.GetLength(0)}";

            baseline.DetectAreasOfInterestUsingTesseract(engine, baselineImagePath);
            baseline.PreProcessImageUsingAreasOfInterest();
            logging += $"baseline image processed\n";
            logging += $"Baseline width: {baseline.RawImage.GetLength(1)}";
            logging += $"Baseline height: {baseline.RawImage.GetLength(0)}";
        }
        var differencesImage = newImage.CompareTo(baseline, out differences);
        isDifferent = differences.Exists(x => x.Type != DifferenceType.None);
        logging += $"IsDifferent after deeper comparison: {isDifferent}\n";
        if (isDifferent)
        {
            var resultPath = $"{outputPath}\\{Guid.NewGuid().ToString()}.png";
            logging += $"Saving result image to: {resultPath}\n";
            differencesImage.Save(resultPath);
            result.IsDifferent = true;
            result.ResultFile = resultPath;
        }
        differencesImage.Dispose();
        return result;        
    } 
    catch (Exception ex)
    {
        var exp = ex;
        int count = 0;
        while (exp != null && count < 5)
        {
            logging += $"Inner exception: \n{exp.Message}\n{exp.StackTrace}\n";
            exp = exp.InnerException;
            count++;
        }
        return null;
    } 
    finally
    {
        //Directory.Delete(workingFolder, true);
       // Directory.Delete(workingFolderB, true);
    }

}
void ProcessArguments(string[] args)
{
    foreach (var arg in args)
    {
        logging += $"{arg}\n";
        var vals = arg.Split('=');
        //Console.WriteLine($"Processing Arg: {vals[0]}:{vals[1]}");
        switch (vals[0])
        {
            case "newImage":
                newImagePath = vals[1];
                break;
            case "baseline":
                baselineImagePath = vals[1];
                break;
            case "tesseractPath":
                //var workingFolder = $"{vals[1]}\\{Guid.NewGuid().ToString()}";
                //Directory.CreateDirectory(workingFolder);
                //System.IO.File.Copy($"{vals[1]}\\eng.traineddata", $"{workingFolder}\\eng.traineddata");
                //tesseractPath = workingFolder;
                tesseractPath = vals[1];
                logging += $"tesseractPath: {tesseractPath}\n";
                break;
            case "SearchHeight":
                SearchHeight = int.Parse(vals[1]);
                break;
            case "SearchWidth":
                SearchWidth = int.Parse(vals[1]);
                break;
            case "PixelThreshold":
                PixelThreshold = int.Parse(vals[1]);
                break;
            case "BlockWidth":
                BlockWidth = int.Parse(vals[1]);
                break;
            case "BlockHeight":
                BlockHeight = int.Parse(vals[1]);
                break;
            case "ImageDetectionThreshold":
                ImageDetectionThreshold = double.Parse(vals[1]);
                break;
            case "OutputPath":
                outputPath = vals[1];
                break;
            case "UseComparableImage":
                useComparable = bool.Parse(vals[1]);
                break;
        }
    }
}