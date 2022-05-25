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
        public string Get([FromQuery] string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            HttpClient http = new HttpClient();
            var data = http.GetAsync("https://assets.neos.com/assets/" + id).Result.Content.ReadAsStream();

            return getJsonFrom7zbson(data);
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