using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace Portal.Core.Compression
{
    public class GZip
    {
        public static byte[] Compress(byte[] byteArray)
        {
            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
            {
                gzipStream.Write(byteArray, 0, byteArray.Length);
            }

            return memoryStream.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            using var memoryStream = new MemoryStream(data);
            using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();

            gzipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        public static bool IsGzipped(byte[] data, int offset=0)
        {
            if (data == null || data.Length < offset + 2)
                return false;

            // GZip header magic number
            return data[offset + 0] == 0x1F && data[offset + 1] == 0x8B;
        }
    }
}
