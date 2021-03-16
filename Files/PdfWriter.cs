using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crusade.Files
{
    public class PdfWriter
    {        
        public PdfWriter()
        {
            WriteString("%PDF-1.7");            
        }

        MemoryStream stream = new MemoryStream();

        List<long> ObjectPositions = new List<long>();
        List<PageData> Pages = new List<PageData>();
        int objCount = 0;

        void BeginObject()
        {
            objCount++;
            ObjectPositions.Add(stream.Length);
            WriteString($"{objCount} 0 obj");
            WriteString("<<");
        }

        void EndObject()
        {
            WriteString(">>");
            WriteString("endobj");
        }

        void WriteString(string str, bool endl = true)
        {
            byte[] bytes = Encoding.Default.GetBytes(str + (endl ? "\n" : ""));
            stream.Write(bytes, 0, bytes.Length);
        }

        int catalogId;
        void WriteCatalog()
        {
            BeginObject();
            catalogId = objCount;
            WriteString("/Type /Catalog");
            WriteString($"/Pages {objCount + 1} 0 R");
            EndObject();
        }

        void WritePagesHeader()
        {
            BeginObject();
            string del = " ";
            int k = objCount, cnt = Pages.Count;
            var pageIds = new List<int>();
            for (int i = 0; i < cnt; i++)
                pageIds.Add(++k);
            WriteString($"/Kids [{string.Join(" ", pageIds.Select(i => i + " 0 R"))}]");
            WriteString("/Type /Pages");
            WriteString($"/Count {cnt}");
            EndObject();
        }

        void WritePage(PageData pData)
        {
            BeginObject();
            string w = (pData.PageWidth).ToString("f7");
            string h = (pData.PageHeight).ToString("f7");
            WriteString($"/Contents {pData.objId + 1} 0 R");
            WriteString("/Type /Page");
            WriteString("/Resources");
            WriteString($"<<\n/XObject\n<<\n/X0 {pData.objId} 0 R\n>>\n>>");
            WriteString($"/Parent {catalogId + 1} 0 R"); // Solves Bug [No. 003]
            WriteString($"/Rotate 0");
            WriteString($"/MediaBox [0 0 {w} {h}]");
            WriteString($"/TrimBox [0 0 {w} {h}]");
            EndObject();
        }

        void WriteXRef()
        {
            var xrefpos = stream.Length;
            WriteString("xref");
            WriteString($"0 {objCount}");
            WriteString("0000000000 65535 f");
            for (int i = 0; i < objCount; i++)
                WriteString($"{ObjectPositions[i].ToString("D10")} 00000 n");
            WriteString("trailer");
            WriteString("<<");
            WriteString($"/Root {catalogId} 0 R");
            WriteString($"/Size {objCount}");
            WriteString(">>");
            WriteString("startxref");
            WriteString($"{xrefpos}");
            WriteString("%%EOF");

        }

        static readonly double cm = 4.902777777778;

        public void AddImage(string filename)
        {
            Bitmap src = new Bitmap(filename);
            var width = src.Width;
            var height = src.Height;
            Bitmap img = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            Graphics.FromImage(img).Clear(Color.Red);
            Graphics.FromImage(img).DrawImage(src, 0, 0, width, height);
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Jpeg);
            img.Save(filename + ".jpg");
            var arr = ms.ToArray();
            var len = arr.Length;

            BeginObject();
            WriteString("/ColorSpace /DeviceRGB");
            WriteString("/Subtype /Image");
            WriteString($"/Height {height}");
            WriteString("/Filter /DCTDecode");
            WriteString("/Type /XObject");
            WriteString("/DecodeParms");
            WriteString("<<\n/Quality 80\n>>");
            WriteString($"/Width {width}");
            WriteString("/BitsPerComponent 8");
            WriteString($"/Length {len}");
            WriteString(">>\nstream");
            stream.Write(ms.ToArray(), 0, len);
            WriteString("\nendstream");
            EndObject();
            var pageData = new PageData(objCount, width, height);
            Pages.Add(pageData);
            var _data = $"q\n{pageData.PageWidth.ToString("f7")} 0 0 {pageData.PageHeight.ToString("f7")} 0 0 cm\n/X0 Do\nQ\n";

            BeginObject();
            WriteString($"/Length {_data.Length}");
            WriteString(">>\nstream");
            WriteString(_data, false);
            WriteString("\nendstream");
            EndObject();
        }

        public void FinishDocument()
        {
            WriteCatalog();
            WritePagesHeader();
            for (int i = 0, cnt = Pages.Count; i < cnt; i++)
                WritePage(Pages[i]);
            WriteXRef();
        }

        public void SaveToFile(string path)
        {
            using (FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write))
                file.Write(stream.ToArray(), 0, stream.ToArray().Length);
        }

        class PageData
        {
            public PageData(int id, int w, int h)
            {
                orientation = (w >= h);
                objId = id;
                PageWidth = w / cm;
                PageHeight = h / cm;
            }
            public double PageWidth;
            public double PageHeight;
            public bool orientation = false; // false = portrait, true = landscape
            public int objId;
        }
    }
}