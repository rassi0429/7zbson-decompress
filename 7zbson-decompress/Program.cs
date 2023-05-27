


using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using SevenZip;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics.Metrics;


 int dictionary = 2097152;
 int posStateBits = 2;
 int litContextBits = 3;
 int litPosBits = 0;
 int algorithm = 2;
 int numFastBytes = 32;
 bool eos = false;
 CoderPropID[] propIDs = new CoderPropID[8]
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

object[] properties = new object[8] { dictionary, posStateBits, litContextBits, litPosBits, algorithm, numFastBytes, "bt4", eos };


void Compress(Stream inStream, Stream outStream)
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

void get7zbsonFromJson(String input, Stream outStr)
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


string pathSource = @"C:\Users\neo\GitHub\JS_7ZBSON_example\simple.json";
string path = @"C:\Users\neo\GitHub\JS_7ZBSON_example\Csimple.7zbson";
using (FileStream fs = File.OpenWrite(path))
using (FileStream inStream = new FileStream(pathSource, FileMode.Open, FileAccess.Read))
{
    using (TextReader sr = new StreamReader(inStream))
    using (JsonTextReader reader = new JsonTextReader(sr))
    {
        reader.MaxDepth = 8192;
        using (MemoryStream s = new MemoryStream())
        using (BsonWriter bWriter = new BsonWriter(s))
        {
            bWriter.WriteToken(reader);
            //s.Seek(0L, SeekOrigin.Begin);
            //// Read the remaining bytes, byte by byte.
            //while (s.Position < s.Length)
            //{
            //    Console.WriteLine(s.ReadByte());
            //}

            s.Seek(0L, SeekOrigin.Begin);

            Compress(s, fs);
        }

    }
}



//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();

//// builder.Services.AddSwaggerGen();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//    //app.UseSwagger();
//    //app.UseSwaggerUI();
//}

//// app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
