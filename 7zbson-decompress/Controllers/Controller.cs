using Microsoft.AspNetCore.Mvc;
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
using System.Net;
using System.Net.Http.Headers;

namespace _7zbson_decompress.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class Controller : ControllerBase
    {

        private readonly ILogger<Controller> _logger;

        public Controller(ILogger<Controller> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("decompress")]
        public string Get([FromQuery] string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            HttpClient http = new HttpClient();
            var data = http.GetAsync("https://assets.neos.com/assets/" + id.Replace("neosdb:///","")).Result.Content.ReadAsStream();

            return getJsonFrom7zbson(data);
        }

        [HttpPost]
        [Route("compress")]
        [Route("")]
        async public Task<FileContentResult> Post()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            using (MemoryStream str = new MemoryStream())
            {
                var input = await reader.ReadToEndAsync();
                get7zbsonFromJson(input, str);
                return File(str.ToArray(), "application/x-7z-compressed");
            }
        }

        private static int dictionary = 2097152;
        private static int posStateBits = 2;
        private static int litContextBits = 3;
        private static int litPosBits = 0;
        private static int algorithm = 2;
        private static int numFastBytes = 32;
        private static bool eos = false;
        private static CoderPropID[] propIDs = new CoderPropID[8]
        {
            CoderPropID.DictionarySize,
            CoderPropID.PosStateBits,
            CoderPropID.LitContextBits,
            CoderPropID.LitPosBits,
            CoderPropID.Algorithm,
            CoderPropID.NumFastBytes,
            CoderPropID.MatchFinder,
            CoderPropID.EndMarker
        };

        private static object[] properties = new object[8] { dictionary, posStateBits, litContextBits, litPosBits, algorithm, numFastBytes, "bt4", eos };

        public static void Compress(Stream inStream, Stream outStream)
        {
            SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
            encoder.SetCoderProperties(propIDs, properties);
            encoder.WriteCoderProperties(outStream);
            BinaryWriter binaryWriter = new BinaryWriter(outStream, Encoding.UTF8);
            binaryWriter.Write(inStream.Length - inStream.Position);
            long positionForCompressedSize = outStream.Position;
            binaryWriter.Write(0L);
            long positionForCompressedDataStart = outStream.Position;
            encoder.Code(inStream, outStream, -1L, -1L, null);
            long positionAfterCompression = outStream.Position;
            outStream.Position = positionForCompressedSize;
            binaryWriter.Write(positionAfterCompression - positionForCompressedDataStart);
            outStream.Position = positionAfterCompression;
        }

        private void Decompress(Stream inStream, Stream outStream)
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

        private void get7zbsonFromJson(String input, Stream outStr)
        {
            using (TextReader sr = new StringReader(input))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                reader.MaxDepth = 8192;
                using (MemoryStream s = new MemoryStream())
                using (BsonWriter bWriter = new BsonWriter(s))
                {
                    bWriter.WriteToken(reader);
                    s.Seek(0L, SeekOrigin.Begin);
                    Compress(s, outStr);
                }

            }

        }

        private string getJsonFrom7zbson(Stream str)
        {
            // string pathSource = @"C:\Users\kokoa\Desktop\Box.7zbson";
            using (MemoryStream outStream = new MemoryStream())
            {
                Decompress(str, outStream);
                outStream.Seek(0L, SeekOrigin.Begin);
                using (BsonReader reader = new BsonReader((Stream)outStream))
                {
                    reader.MaxDepth = 8192;
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