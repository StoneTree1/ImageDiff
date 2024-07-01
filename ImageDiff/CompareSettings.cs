using System.Text.Json.Serialization;

namespace ImageDiff
{
    /// <summary>
    /// Settings to use for ImageDiffCompare
    /// </summary>
    public class CompareSettings
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CompareType CompareType { get; set; }
        public int SearchHeight { get; set; }
        public int SearchWidth { get; set; }
        public int PixelThreshold { get; set; }
        public int BlockWidth { get; set; }
        public int BlockHeight { get; set; }
        public double ImageDetectionThreshold { get; set; }


    }

    public enum CompareType
    {
        Fast,
        FastWithBackgroundDetection,
        DetectMovement,
        DetectRegions
    }
}
