using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDiff
{
    public class ImagePixels
    {
        public List<Row> Rows;

        public ImagePixels()
        {
            Rows = new List<Row>();
        }

        public void Save(string name)
        {

        }

        public void AddRow(Row row)
        {
            Rows.Add(row);
        }

        public bool ContainsImage(ImagePixels otherImage)
        {
            Logger.Log("Checking for image movement");
            if (otherImage.Rows.Count > Rows.Count)
            {
                return false;
            }

            int index = 0;
            int matchedRows = 0;
            bool mismatched = false;
            int indexLastMatched = 0;
            foreach (var row in otherImage.Rows)
            {
                
                bool rowMatched = false;
                
                for (int i = indexLastMatched; i < Rows.Count; i++)
                {
                    if (i == 100)
                    {
                        var s = "";
                    }
                    index++;
                    Logger.Log($"Checking Row {i}\n");
                    if (Rows[i].ContainsSequence(row))
                    {
                        Logger.Log($"Row {i} matched\n");
                        matchedRows++;
                        indexLastMatched = i+1;
                        rowMatched = true;
                        break;
                    }
                    else
                    {
                        Logger.Log($"Row {i} did not match\n");
                    }
                }
                if (!rowMatched)
                {
                    return false;
                }
            }

            //for (int i = 0; i < Rows.Count; i++)
            //{
            //    mismatched = false;
            //    if ((otherImage.Rows.Count - Rows.Count)>i)
            //    {
            //        return false;
            //    }
            //    for (int j = 0; j < otherImage.Rows.Count; j++)
            //    {
            //        if (!Rows[i + j].ContainsSequence(otherImage.Rows[j]))
            //        {
            //            mismatched = true;
            //            break;
            //        }
            //    }
            //    if (mismatched) continue;
            //    return true;
            //}

            return true;
        }

        internal void AddRow(Span<Rgba32> pixelRow)
        {
            Row newRow = new Row();
            foreach(var pixel in pixelRow)
            {
                newRow.Pixels.Add(new Pixel(pixel, 0, 0));
            }
            Rows.Add(newRow);
        }

        internal byte[] ToArray()
        {
            List<byte> imageBytes = new List<byte>();
            foreach(var row in Rows)
            {
                imageBytes.AddRange(row.ToByteArray());      
            }
            return imageBytes.ToArray();
        }
    }
}
