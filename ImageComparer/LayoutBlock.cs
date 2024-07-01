using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace ImageDiff
{
    public class LayoutBlock
    {
        public Rect Bounds { get; set; }
        public string Text {  get; set; }
        public PolyBlockType BlockType { get; set; }
    }
}
