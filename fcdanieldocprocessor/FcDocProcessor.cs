using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Storage;

namespace Function
{
    public class FcDocProcessor
    {
        [FunctionName("FcDocProcessor")]
        //[return: Table("danieldocsinfo", Connection = "AzureWebJobsStorage")]
        [TableOutput("danieldocsinfo", Connection = "AzureWebJobsStorage")]
        public async Task<DocEntity> Run([QueueTrigger("queueprocess", Connection = "AzureWebJobsStorage")] string myQueueItem, ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            var queueItem = JsonSerializer.Deserialize<DocFile>(myQueueItem);

            var blobClient = new BlobClient(
                System.Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                "tobeprocess",
                queueItem.FileName
            );
            var currentBlob = await blobClient.DownloadStreamingAsync();

            var blobContainerClient = new BlobContainerClient(
                System.Environment.GetEnvironmentVariable("AzureWebJobsStorage"),
                "processdone"
            );

            await blobContainerClient.UploadBlobAsync(queueItem.FileName, currentBlob.Value.Content);

            await blobClient.DeleteIfExistsAsync();

            return new DocEntity
            {
                PartitionKey = "nota_fiscal",
                RowKey = Guid.NewGuid().ToString(),
                PersonGovId = queueItem.PersonGovId,
                PersonName = queueItem.PersonName
            };
        }
    }
}
