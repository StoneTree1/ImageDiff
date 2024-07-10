using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiff
{
    public class Row
    {
        public List<Pixel> Pixels;

        public Row()
        {
            Pixels = new List<Pixel>();
        }

        public void Add(Pixel pixel)
        {
            Pixels.Add(pixel);
        }
        public int Count { get { return Pixels.Count; } }
        public bool ContainsSequence(Row sequence)
        {
            if(sequence.Pixels.Count>Pixels.Count)
            {
                FileLogger.Log($"Checking Row failed due to sequence being longer than row\n");
                return false;
            }
            int index = 0;
            bool mismatched = false;

            for(int i=0; i<Pixels.Count; i++)
            {
                mismatched = false;
                if (i == 94)
                {
                    var s = "";
                }
                if (Pixels.Count - sequence.Pixels.Count< i)
                {
                    //if (sequence.Pixels.Count-i < 0)
                    //{
                    FileLogger.Log($"Row does not contain sequence\n");
                    return false;
                }
                int count = 0;
                for(int j=0; j<sequence.Pixels.Count; j++)
                {
                    FileLogger.Log($"Checking pixel {i} vs {j}");
                    count++;
                    if (Pixels.Count > i + j)
                    {
                        if (i + j == 108)
                        {
                            var t = "";
                        }
                        if (!Pixels[i + j].IsMatch(sequence.Pixels[j]))
                        {
                            FileLogger.Log($"Not matched. MatchCount: {count}\n");

                            mismatched = true;
                            break;
                        }
                        else
                        {
                            FileLogger.Log($"matched\n");
                            var s = "";
                        }
                    }
                    else
                    {
                        mismatched = true;
                        break;
                    }
                }
                if (mismatched) continue;
                return true;
            }

            return false;
        }

        internal byte[] ToByteArray()
        {
            List<byte> imageBytes = new List<byte>();
            foreach (var pixel in Pixels)
            {
                imageBytes.Add(pixel.pixel.B);
                imageBytes.Add(pixel.pixel.G);
                imageBytes.Add(pixel.pixel.R);
            }
            return imageBytes.ToArray();
        }
    }
}
