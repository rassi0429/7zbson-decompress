using SevenZip;
using SevenZip.Compression.LZMA;
using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


class HogeClass
{
    public static void Decompress(Stream inStream, Stream outStream)
    {
        byte[] numArray = new byte[5];
        if (inStream.Read(numArray, 0, 5) != 5)
            throw new Exception("Input stream is too short.");
        SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
        decoder.SetDecoderProperties(numArray);
        BinaryReader binaryReader = new BinaryReader(inStream, Encoding.UTF8);
        long outSize = binaryReader.ReadInt64();
        long inSize = binaryReader.ReadInt64();
        decoder.Code(inStream, outStream, inSize, outSize, null);
    }

    public static string getJsonFrom7zbson(string str)
    {
        string pathSource = @"C:\Users\neo\GitHub\JS_7ZBSON_example\MultiTool.7zbson";
        using (FileStream inStream = new FileStream(pathSource, FileMode.Open, FileAccess.Read))
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                Decompress(inStream, outStream);
                outStream.Seek(0L, SeekOrigin.Begin);
                using (BsonReader reader = new BsonReader((Stream)outStream))
                {
                    var sb = new StringBuilder();
                    var sw = new StringWriter(sb);
                    using (var jWriter = new JsonTextWriter(sw))
                    {
                        jWriter.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                        jWriter.WriteToken(reader);
                    }
                    // Console.WriteLine(sb.ToString());
                    return sb.ToString();
                }
            }
        }
    }
}


