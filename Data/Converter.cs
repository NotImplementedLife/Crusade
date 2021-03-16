using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Crusade.Files;

namespace Crusade.Data
{
    public static class Converter
    {
        public static readonly Dictionary<string, ConvertData> ConvertDictionary
            = new Dictionary<string, ConvertData>
            {
                //{ "WORD (not supported)", new ConvertData("Word document (*.docx) | *.docx", (f,o)=>{ }) },
                { "PDF",  new ConvertData("Portable Document Format (*.pdf) | *.pdf", ToPdf) },
                { "ZIP" , new ConvertData("ZIP Archive (*.zip) | *.zip", ToZip)}
            };        

        public static void ToPdf(string[] files,string outputPath)
        {
            var writer = new PdfWriter();
            for(int i=0,cnt=files.Length;i<cnt;i++)
            {
                writer.AddImage(files[i]);
            }
            writer.FinishDocument();
            writer.SaveToFile(outputPath);
        }

        public static void ToZip(string[] files, string outputPath)
        {
            var ms = new MemoryStream();
            List<ZIP_FileHeader> headers = new List<ZIP_FileHeader>();
            int offset = 0;
            using (BinaryWriter w = new BinaryWriter(ms))
            {
                int L = files.Length;
                for (int i = 0; i < L; i++)
                {                                       
                    byte[] filedata = File.ReadAllBytes(files[i]);
                    int size = filedata.Length;
                    string archivefilename = (i + 1).ToString("D3") + Path.GetExtension(files[i]);                    
                    var header = new ZIP_FileHeader(DateTime.Now, filedata.GetCRC32(), size, archivefilename);
                    header.Offset = offset;
                    offset += header.Length + size;
                    w.Write(header);
                    headers.Add(header);
                    w.Write(filedata);
                }
                int central_directory_size = 0;
                for(int i=0;i<L;i++)
                {
                    w.Write(headers[i], true);
                    central_directory_size += headers[i].CentralLength;
                }
                w.Write(new byte[] { 0x50, 0x4B, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00});
                w.Write((short)L);
                w.Write((short)L);
                w.Write(central_directory_size);
                w.Write(offset);
                w.Write(new byte[] { 0x00, 0x00 });

                w.Flush();
                w.Seek(0, SeekOrigin.Begin);
                w.Dispose();
            }
            //System.Windows.MessageBox.Show(BitConverter.ToString(ms.ToArray()));            

            using (FileStream file = new FileStream(outputPath, FileMode.Create, FileAccess.Write)) 
                file.Write(ms.ToArray(), 0, ms.ToArray().Length);                            
        }

        #region ZIP Data
        private class ZIP_FileHeader
        {
            internal static readonly byte[] Signature = { 0x50, 0x4B, 0x03, 0x04 };            
            internal static readonly byte[] Version = { 0x0A, 0x00 };
            internal static readonly byte[] Flags = { 0x00, 0x00 };
            internal static readonly byte[] Compression = { 0x00, 0x00 };
            internal static readonly byte[] Central_Signature = { 0x50, 0x4B, 0x01, 0x02 };
            internal static readonly byte[] Central_Version = { 0x1F, 0x00 };
            internal static readonly byte[] Central_VersionNeeded = { 0x0A, 0x00 };

            internal byte[] FileModificationDateTime;
            internal uint CRC32;
            internal int CompressedSize, UncompressedSize;
            internal string Filename;

            internal int Offset = 0;
            internal int Length;
            internal int CentralLength;
            internal ZIP_FileHeader(DateTime timestamp,uint crc32, int size,string filename)
            {
                FileModificationDateTime = timestamp.ToZipDateTime();
                CRC32 = crc32;
                CompressedSize = UncompressedSize = size;
                Filename = filename;
                Length = 30 + filename.Length;
                CentralLength = 46 + filename.Length + 36;
            }                      
        }

        private static void Write(this BinaryWriter w, ZIP_FileHeader h, bool isCentral = false)
        {
            if (!isCentral)
            {
                w.Write(ZIP_FileHeader.Signature);
                w.Write(ZIP_FileHeader.Version);
                w.Write(ZIP_FileHeader.Flags);
                w.Write(ZIP_FileHeader.Compression);
                w.Write(h.FileModificationDateTime);
                w.Write(h.CRC32);
                w.Write(h.CompressedSize);
                w.Write(h.UncompressedSize);
                w.Write((short)h.Filename.Length);
                w.Write(new byte[] { 0x00, 0x00 }); // no extra field
                for (int i = 0, L = h.Filename.Length; i < L; i++)
                    w.Write(h.Filename[i]);
            }
            else
            {
                w.Write(ZIP_FileHeader.Central_Signature);
                w.Write(ZIP_FileHeader.Central_Version);
                w.Write(ZIP_FileHeader.Central_VersionNeeded);
                w.Write(ZIP_FileHeader.Flags);
                w.Write(ZIP_FileHeader.Compression);
                w.Write(h.FileModificationDateTime);
                w.Write(h.CRC32);
                w.Write(h.CompressedSize);
                w.Write(h.UncompressedSize);
                w.Write((short)h.Filename.Length);
                w.Write(new byte[] { 0x24, 0x00 }); // extra field
                w.Write(new byte[] { 0x00, 0x00 }); // no file comm
                w.Write(new byte[] { 0x00, 0x00 }); // no disk # start
                w.Write(new byte[] { 0x00, 0x00 }); // no internal attr.
                w.Write(new byte[] { 0x20, 0x00, 0x00, 0x00 }); // external attr.
                w.Write(h.Offset); // offset of local header
                for (int i = 0, L = h.Filename.Length; i < L; i++)
                    w.Write(h.Filename[i]);
                w.Write(new byte[] { 0x0A, 0x00 });
                w.Write(new byte[] { 0x20, 0x00 });
                for (int i = 0; i < 32; i++) w.Write((byte)0x00); // extra field

            }
        }

        private static byte[] ToZipDateTime(this DateTime timestamp)
        {
            // Time
            int h = timestamp.Hour;
            int m = timestamp.Minute;
            int s = timestamp.Second / 2;
            short _t = (short)((h << 11) + (m << 5) + s);
            byte t1 = (byte)(_t >> 8);
            byte t0 = (byte)(_t - (t1 << 8));
            //Date
            int y = timestamp.Year - 1980;
            int M = timestamp.Month;
            int d = timestamp.Day;
            short _d = (short)((y << 9) + (M << 5) + d);
            byte d1 = (byte)(_d >> 8);
            byte d0 = (byte)(_d - (d1 << 8));

            return new byte[] { t0, t1, d0, d1 };
        }
        private static uint GetCRC32(this byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            for(int i=0,l=data.Length;i<l;i++)
            {
                crc ^= data[i];
                for(int q=0;q<8;q++)               
                    crc = (crc >> 1) ^ (0xEDB88320 & (uint)(-(crc & 1)));                
            }
            return ~crc;
        }
        #endregion

        public class ConvertData
        {
            public ConvertData(string filter, Action<string[], string> conv)
            {
                Filter = filter;
                Action = conv;
            }
            public string Filter;
            public Action<string[], string> Action;
        }
    }
}
