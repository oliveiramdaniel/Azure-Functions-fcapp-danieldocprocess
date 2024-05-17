using System;
using System.IO;
using System.Text.Json;
using System.Xml;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Function
{
    [StorageAccount("AzureWebJobsStorage")] //Conection String from Storage Account
    public class FcDocTrigger
    {
        [FunctionName("FcDocTrigger")]
        [return: Queue("danielqueueprocess")]
        public string Run([BlobTrigger("danieltobeprocess/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var currentDoc = ProcessDocument(myBlob, name);

            return JsonSerializer.Serialize(currentDoc);
        }

        private static DocFile ProcessDocument(Stream blob, string name)
        {
            XmlDocument document = new XmlDocument();
            using(Stream stream = blob)
            {
                using(XmlReader reader = XmlReader.Create(stream))
                {
                    if(stream.Position > 0)
                    {
                        stream.Position = 0;
                    }
                    document.Load(stream);
                }
            }

            var cpfFromXml = document.SelectSingleNode("nota_fiscal/cpf").InnerText;
            var nomeFromXml = document.SelectSingleNode("nota_fiscal/nome").InnerText;

            var doc = new DocFile()
            {
                FileName = name,
                FileSize = blob.Length,
                PersonName = nomeFromXml,
                PersonGovId = cpfFromXml,
                ProcessDate = DateTime.UtcNow
            };

            return doc;
        }
    }
}
